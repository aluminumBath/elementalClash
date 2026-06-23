using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class TamingTests
    {
        private static CreatureInfo Chance(float c) =>
            new CreatureInfo("Test", Element.Fire, null, false, false, false, 0, c);

        [Test]
        public void NeedsALure()
        {
            Assert.IsFalse(TamingRules.CanAttempt(Chance(0.5f), 0.1f, hasLure: false, out _));
        }

        [Test]
        public void MustBeWeakened()
        {
            Assert.IsFalse(TamingRules.CanAttempt(Chance(0.5f), 0.9f, hasLure: true, out _));
            Assert.IsTrue(TamingRules.CanAttempt(Chance(0.5f), 0.2f, hasLure: true, out _));
        }

        [Test]
        public void FailedPrerequisitesDoNotConsumeLure()
        {
            var outcome = TamingRules.Resolve(Chance(1f), 0.9f, hasLure: true, new SystemRandomSource(1));
            Assert.IsFalse(outcome.Success);
            Assert.IsFalse(outcome.LureConsumed);
        }

        [Test]
        public void AGenuineAttemptConsumesTheLure()
        {
            var outcome = TamingRules.Resolve(Chance(0f), 0.1f, hasLure: true, new SystemRandomSource(1));
            Assert.IsFalse(outcome.Success);     // 0% chance
            Assert.IsTrue(outcome.LureConsumed); // but the lure is spent
        }

        [Test]
        public void ChanceBoundsBehave()
        {
            Assert.IsTrue(TamingRules.Resolve(Chance(1f), 0.1f, true, new SystemRandomSource(5)).Success);
            Assert.IsFalse(TamingRules.Resolve(Chance(0f), 0.1f, true, new SystemRandomSource(5)).Success);
        }

        [Test]
        public void ResolveIsDeterministic()
        {
            var a = TamingRules.Resolve(Chance(0.5f), 0.1f, true, new SystemRandomSource(42));
            var b = TamingRules.Resolve(Chance(0.5f), 0.1f, true, new SystemRandomSource(42));
            Assert.AreEqual(a.Success, b.Success);
        }
    }
}
