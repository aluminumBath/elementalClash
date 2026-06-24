using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class GuardStateTests
    {
        private static GuardState New() => new GuardState(0.22f, 0.7f, 0.3f);

        [Test]
        public void NotGuardingResolvesNone()
        {
            var g = New();
            Assert.IsFalse(g.IsGuarding);
            Assert.AreEqual(GuardResult.None, g.Resolve());
        }

        [Test]
        public void ParriesWithinWindowOncePerRaise()
        {
            var g = New();
            g.Raise();
            Assert.IsTrue(g.InParryWindow);
            Assert.AreEqual(GuardResult.Parried, g.Resolve());
            Assert.IsFalse(g.InParryWindow);              // spent
            Assert.AreEqual(GuardResult.Blocked, g.Resolve()); // next hit just blocks
        }

        [Test]
        public void BlocksAfterParryWindowElapses()
        {
            var g = New();
            g.Raise();
            g.Tick(0.30f);                                // past the 0.22 window
            Assert.IsFalse(g.InParryWindow);
            Assert.AreEqual(GuardResult.Blocked, g.Resolve());
        }

        [Test]
        public void DamageMultiplierPerResult()
        {
            var g = New();
            Assert.AreEqual(0f, g.DamageMultiplier(GuardResult.Parried), 1e-4f);
            Assert.AreEqual(0.3f, g.DamageMultiplier(GuardResult.Blocked), 1e-4f); // 1 - 0.7
            Assert.AreEqual(1f, g.DamageMultiplier(GuardResult.None), 1e-4f);
        }

        [Test]
        public void LowerStopsGuard()
        {
            var g = New();
            g.Raise();
            g.Lower();
            Assert.IsFalse(g.IsGuarding);
            Assert.AreEqual(GuardResult.None, g.Resolve());
        }

        [Test]
        public void ReRaiseRefreshesParry()
        {
            var g = New();
            g.Raise();
            g.Resolve();          // spend parry
            g.Lower();
            g.Raise();            // fresh raise
            Assert.IsTrue(g.InParryWindow);
            Assert.AreEqual(GuardResult.Parried, g.Resolve());
        }
    }
}
