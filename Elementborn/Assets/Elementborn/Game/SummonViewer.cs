using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The Summon Beacon panel (default key <b>U</b>; also on the VR hub). Shows your Sigils + Motes, lets you
    /// pick a banner, reads out pity, and rolls <b>×1</b> or <b>×10</b> through <see cref="SummonController"/> —
    /// new creatures join your roster, duplicates dust into Motes, and Motes can claim a banner's featured
    /// creature. A collection roster (owned vs. locked, by rarity) sits underneath. Built via
    /// <see cref="OverlayUi"/>, so it's world-space in VR. The bootstrap scene adds one.
    /// </summary>
    public sealed class SummonViewer : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.U;

        private static readonly Color Gold = new Color(1f, 0.82f, 0.25f);
        private static readonly Color Purple = new Color(0.74f, 0.56f, 1f);
        private static readonly Color Sky = new Color(0.5f, 0.8f, 1f);
        private static readonly Color Dim = new Color(0.55f, 0.55f, 0.62f);

        private Canvas _canvas;
        private Transform _content;
        private bool _open;
        private string _bannerId = SummonBannerCatalog.Standard.Id;
        private List<SummonResult> _lastResults;

        private void Awake() { Build(); Hide(); }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; if (_canvas != null) _canvas.gameObject.SetActive(true); Rebuild(); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Build()
        {
            var p = OverlayUi.Panel("SummonCanvas", Localization.T("ui.title.summon"), 56, new Vector2(760, 860), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var sc = SummonController.Instance;
            if (sc == null) { OverlayUi.Body(_content, "Summon system not ready."); return; }

            var cfg = sc.Config;
            var banner = sc.BannerForId(_bannerId);
            var state = sc.StateFor(banner.Id);

            OverlayUi.Header(_content, $"Sigils  {sc.Sigils}      Motes  {sc.Motes}");

            // Daily free summon (always the standard banner, independent of the selection below).
            if (sc.DailyAvailable)
            {
                UiTheme.Button(_content, Localization.T("summon.freeDaily"), DoDaily, 700, 48);
                OverlayUi.Body(_content, $"Day {sc.PendingStreakDay()} streak  \u00b7  +{sc.PendingStreakReward()} Sigils", 14, Dim);
            }
            else
            {
                OverlayUi.Body(_content, $"Next free summon in {sc.DailyResetEta()}", 16, Dim);
                OverlayUi.Body(_content, $"Login streak: {sc.LoginStreak} day(s)", 14, Dim);
            }

            // Banner selector (the featured banner is whatever is currently on rotation).
            foreach (var b in sc.Banners)
            {
                var bid = b.Id;
                string mark = b.Id == _bannerId ? "> " : "   ";
                string tag = b.HasFeature ? "  (featured: " + CreatureCatalog.For(b.Featured).Name + ")" : "";
                UiTheme.Button(_content, mark + b.Name + tag, () => { _bannerId = bid; _lastResults = null; Rebuild(); }, 700, 42);
            }

            // Pity readout.
            string pity = $"Legendary guaranteed within {state.PullsUntilPity(cfg)} pulls";
            if (banner.HasFeature && state.GuaranteedFeatured) pity += "   ·   next Legendary is the featured one";
            OverlayUi.Body(_content, pity, 18, Dim);

            if (banner.HasFeature)
                OverlayUi.Body(_content, $"Featured rotates in {sc.DaysUntilNextRotation()} day(s)", 16, Dim);

            // Pull buttons.
            UiTheme.Button(_content, $"Summon  x1   ({cfg.SingleCost} Sigils)", () => DoRoll(false), 700, 48);
            UiTheme.Button(_content, $"Summon  x10  ({cfg.TenCost} Sigils)", () => DoRoll(true), 700, 48);

            if (banner.HasFeature)
                UiTheme.Button(_content,
                    $"Claim {CreatureCatalog.For(banner.Featured).Name}  ({cfg.FeaturedExchangeCost} Motes)",
                    () => { SummonController.Instance?.ClaimFeatured(_bannerId); Rebuild(); }, 700, 44);

            // Last results.
            if (_lastResults != null && _lastResults.Count > 0)
            {
                OverlayUi.Header(_content, "Last summon");
                var pi = PlayerInventory.Instance;
                foreach (var r in _lastResults)
                {
                    string name = CreatureCatalog.For(r.Kind).Name;
                    string suffix = r.WonFeatured ? "  ★ featured!" : "";
                    OverlayUi.Body(_content, Stars(r.Rarity) + "  " + name + suffix, 19, ColorFor(r.Rarity));
                }
            }

            // Summon history (per-banner pulls + lifetime tally).
            BuildHistory(banner, state, sc);

            // Recent notable pulls (Epic+).
            BuildRecentPulls(sc);

            // Collection roster.
            BuildRoster(banner);
        }

        private void BuildRecentPulls(SummonController sc)
        {
            var hist = sc.History;
            if (hist.Count == 0) return;
            OverlayUi.Header(_content, "Recent pulls");
            int shown = 0;
            foreach (var e in hist.Recent)
            {
                if (shown++ >= 6) break;
                string feat = e.WonFeatured ? "  ★ featured" : "";
                OverlayUi.Body(_content,
                    $"{Stars(e.Rarity)}  {CreatureCatalog.For(e.Kind).Name}{feat}   ·   {e.BannerName}   ·   {sc.DescribeAge(e.UtcTicks)}",
                    16, ColorFor(e.Rarity));
            }
        }

        private void BuildHistory(SummonBanner banner, SummonState state, SummonController sc)
        {
            var st = sc.Stats;
            OverlayUi.Header(_content, "Summon history");
            OverlayUi.Body(_content, $"Pulls on this banner: {state.TotalPulls}", 17, Dim);
            OverlayUi.Body(_content, $"Lifetime pulls: {st.TotalPulls}", 17, Dim);
            OverlayUi.Body(_content,
                $"{Stars(SummonRarity.Legendary)} {st.LegendaryCount}    {Stars(SummonRarity.Epic)} {st.EpicCount}    {Stars(SummonRarity.Rare)} {st.RareCount}",
                17, Dim);
            if (banner.HasFeature || st.FeaturedWins > 0)
                OverlayUi.Body(_content, $"Featured pulled: {st.FeaturedWins}", 16, Dim);
            OverlayUi.Body(_content,
                $"Legendary rate {st.RateOf(SummonRarity.Legendary) * 100.0:0.0}%   ·   Sigils spent {st.SigilsSpent}   ·   Motes earned {st.MotesEarned}",
                15, Dim);
        }

        private void BuildRoster(SummonBanner banner)
        {
            var pi = PlayerInventory.Instance;
            int owned = 0, total = 0;
            foreach (var k in banner.AllKinds()) { total++; if (pi != null && pi.Owns(k)) owned++; }

            OverlayUi.Header(_content, $"Collection — {owned}/{total} creatures");
            Tier(banner.Legendary, SummonRarity.Legendary, pi);
            Tier(banner.Epic, SummonRarity.Epic, pi);
            Tier(banner.Rare, SummonRarity.Rare, pi);
        }

        private void Tier(IReadOnlyList<CreatureKind> pool, SummonRarity rarity, PlayerInventory pi)
        {
            var have = new List<string>();
            var lack = new List<string>();
            foreach (var k in pool)
            {
                string name = CreatureCatalog.For(k).Name;
                if (pi != null && pi.Owns(k)) have.Add(name); else lack.Add(name);
            }

            OverlayUi.Body(_content, $"{Stars(rarity)}  {RarityName(rarity)}   {have.Count}/{pool.Count}", 18, ColorFor(rarity));
            if (have.Count > 0) OverlayUi.Body(_content, "   " + Join(have), 17, ColorFor(rarity));
            if (lack.Count > 0) OverlayUi.Body(_content, "   locked: " + Join(lack), 16, Dim);
        }

        private void DoRoll(bool ten)
        {
            var sc = SummonController.Instance;
            if (sc == null) return;
            var outcome = sc.Roll(_bannerId, ten);
            if (outcome.HasValue) _lastResults = new List<SummonResult>(outcome.Value.Results);
            Rebuild();
        }

        private void DoDaily()
        {
            var sc = SummonController.Instance;
            if (sc == null) return;
            var outcome = sc.ClaimDailySummon();
            if (outcome.HasValue) _lastResults = new List<SummonResult>(outcome.Value.Results);
            Rebuild();
        }

        private static string Stars(SummonRarity r) =>
            r == SummonRarity.Legendary ? "★★★★★" : r == SummonRarity.Epic ? "★★★★" : "★★★";

        private static string RarityName(SummonRarity r) =>
            r == SummonRarity.Legendary ? "Legendary" : r == SummonRarity.Epic ? "Epic" : "Rare";

        private static Color ColorFor(SummonRarity r) =>
            r == SummonRarity.Legendary ? Gold : r == SummonRarity.Epic ? Purple : Sky;

        private static string Join(List<string> names)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < names.Count; i++) { if (i > 0) sb.Append(", "); sb.Append(names[i]); }
            return sb.ToString();
        }
    }
}
