using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SweepMovesetTests
    {
        private static AbilityOutcome Sweep(Element e, float charge = 0.5f)
            => new AbilitySystem(ChannelerLoadout.SingleElement(e))
                .Resolve(new ChannelingIntent(IntentType.Sweep, Vector3.forward, charge));

        [Test]
        public void EverySweepIsTheSweepKind() // the distinct shape, not a reskinned projectile
        {
            foreach (Element e in new[] { Element.Fire, Element.Water, Element.Earth, Element.Air })
                Assert.AreEqual(OutcomeKind.Sweep, Sweep(e).Kind, e + " sweep should be a Sweep");
        }

        [Test]
        public void FireSweepBurns()
        {
            Assert.AreEqual(StatusKind.Burn, Sweep(Element.Fire).Status.Kind);
        }

        [Test]
        public void WaterSweepSlows()
        {
            Assert.AreEqual(StatusKind.Slow, Sweep(Element.Water).Status.Kind);
        }

        [Test]
        public void EarthSweepStaggersAndShoves()
        {
            var earth = Sweep(Element.Earth);
            Assert.AreEqual(StatusKind.Control, earth.Status.Kind);
            Assert.Greater(earth.Knockback, 0f);
        }

        [Test]
        public void AirSweepIsPureKnockbackWithTheBiggestShove()
        {
            var air = Sweep(Element.Air);
            Assert.IsTrue(air.Status.IsEmpty, "air sweep carries no status");
            Assert.Greater(air.Knockback, 0f);
            Assert.Greater(air.Knockback, Sweep(Element.Fire).Knockback);
            Assert.Greater(air.Knockback, Sweep(Element.Water).Knockback);
            Assert.Greater(air.Knockback, Sweep(Element.Earth).Knockback);
        }

        [Test]
        public void ChargeIncreasesSweepDamage()
        {
            Assert.Greater(Sweep(Element.Fire, 1f).Damage, Sweep(Element.Fire, 0f).Damage);
        }
    }
}
