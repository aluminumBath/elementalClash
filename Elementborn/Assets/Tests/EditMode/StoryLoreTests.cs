using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class StoryLoreTests
    {
        [Test]
        public void FourKingsOnePerElement()
        {
            Assert.AreEqual(4, StoryLore.Kings.Length);
            var elements = new HashSet<Element>();
            foreach (var k in StoryLore.Kings) elements.Add(k.Element);
            Assert.AreEqual(4, elements.Count, "each King should hold a distinct element");
            Assert.IsTrue(elements.Contains(Element.Fire));
            Assert.IsTrue(elements.Contains(Element.Water));
            Assert.IsTrue(elements.Contains(Element.Earth));
            Assert.IsTrue(elements.Contains(Element.Air));
        }

        [Test]
        public void TerragorIsTheCrownedRhinoEarthKing()
        {
            Assert.AreEqual("Terragor", StoryLore.Terragor.Name);
            Assert.AreEqual(Element.Earth, StoryLore.Terragor.Element);
            Assert.AreEqual(CreatureKind.Rhino, StoryLore.Terragor.Beast);
        }

        [Test]
        public void KingOfReturnsTheMatchingKing()
        {
            Assert.AreEqual(CreatureKind.Phoenix,    StoryLore.KingOf(Element.Fire).Beast);
            Assert.AreEqual(CreatureKind.Tidewarden, StoryLore.KingOf(Element.Water).Beast);
            Assert.AreEqual(CreatureKind.Rhino,      StoryLore.KingOf(Element.Earth).Beast);
            Assert.AreEqual(CreatureKind.Roc,        StoryLore.KingOf(Element.Air).Beast);
        }

        [Test]
        public void CityAndDiplomatAreNamed()
        {
            Assert.IsFalse(string.IsNullOrEmpty(StoryLore.CentralCity));
            Assert.IsFalse(string.IsNullOrEmpty(StoryLore.DiplomatName));
            Assert.IsFalse(string.IsNullOrEmpty(StoryLore.GreatBetrayal));
        }
    }
}
