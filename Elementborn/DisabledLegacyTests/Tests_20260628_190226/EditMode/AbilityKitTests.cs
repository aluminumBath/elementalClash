using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class AbilityKitTests
    {
        private static AbilityOutcome Resolve(Element e, IntentType t, float charge = 0.5f)
            => new AbilitySystem(ChannelerLoadout.SingleElement(e))
                .Resolve(new ChannelingIntent(t, Vector3.forward, charge));

        [Test]
        public void EveryElementHasHeavyAndSweep()
        {
            foreach (Element e in new[] { Element.Fire, Element.Water, Element.Earth, Element.Air })
            {
                var heavy = Resolve(e, IntentType.Heavy);
                var sweep = Resolve(e, IntentType.Sweep);
                Assert.IsFalse(heavy.IsEmpty, e + " Heavy should resolve");
                Assert.IsFalse(sweep.IsEmpty, e + " Sweep should resolve");
                Assert.AreEqual(e, heavy.Element);
                Assert.AreEqual(e, sweep.Element);
            }
        }

        [Test]
        public void HeavyHitsHarderThanPrimaryAndKnocksBack()
        {
            var primary = Resolve(Element.Earth, IntentType.PrimaryCast, 0f);
            var heavy = Resolve(Element.Earth, IntentType.Heavy, 0f);
            Assert.Greater(heavy.Damage, primary.Damage);
            Assert.Greater(heavy.Knockback, 0f);
        }

        [Test]
        public void ChargeIncreasesDamage()
        {
            var low = Resolve(Element.Fire, IntentType.Heavy, 0f);
            var high = Resolve(Element.Fire, IntentType.Heavy, 1f);
            Assert.Greater(high.Damage, low.Damage);
        }

        [Test]
        public void SweepKnocksBack()
        {
            Assert.Greater(Resolve(Element.Air, IntentType.Sweep).Knockback, 0f);
        }

        [Test]
        public void ExistingMovesUnchanged()
        {
            // The original kit still resolves as before (regression guard for the enum/append).
            Assert.AreEqual(OutcomeKind.Projectile, Resolve(Element.Fire, IntentType.PrimaryCast).Kind);
            Assert.AreEqual(OutcomeKind.Barrier, Resolve(Element.Water, IntentType.Defend).Kind);
            Assert.AreEqual(OutcomeKind.Movement, Resolve(Element.Air, IntentType.Dash).Kind);
            Assert.IsTrue(Resolve(Element.Fire, IntentType.Dash).IsEmpty); // fire has no dash
        }
    }
}
