using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ProgressionTests
    {
        [Test]
        public void StartsAtLevelOneWithNoBonus()
        {
            var p = new Progression();
            Assert.AreEqual(1, p.Level);
            Assert.AreEqual(0, p.Xp);
            Assert.AreEqual(100, p.XpToNext);
            Assert.AreEqual(0f, p.BonusMaxHealth);
        }

        [Test]
        public void CrossingThresholdLevelsUpAndCarriesRemainder()
        {
            var p = new Progression();
            Assert.AreEqual(1, p.AddXp(120));   // 100 to reach L2, 20 carried
            Assert.AreEqual(2, p.Level);
            Assert.AreEqual(20, p.Xp);
            Assert.AreEqual(200, p.XpToNext);   // L2 needs 200
            Assert.AreEqual(0, p.AddXp(50));    // not enough for L3
            Assert.AreEqual(2, p.Level);
        }

        [Test]
        public void BigGainCoversSeveralLevels()
        {
            var p = new Progression();
            // 100 -> L2, 200 -> L3, 300 -> L4 (600 total), 0 carried
            Assert.AreEqual(3, p.AddXp(600));
            Assert.AreEqual(4, p.Level);
            Assert.AreEqual(0, p.Xp);
        }

        [Test]
        public void BonusHealthScalesWithLevel()
        {
            var p = new Progression();
            p.AddXp(100 + 200 + 300 + 400);   // reach level 5
            Assert.AreEqual(5, p.Level);
            Assert.AreEqual(4 * Progression.HealthPerLevel, p.BonusMaxHealth);
        }

        [Test]
        public void ZeroOrNegativeXpDoesNothing()
        {
            var p = new Progression();
            Assert.AreEqual(0, p.AddXp(0));
            Assert.AreEqual(0, p.AddXp(-50));
            Assert.AreEqual(1, p.Level);
        }

        [Test]
        public void RestoreClampsAndSets()
        {
            var p = new Progression();
            p.Restore(7, 40);
            Assert.AreEqual(7, p.Level);
            Assert.AreEqual(40, p.Xp);
            p.Restore(0, -10);   // clamps
            Assert.AreEqual(1, p.Level);
            Assert.AreEqual(0, p.Xp);
        }
    }
}
