using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class EnvironmentHazardsTests
    {
        [Test]
        public void NoColdAtOrBelowSafeAltitude()
        {
            Assert.AreEqual(0f, EnvironmentHazards.ColdDamagePerSecond(0f));
            Assert.AreEqual(0f, EnvironmentHazards.ColdDamagePerSecond(EnvironmentHazards.ColdSafeAltitude));
        }

        [Test]
        public void ColdRampsUpAndCapsAtMax()
        {
            Assert.Greater(EnvironmentHazards.ColdDamagePerSecond(150f), 0f);
            Assert.Greater(EnvironmentHazards.ColdDamagePerSecond(200f), EnvironmentHazards.ColdDamagePerSecond(100f));
            Assert.AreEqual(EnvironmentHazards.ColdMaxDps, EnvironmentHazards.ColdDamagePerSecond(10000f), 1e-3f);
        }

        [Test]
        public void NoPressureAtOrAboveSafeDepthOrAboveWater()
        {
            Assert.AreEqual(0f, EnvironmentHazards.PressureDamagePerSecond(0f));
            Assert.AreEqual(0f, EnvironmentHazards.PressureDamagePerSecond(EnvironmentHazards.PressureSafeDepth));
            Assert.AreEqual(0f, EnvironmentHazards.PressureDamagePerSecond(-25f)); // above the surface
        }

        [Test]
        public void PressureRampsUpAndCapsAtMax()
        {
            Assert.Greater(EnvironmentHazards.PressureDamagePerSecond(60f), 0f);
            Assert.Greater(EnvironmentHazards.PressureDamagePerSecond(90f), EnvironmentHazards.PressureDamagePerSecond(40f));
            Assert.AreEqual(EnvironmentHazards.PressureMaxDps, EnvironmentHazards.PressureDamagePerSecond(10000f), 1e-3f);
        }

        [Test]
        public void ColdExemptions()
        {
            Assert.IsTrue(EnvironmentHazards.ImmuneToCold(true, Element.Air, null));
            Assert.IsTrue(EnvironmentHazards.ImmuneToCold(true, Element.Fire, null));
            Assert.IsFalse(EnvironmentHazards.ImmuneToCold(true, Element.Water, null));
            Assert.IsFalse(EnvironmentHazards.ImmuneToCold(true, Element.Earth, null));
            Assert.IsTrue(EnvironmentHazards.ImmuneToCold(false, Element.Water, Element.Fire));   // Fire chest enchant
            Assert.IsFalse(EnvironmentHazards.ImmuneToCold(false, Element.Fire, null));            // non-channeler, no enchant
            Assert.IsFalse(EnvironmentHazards.ImmuneToCold(false, Element.Fire, Element.Water));   // wrong enchant
        }

        [Test]
        public void PressureExemptions()
        {
            Assert.IsTrue(EnvironmentHazards.ImmuneToPressure(true, Element.Water, null, null));
            Assert.IsTrue(EnvironmentHazards.ImmuneToPressure(true, Element.Earth, null, null));
            Assert.IsFalse(EnvironmentHazards.ImmuneToPressure(true, Element.Fire, null, null));
            Assert.IsFalse(EnvironmentHazards.ImmuneToPressure(true, Element.Air, null, null));
            // chest (water/earth) AND helmet (air/water) together exempt a non-channeler
            Assert.IsTrue(EnvironmentHazards.ImmuneToPressure(false, Element.Fire, Element.Water, Element.Air));
            Assert.IsTrue(EnvironmentHazards.ImmuneToPressure(false, Element.Fire, Element.Earth, Element.Water));
            // either piece wrong or missing -> not exempt
            Assert.IsFalse(EnvironmentHazards.ImmuneToPressure(false, Element.Fire, Element.Water, Element.Fire)); // helmet wrong
            Assert.IsFalse(EnvironmentHazards.ImmuneToPressure(false, Element.Fire, Element.Fire, Element.Air));   // chest wrong
            Assert.IsFalse(EnvironmentHazards.ImmuneToPressure(false, Element.Fire, Element.Water, null));         // no helmet
        }
    }
}
