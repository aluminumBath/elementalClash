using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>One possible drop: an item id, its relative weight within a roll, and the quantity range granted
    /// when picked. An empty <see cref="ItemId"/> is a "nothing" slot, so a roll can come up empty.</summary>
    public readonly struct LootEntry
    {
        public readonly string ItemId;
        public readonly int Weight;
        public readonly int MinCount;
        public readonly int MaxCount;

        public LootEntry(string itemId, int weight, int minCount, int maxCount)
        {
            ItemId = itemId;
            Weight = weight < 0 ? 0 : weight;
            MinCount = minCount < 1 ? 1 : minCount;
            MaxCount = maxCount < MinCount ? MinCount : maxCount;
        }

        public LootEntry(string itemId, int weight, int count) : this(itemId, weight, count, count) { }
    }

    /// <summary>A single dropped stack.</summary>
    public readonly struct LootDrop
    {
        public readonly string ItemId;
        public readonly int Count;
        public LootDrop(string itemId, int count) { ItemId = itemId; Count = count; }
    }

    /// <summary>
    /// A weighted drop table: each of <see cref="Rolls"/> picks selects one entry by weight and grants a quantity
    /// in its range; identical items stack and keep first-seen order. Deterministic given the
    /// <see cref="IRandomSource"/> (one unit for the pick, one for the quantity), so it's unit-tested with a
    /// scripted source.
    /// </summary>
    public sealed class LootTable
    {
        public IReadOnlyList<LootEntry> Entries { get; }
        public int Rolls { get; }

        public LootTable(int rolls, params LootEntry[] entries)
        {
            Rolls = rolls < 1 ? 1 : rolls;
            Entries = entries ?? new LootEntry[0];
        }

        public IReadOnlyList<LootDrop> Roll(IRandomSource rng)
        {
            int totalWeight = 0;
            foreach (var e in Entries) totalWeight += e.Weight;
            if (totalWeight <= 0 || rng == null) return new List<LootDrop>();

            var order = new List<string>();
            var counts = new Dictionary<string, int>();

            for (int i = 0; i < Rolls; i++)
            {
                var picked = Pick(rng, totalWeight);
                if (string.IsNullOrEmpty(picked.ItemId)) continue; // "nothing" slot
                int span = picked.MaxCount - picked.MinCount + 1;
                int count = picked.MinCount + (int)(rng.NextUnit() * span);
                if (count > picked.MaxCount) count = picked.MaxCount; // guard against NextUnit() == ~1.0
                if (!counts.ContainsKey(picked.ItemId)) order.Add(picked.ItemId);
                counts.TryGetValue(picked.ItemId, out int have);
                counts[picked.ItemId] = have + count;
            }

            var drops = new List<LootDrop>(order.Count);
            foreach (var id in order) drops.Add(new LootDrop(id, counts[id]));
            return drops;
        }

        private LootEntry Pick(IRandomSource rng, int totalWeight)
        {
            double r = rng.NextUnit() * totalWeight;
            double cum = 0;
            for (int i = 0; i < Entries.Count; i++)
            {
                cum += Entries[i].Weight;
                if (r < cum) return Entries[i];
            }
            return Entries[Entries.Count - 1]; // floating-point fallback
        }
    }

    /// <summary>The drop table for each creature, themed by element/habitat over a common beast baseline. Every
    /// item id here exists in <see cref="ItemCatalog"/> (a test asserts it).</summary>
    public static class LootTables
    {
        public static LootTable For(CreatureKind kind)
        {
            switch (kind)
            {
                case CreatureKind.FireDragon:
                case CreatureKind.Phoenix:
                    return Fire;

                case CreatureKind.WaterDragon:
                case CreatureKind.Mermaid:
                case CreatureKind.Eel:
                case CreatureKind.Crab:
                case CreatureKind.WaterCat:
                case CreatureKind.IceCat:
                case CreatureKind.Goldkoi:
                case CreatureKind.Skimfin:
                case CreatureKind.Gillcloak:
                case CreatureKind.Tidewarden:
                    return Water;

                case CreatureKind.EarthMole:
                case CreatureKind.EarthCat:
                case CreatureKind.Rhino:
                    return Earth;

                case CreatureKind.AirDragonfly:
                case CreatureKind.AirJellyfish:
                case CreatureKind.Roc:
                case CreatureKind.Thunderbird:
                case CreatureKind.Ridgewing:
                case CreatureKind.Glidewisp:
                case CreatureKind.Skytyrant:
                    return Air;

                case CreatureKind.Direstalker:
                    return Apex;

                default:
                    return Beast;
            }
        }

        // Common wildlife: mostly hide, the odd consumable, and a few low-value forage drops.
        private static readonly LootTable Beast = new LootTable(2,
            new LootEntry("hide", 60, 1, 2),
            new LootEntry("sunflower_seeds", 34, 1, 2),
            new LootEntry("healing_tonic", 6, 1));

        private static readonly LootTable Fire = new LootTable(2,
            new LootEntry("ember_shard", 45, 1, 2),
            new LootEntry("hide", 50, 1, 2),
            new LootEntry("old_relic", 5, 1));

        private static readonly LootTable Water = new LootTable(2,
            new LootEntry("river_pearl", 40, 1, 2),
            new LootEntry("deep_jelly", 40, 1, 2),
            new LootEntry("hide", 20, 1));

        private static readonly LootTable Earth = new LootTable(2,
            new LootEntry("ore_marrow_bone", 40, 1, 2),
            new LootEntry("compost_truffle", 40, 1),
            new LootEntry("hide", 20, 1, 2));

        private static readonly LootTable Air = new LootTable(2,
            new LootEntry("iridescent_beetle", 70, 1, 2),
            new LootEntry("hide", 30, 1));

        // Apex predator: more rolls, a real shot at a relic.
        private static readonly LootTable Apex = new LootTable(3,
            new LootEntry("old_relic", 25, 1),
            new LootEntry("ember_shard", 20, 1, 2),
            new LootEntry("hide", 55, 1, 2));
    }
}
