using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SummonStatsTests
    {
        private static SummonResult Res(SummonRarity r, bool featured = false) =>
            new SummonResult(CreatureKind.Phoenix, r, featured, false);

        [Test]
        public void RecordResultCountsTierAndTotals()
        {
            var st = new SummonStats();
            st.RecordResult(Res(SummonRarity.Rare));
            st.RecordResult(Res(SummonRarity.Epic));
            st.RecordResult(Res(SummonRarity.Legendary, featured: true));

            Assert.AreEqual(3, st.TotalPulls);
            Assert.AreEqual(1, st.RareCount);
            Assert.AreEqual(1, st.EpicCount);
            Assert.AreEqual(1, st.LegendaryCount);
            Assert.AreEqual(1, st.FeaturedWins, "a won-featured Legendary counts as a featured win");
        }

        [Test]
        public void NonFeaturedLegendaryDoesNotCountAsFeaturedWin()
        {
            var st = new SummonStats();
            st.RecordResult(Res(SummonRarity.Legendary, featured: false));
            Assert.AreEqual(1, st.LegendaryCount);
            Assert.AreEqual(0, st.FeaturedWins);
        }

        [Test]
        public void RecordResultsSumsABatch()
        {
            var st = new SummonStats();
            st.RecordResults(new List<SummonResult>
            {
                Res(SummonRarity.Rare), Res(SummonRarity.Rare), Res(SummonRarity.Epic)
            });
            Assert.AreEqual(3, st.TotalPulls);
            Assert.AreEqual(2, st.CountOf(SummonRarity.Rare));
            Assert.AreEqual(1, st.CountOf(SummonRarity.Epic));
        }

        [Test]
        public void SpendAndMotesAccumulateAndIgnoreNonPositive()
        {
            var st = new SummonStats();
            st.RecordSpend(160);
            st.RecordSpend(1600);
            st.RecordSpend(0);
            st.RecordSpend(-50);
            st.RecordMotes(5);
            st.RecordMotes(-1);
            Assert.AreEqual(1760, st.SigilsSpent);
            Assert.AreEqual(5, st.MotesEarned);
        }

        [Test]
        public void RateOfIsZeroWithNoPullsThenObservedShare()
        {
            var st = new SummonStats();
            Assert.AreEqual(0.0, st.RateOf(SummonRarity.Legendary), 1e-9);

            for (int i = 0; i < 3; i++) st.RecordResult(Res(SummonRarity.Rare));
            st.RecordResult(Res(SummonRarity.Legendary));
            Assert.AreEqual(0.25, st.RateOf(SummonRarity.Legendary), 1e-9, "1 of 4 pulls was Legendary");
        }

        [Test]
        public void LoadFromClampsNegatives()
        {
            var st = new SummonStats();
            st.LoadFrom(-1, -1, -1, -1, -1, -1, -1);
            Assert.AreEqual(0, st.TotalPulls);
            Assert.AreEqual(0, st.SigilsSpent);
            Assert.AreEqual(0, st.MotesEarned);

            st.LoadFrom(10, 6, 3, 1, 1, 320, 4);
            Assert.AreEqual(10, st.TotalPulls);
            Assert.AreEqual(6, st.RareCount);
            Assert.AreEqual(3, st.EpicCount);
            Assert.AreEqual(1, st.LegendaryCount);
            Assert.AreEqual(320, st.SigilsSpent);
            Assert.AreEqual(4, st.MotesEarned);
        }
    }
}
