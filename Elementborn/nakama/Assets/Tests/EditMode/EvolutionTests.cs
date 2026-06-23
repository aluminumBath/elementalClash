using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class EvolutionTests
    {
        [Test]
        public void EveryPairYieldsItsSpecialtyEitherOrder()
        {
            Assert.AreEqual(SubArt.Verdancy,     Specialties.For(Element.Water, Element.Earth));
            Assert.AreEqual(SubArt.Verdancy,     Specialties.For(Element.Earth, Element.Water)); // order-independent
            Assert.AreEqual(SubArt.SanguineGrip, Specialties.For(Element.Water, Element.Air));
            Assert.AreEqual(SubArt.Steamcraft,   Specialties.For(Element.Water, Element.Fire));
            Assert.AreEqual(SubArt.Oreshaping,   Specialties.For(Element.Earth, Element.Air));
            Assert.AreEqual(SubArt.Magmacraft,   Specialties.For(Element.Earth, Element.Fire));
            Assert.AreEqual(SubArt.Flight,       Specialties.For(Element.Fire, Element.Air));
        }

        [Test]
        public void SameElementHasNoSpecialty()
        {
            Assert.AreEqual(SubArt.None, Specialties.For(Element.Fire, Element.Fire));
        }

        [Test]
        public void EvolutionGrantsSpecialtyAndLoadout()
        {
            var evo = new ElementalEvolution(Element.Water);
            Assert.IsFalse(evo.HasSecondary);
            Assert.AreEqual(SubArt.None, evo.Specialty);

            Assert.IsFalse(evo.Evolve(Element.Water));   // can't pick the same element
            Assert.IsTrue(evo.Evolve(Element.Fire));     // water + fire -> steam & healing
            Assert.AreEqual(SubArt.Steamcraft, evo.Specialty);
            Assert.IsFalse(evo.Evolve(Element.Earth));   // only evolve once

            var loadout = evo.ToLoadout();
            Assert.IsTrue(loadout.HasElement(Element.Water));
            Assert.IsTrue(loadout.HasElement(Element.Fire));
            Assert.IsTrue(loadout.HasSubArt(SubArt.Steamcraft));
        }
    }
}
