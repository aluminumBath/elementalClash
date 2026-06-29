using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class AbilityUnlocksTests
    {
        [Test]
        public void StartingKitIsAvailableFromLevelOne()
        {
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.PrimaryCast, 1));
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.Defend, 1));
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.Dash, 1));   // movement is never gated
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.None, 1));
        }

        [Test]
        public void SweepUnlocksAtTwo()
        {
            Assert.AreEqual(2, AbilityUnlocks.RequiredLevel(IntentType.Sweep));
            Assert.IsFalse(AbilityUnlocks.IsUnlocked(IntentType.Sweep, 1));
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.Sweep, 2));
        }

        [Test]
        public void HeavyUnlocksAtFour()
        {
            Assert.AreEqual(4, AbilityUnlocks.RequiredLevel(IntentType.Heavy));
            Assert.IsFalse(AbilityUnlocks.IsUnlocked(IntentType.Heavy, 3));
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.Heavy, 4));
        }

        [Test]
        public void SignatureCastsUnlockAtSix()
        {
            Assert.AreEqual(6, AbilityUnlocks.RequiredLevel(IntentType.SecondaryCast));
            Assert.AreEqual(6, AbilityUnlocks.RequiredLevel(IntentType.Signature));
            Assert.IsFalse(AbilityUnlocks.IsUnlocked(IntentType.SecondaryCast, 5));
            Assert.IsFalse(AbilityUnlocks.IsUnlocked(IntentType.Signature, 5));
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.SecondaryCast, 6));
            Assert.IsTrue(AbilityUnlocks.IsUnlocked(IntentType.Signature, 6));
        }

        [Test]
        public void NewlyUnlockedReportsEachStep()
        {
            CollectionAssert.AreEquivalent(new[] { IntentType.Sweep },
                AbilityUnlocks.NewlyUnlocked(1, 2));
            CollectionAssert.AreEquivalent(new[] { IntentType.Heavy },
                AbilityUnlocks.NewlyUnlocked(3, 4));
            CollectionAssert.AreEquivalent(new[] { IntentType.SecondaryCast, IntentType.Signature },
                AbilityUnlocks.NewlyUnlocked(5, 6));
        }

        [Test]
        public void NewlyUnlockedIsEmptyForALevelWithNoUnlock()
        {
            Assert.AreEqual(0, AbilityUnlocks.NewlyUnlocked(2, 3).Count);
        }

        [Test]
        public void NewlyUnlockedCoversMultiLevelJumps()
        {
            var all = AbilityUnlocks.NewlyUnlocked(1, 6);
            CollectionAssert.AreEquivalent(
                new[] { IntentType.Sweep, IntentType.Heavy, IntentType.SecondaryCast, IntentType.Signature },
                all);
        }

        [Test]
        public void DisplayNamesAreNonEmpty()
        {
            foreach (IntentType i in new[] { IntentType.PrimaryCast, IntentType.SecondaryCast, IntentType.Defend,
                                             IntentType.Dash, IntentType.Heavy, IntentType.Sweep, IntentType.Signature })
                Assert.IsFalse(string.IsNullOrEmpty(AbilityUnlocks.DisplayName(i)), i.ToString());
        }
    }
}
