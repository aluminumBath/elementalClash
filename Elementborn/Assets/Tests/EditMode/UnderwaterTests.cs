using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class UnderwaterTests
    {
        [Test]
        public void OxygenDrainsWhenNotBreathing()
        {
            var o = new OxygenModel(max: 10f, drainPerSecond: 2f, refillPerSecond: 5f);
            Assert.AreEqual(1f, o.Current01, 0.001f);

            o.Tick(2f, breathing: false);
            Assert.AreEqual(6f, o.Current, 0.001f);
            Assert.IsFalse(o.IsEmpty);

            o.Tick(10f, breathing: false); // floors at 0
            Assert.AreEqual(0f, o.Current, 0.001f);
            Assert.IsTrue(o.IsEmpty);
        }

        [Test]
        public void OxygenRefillsWhenBreathingAndCaps()
        {
            var o = new OxygenModel(max: 10f, drainPerSecond: 2f, refillPerSecond: 5f);
            o.Tick(3f, breathing: false); // down to 4
            Assert.AreEqual(4f, o.Current, 0.001f);

            o.Tick(1f, breathing: true);  // +5 -> 9
            Assert.AreEqual(9f, o.Current, 0.001f);

            o.Tick(5f, breathing: true);  // capped at max
            Assert.AreEqual(10f, o.Current, 0.001f);
        }

        [Test]
        public void SurfaceAllowsEveryElement()
        {
            Assert.IsTrue(UnderwaterAbilityRules.AllowsCast(false, Element.Fire, AbilityVariant.Standard, OutcomeKind.Projectile));
            Assert.IsTrue(UnderwaterAbilityRules.AllowsCast(false, Element.Earth, AbilityVariant.Standard, OutcomeKind.Projectile));
        }

        [Test]
        public void SubmergedAllowsOnlyWaterIceOffense()
        {
            // Water's ice works; its non-ice does not.
            Assert.IsTrue(UnderwaterAbilityRules.AllowsCast(true, Element.Water, AbilityVariant.Ice, OutcomeKind.Projectile));
            Assert.IsFalse(UnderwaterAbilityRules.AllowsCast(true, Element.Water, AbilityVariant.Standard, OutcomeKind.Projectile));

            // Fire / earth / air offense fizzles below the surface.
            Assert.IsFalse(UnderwaterAbilityRules.AllowsCast(true, Element.Fire, AbilityVariant.Standard, OutcomeKind.Projectile));
            Assert.IsFalse(UnderwaterAbilityRules.AllowsCast(true, Element.Air, AbilityVariant.Standard, OutcomeKind.Projectile));
        }

        [Test]
        public void BarriersAndMovementAlwaysPassUnderwater()
        {
            Assert.IsTrue(UnderwaterAbilityRules.AllowsCast(true, Element.Earth, AbilityVariant.Standard, OutcomeKind.Barrier));
            Assert.IsTrue(UnderwaterAbilityRules.AllowsCast(true, Element.Air, AbilityVariant.Standard, OutcomeKind.Movement));
        }

        [Test]
        public void SwimSpeedScalesWithUnderwaterFactor()
        {
            var full = SwimMotion.Velocity(Vector3.forward, 0f, 4f, 1f, 10f);
            var half = SwimMotion.Velocity(Vector3.forward, 0f, 4f, 0.5f, 10f);
            Assert.AreEqual(4f, full.magnitude, 0.01f);
            Assert.AreEqual(2f, half.magnitude, 0.01f);
        }

        [Test]
        public void SwimVelocityIsCappedForComfort()
        {
            var v = SwimMotion.Velocity(Vector3.forward, 0f, 100f, 1f, 6f);
            Assert.LessOrEqual(v.magnitude, 6.01f);
        }

        [Test]
        public void SwimVerticalInputAscends()
        {
            var up = SwimMotion.Velocity(Vector3.zero, 1f, 4f, 1f, 10f);
            Assert.Greater(up.y, 0f);
        }
    }
}
