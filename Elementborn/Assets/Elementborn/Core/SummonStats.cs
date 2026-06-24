using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// A lifetime summon tally across all banners: how many pulls, how many of each tier, featured wins, and the
    /// Sigils spent / Motes earned. Pure and purely additive (no Unity, no I/O), so it's unit-tested; the
    /// <see cref="SummonController"/> feeds it every pull and persists it. Drives the Beacon's history readout.
    /// </summary>
    public sealed class SummonStats
    {
        public int TotalPulls { get; private set; }
        public int RareCount { get; private set; }
        public int EpicCount { get; private set; }
        public int LegendaryCount { get; private set; }
        public int FeaturedWins { get; private set; }   // Legendaries that matched a banner's rate-up
        public int SigilsSpent { get; private set; }
        public int MotesEarned { get; private set; }

        /// <summary>Tally one pull's result.</summary>
        public void RecordResult(SummonResult r)
        {
            TotalPulls++;
            switch (r.Rarity)
            {
                case SummonRarity.Legendary: LegendaryCount++; break;
                case SummonRarity.Epic:      EpicCount++; break;
                default:                     RareCount++; break;
            }
            if (r.WonFeatured) FeaturedWins++;
        }

        public void RecordResults(IEnumerable<SummonResult> results)
        {
            if (results == null) return;
            foreach (var r in results) RecordResult(r);
        }

        public void RecordSpend(int sigils) { if (sigils > 0) SigilsSpent += sigils; }
        public void RecordMotes(int motes) { if (motes > 0) MotesEarned += motes; }

        public int CountOf(SummonRarity r) =>
            r == SummonRarity.Legendary ? LegendaryCount : r == SummonRarity.Epic ? EpicCount : RareCount;

        /// <summary>The observed share of a tier across all pulls (0 if none yet). For display, not balancing.</summary>
        public double RateOf(SummonRarity r) => TotalPulls > 0 ? (double)CountOf(r) / TotalPulls : 0.0;

        /// <summary>Restore from a save (each field clamped to non-negative).</summary>
        public void LoadFrom(int pulls, int rare, int epic, int legendary, int featuredWins, int sigilsSpent, int motesEarned)
        {
            TotalPulls = pulls < 0 ? 0 : pulls;
            RareCount = rare < 0 ? 0 : rare;
            EpicCount = epic < 0 ? 0 : epic;
            LegendaryCount = legendary < 0 ? 0 : legendary;
            FeaturedWins = featuredWins < 0 ? 0 : featuredWins;
            SigilsSpent = sigilsSpent < 0 ? 0 : sigilsSpent;
            MotesEarned = motesEarned < 0 ? 0 : motesEarned;
        }
    }
}
