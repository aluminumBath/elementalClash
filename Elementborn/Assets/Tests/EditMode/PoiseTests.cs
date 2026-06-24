using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class PoiseTests
    {
        [Test]
        public void AccumulatesUntilBreak()
        {
            var p = new Poise(100f, 50f, 1f);
            Assert.IsFalse(p.AddHit(40f));
            Assert.AreEqual(40f, p.Current, 1e-4f);
            Assert.AreEqual(0.4f, p.Fraction, 1e-4f);
            Assert.IsFalse(p.AddHit(40f));   // 80
            Assert.IsTrue(p.AddHit(40f));    // 120 >= 100 -> break
            Assert.AreEqual(0f, p.Current, 1e-4f); // resets on break
        }

        [Test]
        public void BreaksExactlyAtMax()
        {
            var p = new Poise(50f, 10f, 1f);
            Assert.IsTrue(p.AddHit(50f));
            Assert.AreEqual(0f, p.Current, 1e-4f);
        }

        [Test]
        public void DoesNotRegenDuringDelay()
        {
            var p = new Poise(100f, 50f, 1f);
            p.AddHit(40f);        // sinceHit = 0
            p.Tick(0.5f);         // 0.5 < 1.0 delay -> no regen
            Assert.AreEqual(40f, p.Current, 1e-4f);
        }

        [Test]
        public void RegensAfterDelayAndClampsAtZero()
        {
            var p = new Poise(100f, 50f, 1f);
            p.AddHit(40f);
            p.Tick(0.6f);         // sinceHit 0.6 < 1 -> no regen
            p.Tick(0.6f);         // sinceHit 1.2 >= 1 -> -30 -> 10
            Assert.AreEqual(10f, p.Current, 1e-3f);
            p.Tick(1.0f);         // -50 -> clamp 0
            Assert.AreEqual(0f, p.Current, 1e-4f);
        }

        [Test]
        public void ConstructorGuardsBadMax()
        {
            var p = new Poise(0f, 10f, 1f);
            Assert.GreaterOrEqual(p.Max, 1f);
        }
    }
}
