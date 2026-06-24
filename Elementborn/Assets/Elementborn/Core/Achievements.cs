using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>What an achievement counts. Each maps to an event on the <see cref="QuestEvents"/> bus.</summary>
    public enum AchievementMetric
    {
        CreaturesDefeated,
        CreaturesTamed,
        CreaturesSighted,
        AbilitiesCast,
        ItemsCollected,
        CurrencyEarned,
        QuestsCompleted,
        NpcsMet,
    }

    /// <summary>A single achievement: reach <see cref="Target"/> of <see cref="Metric"/>, optionally filtered to a
    /// <see cref="Param"/> qualifier (empty = any value of the metric).</summary>
    public readonly struct AchievementDef
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Description;
        public readonly AchievementMetric Metric;
        public readonly int Target;
        public readonly string Param;

        public AchievementDef(string id, string name, string description, AchievementMetric metric, int target, string param = "")
        {
            Id = id; Name = name; Description = description; Metric = metric;
            Target = target < 1 ? 1 : target; Param = param ?? "";
        }
    }

    public static class AchievementCatalog
    {
        public static IReadOnlyList<AchievementDef> All { get; } = new List<AchievementDef>
        {
            new AchievementDef("first_blood", "First Blood",    "Defeat your first creature.",    AchievementMetric.CreaturesDefeated, 1),
            new AchievementDef("hunter",      "Hunter",         "Defeat 25 creatures.",           AchievementMetric.CreaturesDefeated, 25),
            new AchievementDef("apex",        "Apex Predator",  "Defeat 100 creatures.",          AchievementMetric.CreaturesDefeated, 100),
            new AchievementDef("first_tame",  "Kindred",        "Tame your first creature.",      AchievementMetric.CreaturesTamed, 1),
            new AchievementDef("tamer",       "Tamer",          "Tame 5 creatures.",              AchievementMetric.CreaturesTamed, 5),
            new AchievementDef("beastmaster", "Beastmaster",    "Tame 20 creatures.",             AchievementMetric.CreaturesTamed, 20),
            new AchievementDef("naturalist",  "Naturalist",     "Spot 30 creatures in the wild.", AchievementMetric.CreaturesSighted, 30),
            new AchievementDef("channeler",   "Channeler",      "Cast 50 abilities.",             AchievementMetric.AbilitiesCast, 50),
            new AchievementDef("archmage",    "Archmage",       "Cast 500 abilities.",            AchievementMetric.AbilitiesCast, 500),
            new AchievementDef("collector",   "Collector",      "Collect 50 items.",              AchievementMetric.ItemsCollected, 50),
            new AchievementDef("coffers",     "Coffers",        "Earn 1000 silver.",              AchievementMetric.CurrencyEarned, 1000, "Silver"),
            new AchievementDef("questmaster", "Questmaster",    "Complete 10 quests.",            AchievementMetric.QuestsCompleted, 10),
            new AchievementDef("friendly",    "Friendly Face",  "Meet 10 people.",                AchievementMetric.NpcsMet, 10),
        };
    }

    /// <summary>
    /// Tracks progress toward achievements and which are unlocked. Counts are kept per (metric, qualifier): an
    /// event bumps both the "any" bucket and — when it carries a qualifier — the specific one, so broad and
    /// targeted achievements both advance. Pure and unit-tested; the runtime layer feeds it from the QuestEvents
    /// bus, celebrates the unlocks it returns, and persists the counts.
    /// </summary>
    public sealed class AchievementProgress
    {
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

        private static string Key(AchievementMetric metric, string param) => (int)metric + "|" + (param ?? "");

        public int CountFor(AchievementMetric metric, string param = "")
            => _counts.TryGetValue(Key(metric, param), out int n) ? n : 0;

        public bool IsUnlocked(AchievementDef a) => CountFor(a.Metric, a.Param) >= a.Target;

        /// <summary>Current progress toward an achievement, clamped to its target.</summary>
        public int Progress(AchievementDef a)
        {
            int c = CountFor(a.Metric, a.Param);
            return c > a.Target ? a.Target : c;
        }

        public int UnlockedCount
        {
            get { int n = 0; foreach (var a in AchievementCatalog.All) if (IsUnlocked(a)) n++; return n; }
        }

        /// <summary>Apply an event. Returns the achievements that crossed their target on THIS call (to celebrate).</summary>
        public IReadOnlyList<AchievementDef> Record(AchievementMetric metric, int amount, string param = "")
        {
            if (amount <= 0) return System.Array.Empty<AchievementDef>();

            var before = new HashSet<string>();
            foreach (var a in AchievementCatalog.All) if (IsUnlocked(a)) before.Add(a.Id);

            Bump(Key(metric, ""), amount);
            if (!string.IsNullOrEmpty(param)) Bump(Key(metric, param), amount);

            List<AchievementDef> unlocked = null;
            foreach (var a in AchievementCatalog.All)
                if (!before.Contains(a.Id) && IsUnlocked(a))
                {
                    if (unlocked == null) unlocked = new List<AchievementDef>();
                    unlocked.Add(a);
                }
            return unlocked != null ? (IReadOnlyList<AchievementDef>)unlocked : System.Array.Empty<AchievementDef>();
        }

        private void Bump(string key, int amount)
        {
            _counts.TryGetValue(key, out int have);
            _counts[key] = have + amount;
        }

        public IReadOnlyDictionary<string, int> ToSave() => new Dictionary<string, int>(_counts);

        public void LoadFrom(IReadOnlyDictionary<string, int> data)
        {
            _counts.Clear();
            if (data == null) return;
            foreach (var kv in data) _counts[kv.Key] = kv.Value;
        }
    }
}
