using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class HeavyMovesetTests
    {
        private static AbilityOutcome Heavy(Element e, float charge = 0.5f)
            => new AbilitySystem(ChannelerLoadout.SingleElement(e))
                .Resolve(new ChannelingIntent(IntentType.Heavy, Vector3.forward, charge));

        [Test]
        public void EveryHeavyIsTheHeavyKind() // committed impact zone, not a projectile
        {
            foreach (Element e in new[] { Element.Fire, Element.Water, Element.Earth, Element.Air })
                Assert.AreEqual(OutcomeKind.Heavy, Heavy(e).Kind, e + " heavy should be a Heavy");
        }

        [Test]
        public void EveryHeavyKnocksBack()
        {
            foreach (Element e in new[] { Element.Fire, Element.Water, Element.Earth, Element.Air })
                Assert.Greater(Heavy(e).Knockback, 0f, e + " heavy should knock back");
        }

        [Test]
        public void WaterHeavySlows()
        {
            Assert.AreEqual(StatusKind.Slow, Heavy(Element.Water).Status.Kind);
        }

        [Test]
        public void HeavyCarriesNoTravelSpeed() // it's an instant impact, not a projectile
        {
            Assert.AreEqual(0f, Heavy(Element.Earth).Speed, 0.0001f);
        }

        [Test]
        public void ChargeIncreasesHeavyDamage()
        {
            Assert.Greater(Heavy(Element.Earth, 1f).Damage, Heavy(Element.Earth, 0f).Damage);
        }

        [Test]
        public void HeavyCarriesItsCastCharge() // so the controller can scale the blast radius + ring
        {
            Assert.AreEqual(0.5f, Heavy(Element.Fire, 0.5f).Charge, 0.0001f);
            Assert.AreEqual(1f, Heavy(Element.Earth, 1f).Charge, 0.0001f);
        }
    }
}
