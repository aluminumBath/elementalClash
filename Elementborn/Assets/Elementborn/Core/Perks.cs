using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    public enum PerkId { Toughness, Scholar, Fortune }

    public sealed class PerkDef
    {
        public PerkId Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaxRank { get; }

        public PerkDef(PerkId id, string name, string description, int maxRank)
        {
            Id = id; Name = name; Description = description; MaxRank = maxRank < 1 ? 1 : maxRank;
        }
    }

    public static class PerkCatalog
    {
        public static IReadOnlyList<PerkDef> All { get; } = new List<PerkDef>
        {
            new PerkDef(PerkId.Toughness, "Toughness", "+20 max health per rank.", 5),
            new PerkDef(PerkId.Scholar, "Scholar", "+10% experience per rank.", 5),
            new PerkDef(PerkId.Fortune, "Fortune", "+15% currency from rewards per rank.", 5),
        };

        public static PerkDef Get(PerkId id)
        {
            foreach (var p in All) if (p.Id == id) return p;
            return null;
        }
    }

    /// <summary>
    /// Tracks perk ranks and unspent perk points, and aggregates the effects perks confer (read by the systems
    /// that apply them). You earn points by leveling and spend them to rank perks up to their cap. Pure —
    /// unit-tested.
    /// </summary>
    public sealed class PerkState
    {
        private readonly Dictionary<PerkId, int> _ranks = new Dictionary<PerkId, int>();
        public int AvailablePoints { get; private set; }
        public event Action Changed;

        public int RankOf(PerkId id) => _ranks.TryGetValue(id, out int r) ? r : 0;

        public bool CanRank(PerkId id)
        {
            var def = PerkCatalog.Get(id);
            return def != null && AvailablePoints > 0 && RankOf(id) < def.MaxRank;
        }

        public bool Rank(PerkId id)
        {
            if (!CanRank(id)) return false;
            _ranks[id] = RankOf(id) + 1;
            AvailablePoints--;
            Changed?.Invoke();
            return true;
        }

        public void Grant(int points)
        {
            if (points <= 0) return;
            AvailablePoints += points;
            Changed?.Invoke();
        }

        public void Restore(int availablePoints, IReadOnlyList<PerkId> ids, IReadOnlyList<int> ranks)
        {
            _ranks.Clear();
            AvailablePoints = availablePoints < 0 ? 0 : availablePoints;
            if (ids != null && ranks != null)
            {
                int n = Math.Min(ids.Count, ranks.Count);
                for (int i = 0; i < n; i++) _ranks[ids[i]] = ranks[i];
            }
            Changed?.Invoke();
        }

        // ---- aggregated effects ----
        public float BonusMaxHealth => RankOf(PerkId.Toughness) * 20f;
        public float XpMultiplier => 1f + RankOf(PerkId.Scholar) * 0.10f;
        public float RewardMultiplier => 1f + RankOf(PerkId.Fortune) * 0.15f;
    }
}
