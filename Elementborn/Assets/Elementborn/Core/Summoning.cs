using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>Pull tiers, low to high. Drives drop colours, refunds, and pity.</summary>
    public enum SummonRarity { Rare = 0, Epic = 1, Legendary = 2 }

    /// <summary>One creature obtained from a summon, tagged with the tier it came in at.</summary>
    public readonly struct SummonResult
    {
        public readonly CreatureKind Kind;
        public readonly SummonRarity Rarity;
        public readonly bool WonFeatured; // a Legendary that matched the banner's rate-up
        public readonly bool PityForced;  // the hard-pity guarantee produced this Legendary

        public SummonResult(CreatureKind kind, SummonRarity rarity, bool wonFeatured, bool pityForced)
        {
            Kind = kind;
            Rarity = rarity;
            WonFeatured = wonFeatured;
            PityForced = pityForced;
        }
    }

    /// <summary>
    /// A summon pool: three rarity tiers of creatures, optionally with one rate-up "featured" Legendary.
    /// The pools are existing <see cref="CreatureKind"/>s (companions + rideable mounts) so anything summoned
    /// drops straight into the player's roster and is usable by the companion/mount summoners.
    /// </summary>
    public sealed class SummonBanner
    {
        public string Id { get; }
        public string Name { get; }
        public bool HasFeature { get; }
        public CreatureKind Featured { get; }
        public IReadOnlyList<CreatureKind> Rare { get; }
        public IReadOnlyList<CreatureKind> Epic { get; }
        public IReadOnlyList<CreatureKind> Legendary { get; }

        public SummonBanner(string id, string name,
            IReadOnlyList<CreatureKind> rare, IReadOnlyList<CreatureKind> epic, IReadOnlyList<CreatureKind> legendary,
            bool hasFeature = false, CreatureKind featured = default)
        {
            Id = id;
            Name = name;
            Rare = rare ?? Array.Empty<CreatureKind>();
            Epic = epic ?? Array.Empty<CreatureKind>();
            Legendary = legendary ?? Array.Empty<CreatureKind>();
            HasFeature = hasFeature;
            Featured = featured;
        }

        public IReadOnlyList<CreatureKind> Pool(SummonRarity r) =>
            r == SummonRarity.Legendary ? Legendary : r == SummonRarity.Epic ? Epic : Rare;

        /// <summary>Every creature this banner can yield, highest tier first (for the collection roster).</summary>
        public IEnumerable<CreatureKind> AllKinds()
        {
            foreach (var k in Legendary) yield return k;
            foreach (var k in Epic) yield return k;
            foreach (var k in Rare) yield return k;
        }
    }

    /// <summary>
    /// Tunable summon economy + rates. A single pull costs <see cref="SingleCost"/>; a ten-pull guarantees at
    /// least one Epic. Pity guarantees a Legendary by <see cref="HardPity"/> pulls. A lost featured 50/50 makes
    /// the next Legendary a guaranteed featured. Duplicates refund Motes by tier; Motes can claim the featured
    /// creature outright via <see cref="FeaturedExchangeCost"/> (the "spark" floor).
    /// </summary>
    public sealed class SummonConfig
    {
        public int SingleCost { get; set; } = 160;
        public int TenCost { get; set; } = 1600;
        public double LegendaryChance { get; set; } = 0.006;
        public double EpicChance { get; set; } = 0.051;
        public int HardPity { get; set; } = 80;
        public double FeaturedChance { get; set; } = 0.5;
        public int TenPullSize { get; set; } = 10;

        /// <summary>Motes refunded when a pull duplicates an owned creature, by tier.</summary>
        public int RefundRare { get; set; } = 1;
        public int RefundEpic { get; set; } = 5;
        public int RefundLegendary { get; set; } = 25;

        /// <summary>Motes to claim the active banner's featured creature outright.</summary>
        public int FeaturedExchangeCost { get; set; } = 100;

        public int RefundFor(SummonRarity r) =>
            r == SummonRarity.Legendary ? RefundLegendary : r == SummonRarity.Epic ? RefundEpic : RefundRare;

        public int CostFor(bool ten) => ten ? TenCost : SingleCost;

        public static SummonConfig Default => new SummonConfig();
    }

    /// <summary>Per-banner pull memory: the pity counter and whether the next Legendary is a guaranteed featured.</summary>
    public sealed class SummonState
    {
        public int PityCounter { get; set; }        // pulls since the last Legendary
        public bool GuaranteedFeatured { get; set; } // set after losing a featured 50/50
        public int TotalPulls { get; set; }

        public int PullsUntilPity(SummonConfig cfg) => Math.Max(0, (cfg?.HardPity ?? 0) - PityCounter);
    }

    /// <summary>
    /// Pure, seedable summon resolver. No Unity, no I/O, no currency — the controller owns spending and
    /// persistence. Given a banner, mutable <see cref="SummonState"/>, config, and an <see cref="IRandomSource"/>,
    /// it rolls a tier (honouring hard pity), then a creature (honouring the featured 50/50).
    /// </summary>
    public static class SummonRoller
    {
        public static SummonResult Pull(SummonBanner banner, SummonState state, SummonConfig cfg, IRandomSource rng)
        {
            if (banner == null) throw new ArgumentNullException(nameof(banner));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            state.TotalPulls++;
            state.PityCounter++;

            SummonRarity rarity;
            bool forced = false;
            if (state.PityCounter >= cfg.HardPity)
            {
                rarity = SummonRarity.Legendary;
                forced = true;
            }
            else
            {
                double r = rng.NextUnit();
                if (r < cfg.LegendaryChance) rarity = SummonRarity.Legendary;
                else if (r < cfg.LegendaryChance + cfg.EpicChance) rarity = SummonRarity.Epic;
                else rarity = SummonRarity.Rare;
            }

            if (rarity == SummonRarity.Legendary) state.PityCounter = 0;

            CreatureKind kind;
            bool wonFeatured = false;
            if (rarity == SummonRarity.Legendary && banner.HasFeature)
            {
                if (state.GuaranteedFeatured)
                {
                    kind = banner.Featured;
                    wonFeatured = true;
                    state.GuaranteedFeatured = false;
                }
                else if (rng.NextUnit() < cfg.FeaturedChance)
                {
                    kind = banner.Featured;
                    wonFeatured = true;
                }
                else
                {
                    kind = PickExcluding(banner.Legendary, banner.Featured, rng);
                    state.GuaranteedFeatured = true; // lost the 50/50 -> next Legendary is guaranteed featured
                }
            }
            else
            {
                kind = Pick(banner.Pool(rarity), rng);
            }

            return new SummonResult(kind, rarity, wonFeatured, forced);
        }

        /// <summary>Rolls <paramref name="count"/> pulls in sequence, then enforces the ten-pull floor (at least one
        /// Epic-or-better) by bumping the last result if the whole batch came up Rare.</summary>
        public static List<SummonResult> PullMany(SummonBanner banner, SummonState state, SummonConfig cfg, IRandomSource rng, int count)
        {
            if (count < 1) count = 1;
            var results = new List<SummonResult>(count);
            SummonRarity best = SummonRarity.Rare;

            for (int i = 0; i < count; i++)
            {
                var res = Pull(banner, state, cfg, rng);
                if ((int)res.Rarity > (int)best) best = res.Rarity;
                results.Add(res);
            }

            if (count >= cfg.TenPullSize && best < SummonRarity.Epic && results.Count > 0)
                results[results.Count - 1] = new SummonResult(Pick(banner.Epic, rng), SummonRarity.Epic, false, false);

            return results;
        }

        private static CreatureKind Pick(IReadOnlyList<CreatureKind> pool, IRandomSource rng)
        {
            if (pool == null || pool.Count == 0) return default;
            int i = (int)(rng.NextUnit() * pool.Count);
            if (i >= pool.Count) i = pool.Count - 1;
            if (i < 0) i = 0;
            return pool[i];
        }

        private static CreatureKind PickExcluding(IReadOnlyList<CreatureKind> pool, CreatureKind excluded, IRandomSource rng)
        {
            if (pool == null || pool.Count == 0) return excluded;
            var subset = new List<CreatureKind>(pool.Count);
            foreach (var k in pool) if (!k.Equals(excluded)) subset.Add(k);
            if (subset.Count == 0) return excluded; // featured is the only Legendary -> can't avoid it
            return Pick(subset, rng);
        }
    }
}
