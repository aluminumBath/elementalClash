using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ElementTravelTests
    {
        [Test]
        public void WaterRaisesAnIceFloe()
        {
            Assert.AreEqual(TravelMode.IceFloe, ElementTravel.ModeFor(Element.Water));
        }

        [Test]
        public void AirFormsABubble()
        {
            Assert.AreEqual(TravelMode.Bubble, ElementTravel.ModeFor(Element.Air));
        }

        [Test]
        public void FireAndEarthHaveNoWaterTravel()
        {
            Assert.AreEqual(TravelMode.None, ElementTravel.ModeFor(Element.Fire));
            Assert.AreEqual(TravelMode.None, ElementTravel.ModeFor(Element.Earth));
        }

        [Test]
        public void NonChannelerHasNoWaterTravel()
        {
            Assert.AreEqual(TravelMode.None, ElementTravel.ModeFor(null));
        }
    }
}
