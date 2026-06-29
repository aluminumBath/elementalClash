using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ElementMatchupTests
    {
        [Test]
        public void StrongAlongTheCycle()
        {
            Assert.AreEqual(ElementMatchup.Strong, ElementMatchup.Multiplier(Element.Fire, Element.Earth), 1e-5f);
            Assert.AreEqual(ElementMatchup.Strong, ElementMatchup.Multiplier(Element.Earth, Element.Air), 1e-5f);
            Assert.AreEqual(ElementMatchup.Strong, ElementMatchup.Multiplier(Element.Air, Element.Water), 1e-5f);
            Assert.AreEqual(ElementMatchup.Strong, ElementMatchup.Multiplier(Element.Water, Element.Fire), 1e-5f);
        }

        [Test]
        public void WeakAgainstThePriorElement()
        {
            Assert.AreEqual(ElementMatchup.Weak, ElementMatchup.Multiplier(Element.Earth, Element.Fire), 1e-5f);
            Assert.AreEqual(ElementMatchup.Weak, ElementMatchup.Multiplier(Element.Fire, Element.Water), 1e-5f);
            Assert.AreEqual(ElementMatchup.Weak, ElementMatchup.Multiplier(Element.Water, Element.Air), 1e-5f);
            Assert.AreEqual(ElementMatchup.Weak, ElementMatchup.Multiplier(Element.Air, Element.Earth), 1e-5f);
        }

        [Test]
        public void NeutralAcrossAndAgainstSelf()
        {
            Assert.AreEqual(ElementMatchup.Neutral, ElementMatchup.Multiplier(Element.Fire, Element.Air), 1e-5f);
            Assert.AreEqual(ElementMatchup.Neutral, ElementMatchup.Multiplier(Element.Earth, Element.Water), 1e-5f);
            Assert.AreEqual(ElementMatchup.Neutral, ElementMatchup.Multiplier(Element.Fire, Element.Fire), 1e-5f);
            Assert.AreEqual(ElementMatchup.Neutral, ElementMatchup.Multiplier(Element.Water, Element.Water), 1e-5f);
        }

        [Test]
        public void ClassifyLabelsTheCycle()
        {
            Assert.AreEqual(Effectiveness.Strong, ElementMatchup.Classify(Element.Fire, Element.Earth));
            Assert.AreEqual(Effectiveness.Weak, ElementMatchup.Classify(Element.Earth, Element.Fire));
            Assert.AreEqual(Effectiveness.Neutral, ElementMatchup.Classify(Element.Fire, Element.Air));
            Assert.AreEqual(Effectiveness.Neutral, ElementMatchup.Classify(Element.Fire, Element.Fire));
        }

        [Test]
        public void ClassifyAndMultiplierAgreeForEveryPair()
        {
            foreach (Element a in System.Enum.GetValues(typeof(Element)))
            foreach (Element d in System.Enum.GetValues(typeof(Element)))
            {
                var c = ElementMatchup.Classify(a, d);
                float expected = c == Effectiveness.Strong ? ElementMatchup.Strong
                               : c == Effectiveness.Weak ? ElementMatchup.Weak
                               : ElementMatchup.Neutral;
                Assert.AreEqual(expected, ElementMatchup.Multiplier(a, d), 1e-5f, $"{a} vs {d}");
            }
        }
    }
}
