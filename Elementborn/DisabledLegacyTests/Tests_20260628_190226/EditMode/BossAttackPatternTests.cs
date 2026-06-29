using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class BossAttackPatternTests
    {
        [Test]
        public void Charges_ThenTelegraphs_ThenStrikes()
        {
            var p = new BossAttackPattern(2f, 0.5f);
            Assert.AreEqual(BossAttackPhase.Cooldown, p.Phase);
            Assert.IsFalse(p.Tick(1.9f));                      // still charging
            Assert.IsFalse(p.Tick(0.2f));                      // crossed into telegraph
            Assert.AreEqual(BossAttackPhase.Telegraph, p.Phase);
            Assert.IsFalse(p.Tick(0.3f));                      // mid wind-up
            Assert.IsTrue(p.Tick(0.3f));                       // wind-up ends -> strike
            Assert.AreEqual(BossAttackPhase.Cooldown, p.Phase);
        }

        [Test]
        public void StrikesRepeatOnCadence()
        {
            var p = new BossAttackPattern(1f, 0.5f);           // 1.5s cycle
            int strikes = 0;
            for (int i = 0; i < 200; i++) if (p.Tick(0.1f)) strikes++; // 20s
            Assert.GreaterOrEqual(strikes, 10);
            Assert.LessOrEqual(strikes, 15);
        }

        [Test]
        public void TelegraphProgress_RisesToOne()
        {
            var p = new BossAttackPattern(1f, 1f);
            p.Tick(1f);                                        // into telegraph
            Assert.AreEqual(0f, p.TelegraphProgress, 0.05f);
            p.Tick(0.5f);
            Assert.That(p.TelegraphProgress, Is.GreaterThan(0.4f).And.LessThan(0.6f));
        }

        [Test]
        public void Reset_ReturnsToCooldown()
        {
            var p = new BossAttackPattern(2f, 0.5f);
            p.Tick(2.1f);                                      // telegraphing
            p.Reset();
            Assert.AreEqual(BossAttackPhase.Cooldown, p.Phase);
            Assert.AreEqual(0f, p.TelegraphProgress, 1e-4f);
        }
    }
}
