using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class AbilityPaletteTests
    {
        [Test]
        public void EachElementHasADistinctOpaquePrimary()
        {
            var fire = AbilityPalette.Primary(Element.Fire);
            var water = AbilityPalette.Primary(Element.Water);
            var earth = AbilityPalette.Primary(Element.Earth);
            var air = AbilityPalette.Primary(Element.Air);
            Assert.AreNotEqual(fire, water);
            Assert.AreNotEqual(water, earth);
            Assert.AreNotEqual(earth, air);
            Assert.AreEqual(1f, fire.a, 0.0001f); // opaque
        }

        [Test]
        public void SubArtVariantGivesItsOwnSecondaryTint()
        {
            var iceSecondary = AbilityPalette.Secondary(AbilityVariant.Ice, Element.Water);
            var plainSecondary = AbilityPalette.Secondary(AbilityVariant.Standard, Element.Water);
            Assert.AreNotEqual(iceSecondary, plainSecondary); // the sub-art uses its own tint, not the fallback
        }

        [Test]
        public void StandardSecondaryIsALighterShadeOfThePrimary()
        {
            var primary = AbilityPalette.Primary(Element.Earth);
            var secondary = AbilityPalette.Secondary(AbilityVariant.Standard, Element.Earth);
            Assert.GreaterOrEqual(secondary.r, primary.r - 0.0001f);
            Assert.GreaterOrEqual(secondary.g, primary.g - 0.0001f);
            Assert.GreaterOrEqual(secondary.b, primary.b - 0.0001f);
            Assert.AreNotEqual(primary, secondary);
        }
    }
}
