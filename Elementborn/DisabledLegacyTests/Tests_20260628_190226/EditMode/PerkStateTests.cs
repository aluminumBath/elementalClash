using NUnit.Framework;
using System.Collections.Generic;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class PerkStateTests
    {
        [Test]
        public void SpendingNeedsPointsAndRanksUp()
        {
            var p = new PerkState();
            Assert.IsFalse(p.CanRank(PerkId.Toughness));  // no points yet
            Assert.IsFalse(p.Rank(PerkId.Toughness));

            p.Grant(2);
            Assert.IsTrue(p.Rank(PerkId.Toughness));
            Assert.AreEqual(1, p.RankOf(PerkId.Toughness));
            Assert.AreEqual(1, p.AvailablePoints);
        }

        [Test]
        public void RanksCapAtMax()
        {
            var p = new PerkState();
            var def = PerkCatalog.Get(PerkId.Scholar);
            p.Grant(100);
            for (int i = 0; i < def.MaxRank; i++) Assert.IsTrue(p.Rank(PerkId.Scholar));
            Assert.AreEqual(def.MaxRank, p.RankOf(PerkId.Scholar));
            Assert.IsFalse(p.CanRank(PerkId.Scholar));   // capped
            Assert.IsFalse(p.Rank(PerkId.Scholar));
        }

        [Test]
        public void EffectsScaleWithRank()
        {
            var p = new PerkState();
            p.Grant(10);
            Assert.AreEqual(0f, p.BonusMaxHealth);
            Assert.AreEqual(1f, p.XpMultiplier);
            Assert.AreEqual(1f, p.RewardMultiplier);

            p.Rank(PerkId.Toughness);
            p.Rank(PerkId.Scholar);
            p.Rank(PerkId.Fortune);
            Assert.AreEqual(20f, p.BonusMaxHealth);
            Assert.AreEqual(1.10f, p.XpMultiplier, 0.0001f);
            Assert.AreEqual(1.15f, p.RewardMultiplier, 0.0001f);
        }

        [Test]
        public void RestoreSetsRanksAndPoints()
        {
            var p = new PerkState();
            p.Restore(3, new List<PerkId> { PerkId.Toughness, PerkId.Fortune }, new List<int> { 2, 1 });
            Assert.AreEqual(3, p.AvailablePoints);
            Assert.AreEqual(2, p.RankOf(PerkId.Toughness));
            Assert.AreEqual(1, p.RankOf(PerkId.Fortune));
            Assert.AreEqual(40f, p.BonusMaxHealth);
            p.Restore(-5, null, null);   // clamps + clears
            Assert.AreEqual(0, p.AvailablePoints);
            Assert.AreEqual(0, p.RankOf(PerkId.Toughness));
        }
    }
}
