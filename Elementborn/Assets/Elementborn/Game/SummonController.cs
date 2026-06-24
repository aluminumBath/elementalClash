using System;
using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Runtime owner of the summon loop. Holds the two summon resources — <b>Sigils</b> (the premium pull
    /// currency, seeded at a fresh start and topped up on level-up) and <b>Motes</b> (duplicate dust) — plus a
    /// per-banner <see cref="SummonState"/> (pity). Rolling spends Sigils, runs the pure <see cref="SummonRoller"/>,
    /// then grants new creatures into <see cref="PlayerInventory"/> (so the companion/mount summoners can use them)
    /// or refunds Motes for duplicates. Motes can claim a banner's featured creature outright. State persists via
    /// the same CaptureInto/RestoreFrom pattern the other subsystems use. The <see cref="SummonViewer"/> renders it.
    /// </summary>
    public sealed class SummonController : MonoBehaviour
    {
        public static SummonController Instance { get; private set; }

        [SerializeField] private int starterSigils = 1600;   // enough for one ten-pull at a fresh start
        [SerializeField] private int sigilsPerLevel = 60;
        [SerializeField] private int rotationPeriodDays = 7;  // the featured banner rotates every N days

        private readonly SummonConfig _config = SummonConfig.Default;
        private readonly Dictionary<string, SummonState> _states = new Dictionary<string, SummonState>();
        private IRandomSource _rng = new SystemRandomSource();
        private Func<DateTime> _utcNow = () => DateTime.UtcNow;   // clock seam (tests can override)

        // Daily free summon: when it was last claimed (UTC) and whether it ever has been.
        private DateTime _dailyLastClaimUtc;
        private bool _dailyEverClaimed;
        private int _loginStreak;   // consecutive-day claim streak

        // A fixed UTC anchor for the rotation schedule, so "which featured banner" is identical on every client.
        private static readonly DateTime RotationEpochUtc = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private int _sigils;
        private int _motes;
        private bool _seeded;
        private Progression _progression;   // the instance we subscribe to, owned by ProgressionController
        private readonly SummonStats _stats = new SummonStats();
        private readonly SummonHistory _history = new SummonHistory();

        /// <summary>Raised whenever Sigils, Motes, or any banner's pity changes — the viewer re-reads on this.</summary>
        public event Action Changed;

        public SummonConfig Config => _config;
        public int Sigils => _sigils;
        public int Motes => _motes;
        public SummonStats Stats => _stats;
        public SummonHistory History => _history;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            // Fresh game (no save loaded): grant the one-time starter so the Beacon is playable immediately.
            // A loaded game arrives with _seeded already true via RestoreFrom, so this is a no-op then.
            if (!_seeded)
            {
                _seeded = true;
                _sigils += starterSigils;
                Changed?.Invoke();
            }

            // LeveledUp is an instance event on the Progression that ProgressionController owns (not static),
            // so subscribe here in Start once every Awake has run and its singleton exists. Top up on level-up.
            var pc = ProgressionController.Instance;
            if (pc != null)
            {
                _progression = pc.Progression;
                _progression.LeveledUp += OnLeveledUp;
            }
        }

        private void OnDestroy()
        {
            if (_progression != null) _progression.LeveledUp -= OnLeveledUp;
            if (Instance == this) Instance = null;
        }

        private void OnLeveledUp(int levels)
        {
            if (levels <= 0 || sigilsPerLevel <= 0) return;
            int gain = sigilsPerLevel * levels;
            _sigils += gain;
            Changed?.Invoke();
            GameHud.Instance?.Toast($"+{gain} Sigils");
        }

        /// <summary>Test/debug seam for deterministic rolls.</summary>
        public void SetRandomSource(IRandomSource rng) { if (rng != null) _rng = rng; }

        /// <summary>Test/debug seam for the rotation clock (defaults to the system UTC clock).</summary>
        public void SetClock(Func<DateTime> utcNow) { if (utcNow != null) _utcNow = utcNow; }

        /// <summary>Award Sigils from elsewhere (quest rewards, etc.).</summary>
        public void AddSigils(int amount)
        {
            if (amount <= 0) return;
            _sigils += amount;
            Changed?.Invoke();
        }

        public SummonState StateFor(string bannerId)
        {
            if (!_states.TryGetValue(bannerId, out var s)) { s = new SummonState(); _states[bannerId] = s; }
            return s;
        }

        public bool CanAfford(bool ten) => _sigils >= _config.CostFor(ten);

        // --- featured-banner rotation ---
        /// <summary>The current rotation period: whole <c>rotationPeriodDays</c> windows since the fixed epoch.</summary>
        public int CurrentPeriod() =>
            SummonBannerCatalog.PeriodFor(_utcNow(), RotationEpochUtc, Mathf.Max(1, rotationPeriodDays));

        /// <summary>The featured banner currently on rotation. Its creature changes each period; the slot id is
        /// stable, so pity and the lost-50/50 guarantee carry across rotations.</summary>
        public SummonBanner CurrentFeatured() => SummonBannerCatalog.FeaturedForPeriod(CurrentPeriod());

        /// <summary>The banners to show right now: the standard banner plus the current featured.</summary>
        public IReadOnlyList<SummonBanner> Banners => new[] { SummonBannerCatalog.Standard, CurrentFeatured() };

        /// <summary>Resolve a banner id to the banner to roll on (the featured slot maps to the current rotation).</summary>
        public SummonBanner BannerForId(string id) =>
            id == SummonBannerCatalog.FeaturedSlotId ? CurrentFeatured() : SummonBannerCatalog.Standard;

        /// <summary>Whole days left in the current rotation window (for the viewer's countdown).</summary>
        public int DaysUntilNextRotation()
        {
            int days = Mathf.Max(1, rotationPeriodDays);
            DateTime windowStart = RotationEpochUtc.AddDays((double)CurrentPeriod() * days);
            double remaining = (windowStart.AddDays(days) - _utcNow()).TotalDays;
            return Mathf.Max(0, (int)Math.Ceiling(remaining));
        }

        /// <summary>A short human age ("just now", "5m ago", "2h ago", "3d ago") for a recent-pull timestamp.</summary>
        public string DescribeAge(long utcTicks)
        {
            TimeSpan span = _utcNow() - new DateTime(utcTicks, DateTimeKind.Utc);
            if (span.Ticks < 0 || span.TotalMinutes < 1) return "just now";
            if (span.TotalHours < 1) return (int)span.TotalMinutes + "m ago";
            if (span.TotalDays < 1) return (int)span.TotalHours + "h ago";
            return (int)span.TotalDays + "d ago";
        }

        /// <summary>The cumulative result of a roll, for the viewer's summary line + toasts.</summary>
        public readonly struct RollOutcome
        {
            public readonly IReadOnlyList<SummonResult> Results;
            public readonly int NewCount;
            public readonly int MotesGained;
            public readonly bool AnyLegendary;

            public RollOutcome(IReadOnlyList<SummonResult> results, int newCount, int motesGained, bool anyLegendary)
            {
                Results = results;
                NewCount = newCount;
                MotesGained = motesGained;
                AnyLegendary = anyLegendary;
            }
        }

        /// <summary>Spend Sigils and roll one or ten pulls on the given banner. Returns null if you can't afford it.</summary>
        public RollOutcome? Roll(string bannerId, bool ten)
        {
            if (!CanAfford(ten))
            {
                GameHud.Instance?.Toast("Not enough Sigils");
                AudioController.Instance?.Back();
                return null;
            }

            var banner = BannerForId(bannerId);
            var state = StateFor(banner.Id);
            int cost = _config.CostFor(ten);
            _sigils -= cost;
            return ResolvePull(banner, state, ten, cost, ten ? "Ten-pull" : "Summon");
        }

        /// <summary>
        /// Shared roll resolution used by paid pulls and the free daily summon: rolls, grants/refunds, logs notable
        /// pulls, plays audio, updates stats (spending <paramref name="spentSigils"/>, 0 for free), and fires the
        /// summon quest hook. Assumes any Sigil cost has already been deducted.
        /// </summary>
        private RollOutcome ResolvePull(SummonBanner banner, SummonState state, bool ten, int spentSigils, string label)
        {
            var results = SummonRoller.PullMany(banner, state, _config, _rng, ten ? _config.TenPullSize : 1);

            int newCount = 0, motesGained = 0;
            SummonRarity best = SummonRarity.Rare;
            var pi = PlayerInventory.Instance;
            foreach (var r in results)
            {
                if ((int)r.Rarity > (int)best) best = r.Rarity;
                bool isNew = pi != null && pi.GrantOwned(r.Kind);
                if (isNew) newCount++;
                else { int refund = _config.RefundFor(r.Rarity); _motes += refund; motesGained += refund; }

                // Log notable pulls (Epic or better) for the recent-pulls history.
                if ((int)r.Rarity >= (int)SummonRarity.Epic)
                    _history.Record(new SummonHistoryEntry(r.Kind, r.Rarity, r.WonFeatured, banner.Name, _utcNow().Ticks));
            }
            bool anyLeg = best == SummonRarity.Legendary;

            // The Beacon's own sounds: a "cast" whoosh, then the reveal sting for the batch's best tier.
            var audio = AudioController.Instance;
            if (audio != null) { audio.SummonPull(); audio.SummonReveal(best); }

            // Lifetime history.
            if (spentSigils > 0) _stats.RecordSpend(spentSigils);
            _stats.RecordResults(results);
            _stats.RecordMotes(motesGained);

            // Tutorial/quest hook: one summon performed (a x1 or x10 both count once).
            QuestEvents.RaiseSummonPerformed(results.Count > 0 ? results[0].Kind.ToString() : "");

            GameHud.Instance?.Toast($"{label}: {newCount} new" + (motesGained > 0 ? $", +{motesGained} Motes" : ""));
            Changed?.Invoke();
            return new RollOutcome(results, newCount, motesGained, anyLeg);
        }

        /// <summary>Spend Motes to claim a banner's featured creature outright (the guaranteed floor).</summary>
        /// <summary>Is the once-per-day free summon available right now?</summary>
        public bool DailyAvailable => DailySummon.IsAvailable(_dailyLastClaimUtc, _utcNow(), _dailyEverClaimed);

        /// <summary>Short countdown to the next free summon ("3h 20m" / "12m"), for the viewer.</summary>
        public string DailyResetEta()
        {
            TimeSpan t = DailySummon.UntilReset(_utcNow());
            int h = (int)t.TotalHours;
            return h > 0 ? $"{h}h {t.Minutes}m" : $"{t.Minutes}m";
        }

        /// <summary>The current consecutive-day login streak (the last claimed day count).</summary>
        public int LoginStreak => _loginStreak;

        /// <summary>The streak day claiming right now would land on (continues, or resets to 1 if a day lapsed).</summary>
        public int PendingStreakDay() =>
            Elementborn.Core.LoginStreak.NextStreak(_dailyLastClaimUtc, _utcNow(), _loginStreak, _dailyEverClaimed);

        /// <summary>The streak Sigil bonus claiming right now would pay out.</summary>
        public int PendingStreakReward() => Elementborn.Core.LoginStreak.RewardFor(PendingStreakDay());

        /// <summary>Claim the free daily summon: a streak Sigil bonus plus one no-cost pull on the standard banner.
        /// Null if not available yet.</summary>
        public RollOutcome? ClaimDailySummon()
        {
            if (!DailyAvailable)
            {
                GameHud.Instance?.Toast("Daily summon not ready");
                AudioController.Instance?.Back();
                return null;
            }

            // Advance the login streak (uses the previous claim time, so compute before overwriting it).
            int streak = Elementborn.Core.LoginStreak.NextStreak(_dailyLastClaimUtc, _utcNow(), _loginStreak, _dailyEverClaimed);
            int streakBonus = Elementborn.Core.LoginStreak.RewardFor(streak);
            _loginStreak = streak;
            _sigils += streakBonus;

            _dailyLastClaimUtc = _utcNow();
            _dailyEverClaimed = true;
            GameHud.Instance?.Toast($"Day {streak} streak  ·  +{streakBonus} Sigils");

            var banner = BannerForId(SummonBannerCatalog.Standard.Id);
            var state = StateFor(banner.Id);
            return ResolvePull(banner, state, false, 0, "Free summon");
        }

        public bool ClaimFeatured(string bannerId)
        {
            var banner = BannerForId(bannerId);
            if (!banner.HasFeature) return false;

            var pi = PlayerInventory.Instance;
            if (pi != null && pi.Owns(banner.Featured))
            {
                GameHud.Instance?.Toast("Already collected");
                AudioController.Instance?.Back();
                return false;
            }
            if (_motes < _config.FeaturedExchangeCost)
            {
                GameHud.Instance?.Toast("Not enough Motes");
                AudioController.Instance?.Back();
                return false;
            }

            _motes -= _config.FeaturedExchangeCost;
            pi?.GrantOwned(banner.Featured);
            GameHud.Instance?.Toast($"Claimed {CreatureCatalog.For(banner.Featured).Name}");
            AudioController.Instance?.SummonReveal(SummonRarity.Legendary);
            QuestEvents.RaiseFeaturedClaimed(banner.Featured.ToString());
            Changed?.Invoke();
            return true;
        }

        // --- persistence (mirrors the other subsystems) ---
        public void CaptureInto(SaveData data)
        {
            if (data == null) return;
            data.summonSigils = _sigils;
            data.summonMotes = _motes;
            data.summonSeeded = _seeded;

            data.summonBannerIds.Clear();
            data.summonPity.Clear();
            data.summonGuaranteed.Clear();
            data.summonTotalPulls.Clear();
            foreach (var kv in _states)
            {
                data.summonBannerIds.Add(kv.Key);
                data.summonPity.Add(kv.Value.PityCounter);
                data.summonGuaranteed.Add(kv.Value.GuaranteedFeatured ? 1 : 0);
                data.summonTotalPulls.Add(kv.Value.TotalPulls);
            }

            data.summonStatPulls = _stats.TotalPulls;
            data.summonStatRare = _stats.RareCount;
            data.summonStatEpic = _stats.EpicCount;
            data.summonStatLegendary = _stats.LegendaryCount;
            data.summonStatFeaturedWins = _stats.FeaturedWins;
            data.summonStatSigilsSpent = _stats.SigilsSpent;
            data.summonStatMotesEarned = _stats.MotesEarned;

            data.summonHistKinds.Clear();
            data.summonHistRarity.Clear();
            data.summonHistFeatured.Clear();
            data.summonHistBanner.Clear();
            data.summonHistTicks.Clear();
            foreach (var e in _history.Recent)
            {
                data.summonHistKinds.Add(e.Kind.ToString());
                data.summonHistRarity.Add((int)e.Rarity);
                data.summonHistFeatured.Add(e.WonFeatured ? 1 : 0);
                data.summonHistBanner.Add(e.BannerName);
                data.summonHistTicks.Add(e.UtcTicks.ToString());
            }

            data.summonDailyClaimed = _dailyEverClaimed;
            data.summonDailyTicks = _dailyLastClaimUtc.Ticks.ToString();
            data.summonLoginStreak = _loginStreak;
        }

        public void RestoreFrom(SaveData data)
        {
            if (data == null) return;
            _sigils = data.summonSigils;
            _motes = data.summonMotes;
            _seeded = data.summonSeeded;

            _states.Clear();
            int n = Mathf.Min(data.summonBannerIds.Count,
                     Mathf.Min(data.summonPity.Count, Mathf.Min(data.summonGuaranteed.Count, data.summonTotalPulls.Count)));
            for (int i = 0; i < n; i++)
            {
                _states[data.summonBannerIds[i]] = new SummonState
                {
                    PityCounter = data.summonPity[i],
                    GuaranteedFeatured = data.summonGuaranteed[i] != 0,
                    TotalPulls = data.summonTotalPulls[i]
                };
            }

            _stats.LoadFrom(data.summonStatPulls, data.summonStatRare, data.summonStatEpic, data.summonStatLegendary,
                data.summonStatFeaturedWins, data.summonStatSigilsSpent, data.summonStatMotesEarned);

            var hist = new List<SummonHistoryEntry>();
            int hn = Mathf.Min(data.summonHistKinds.Count,
                     Mathf.Min(data.summonHistRarity.Count,
                     Mathf.Min(data.summonHistFeatured.Count,
                     Mathf.Min(data.summonHistBanner.Count, data.summonHistTicks.Count))));
            for (int i = 0; i < hn; i++)
            {
                if (!System.Enum.TryParse(data.summonHistKinds[i], out CreatureKind kind)) continue;
                long.TryParse(data.summonHistTicks[i], out long ticks);
                int rar = data.summonHistRarity[i];
                if (rar < 0) rar = 0;
                if (rar > (int)SummonRarity.Legendary) rar = (int)SummonRarity.Legendary;
                hist.Add(new SummonHistoryEntry(kind, (SummonRarity)rar, data.summonHistFeatured[i] != 0,
                    data.summonHistBanner[i], ticks));
            }
            _history.LoadFrom(hist);

            _dailyEverClaimed = data.summonDailyClaimed;
            long.TryParse(data.summonDailyTicks, out long dailyTicks);
            _dailyLastClaimUtc = dailyTicks > 0 ? new DateTime(dailyTicks, DateTimeKind.Utc) : default;
            _loginStreak = data.summonLoginStreak;
            Changed?.Invoke();
        }
    }
}
