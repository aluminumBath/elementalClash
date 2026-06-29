using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ElementDexTests
    {
        [Test]
        public void StartsEmpty()
        {
            var d = new ElementDex();
            Assert.IsFalse(d.IsDiscovered(Element.Fire));
            Assert.AreEqual(0, d.Count);
        }

        [Test]
        public void DiscoverIsIdempotent()
        {
            var d = new ElementDex();
            Assert.IsTrue(d.Discover(Element.Water));
            Assert.IsFalse(d.Discover(Element.Water)); // already known
            Assert.IsTrue(d.IsDiscovered(Element.Water));
            Assert.AreEqual(1, d.Count);
        }

        [Test]
        public void RoundTripsThroughSave()
        {
            var d = new ElementDex();
            d.Discover(Element.Fire);
            d.Discover(Element.Air);
            var reloaded = ElementDex.LoadFrom(d.ToSave());
            Assert.IsTrue(reloaded.IsDiscovered(Element.Fire));
            Assert.IsTrue(reloaded.IsDiscovered(Element.Air));
            Assert.IsFalse(reloaded.IsDiscovered(Element.Earth));
            Assert.AreEqual(2, reloaded.Count);
        }

        [Test]
        public void LoadFromNullIsEmpty()
        {
            Assert.AreEqual(0, ElementDex.LoadFrom(null).Count);
        }
    }
}
