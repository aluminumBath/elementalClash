using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class GachaRollerTests
    {
        private static GachaConfig Config() => new GachaConfig
        {
            ConfluenceChance = 0.001,
            SubArtChance = 0.05,
            ConfluenceIncludesSubArts = true
        };

        [Test]
        public void LowRoll_GrantsConfluenceWithAllElements()
        {
            var rng = new ScriptedRandomSource(0.5, 0.0);
            var loadout = GachaRoller.RollForChanneler(Element.Fire, rng, Config());

            Assert.IsTrue(loadout.IsConfluence);
            Assert.AreEqual(4, loadout.Elements.Count);
            Assert.IsTrue(loadout.HasSubArt(SubArt.Magmacraft));
            Assert.IsTrue(loadout.HasSubArt(SubArt.SanguineGrip));
        }

        [Test]
        public void MidRoll_GrantsChosenElementWithMatchingSubArt()
        {
            // Above the confluence band, inside the sub-art band.
            var rng = new ScriptedRandomSource(0.5, 0.01);
            var loadout = GachaRoller.RollForChanneler(Element.Water, rng, Config());

            Assert.IsFalse(loadout.IsConfluence);
            CollectionAssert.AreEqual(new[] { Element.Water }, loadout.Elements);
            Assert.IsTrue(loadout.HasSubArt(SubArt.SanguineGrip));
            Assert.IsFalse(loadout.HasSubArt(SubArt.Magmacraft));
        }

        [Test]
        public void HighRoll_GrantsOnlyTheBaseElement()
        {
            var rng = new ScriptedRandomSource(0.5, 0.9);
            var loadout = GachaRoller.RollForChanneler(Element.Earth, rng, Config());

            Assert.IsFalse(loadout.IsConfluence);
            CollectionAssert.AreEqual(new[] { Element.Earth }, loadout.Elements);
            Assert.AreEqual(0, loadout.SubArts.Count);
        }

        [TestCase(Element.Fire, SubArt.Magmacraft)]
        [TestCase(Element.Water, SubArt.SanguineGrip)]
        [TestCase(Element.Earth, SubArt.Oreshaping)]
        [TestCase(Element.Air, SubArt.Flight)]
        public void SubArtAlwaysMatchesChosenElement(Element element, SubArt expected)
        {
            var rng = new ScriptedRandomSource(0.5, 0.01); // sub-art band
            var loadout = GachaRoller.RollForChanneler(element, rng, Config());
            Assert.IsTrue(loadout.HasSubArt(expected));
        }

        [Test]
        public void WeaponChoice_HasNoElements()
        {
            var loadout = GachaRoller.ChooseWeapon(WeaponType.Sword);
            Assert.IsFalse(loadout.IsChanneler);
            Assert.AreEqual(WeaponType.Sword, loadout.Weapon);
        }

        [Test]
        public void ConfluenceWithoutSubArts_WhenConfigured()
        {
            var config = Config();
            config.ConfluenceIncludesSubArts = false;
            var rng = new ScriptedRandomSource(0.5, 0.0);

            var loadout = GachaRoller.RollForChanneler(Element.Air, rng, config);
            Assert.IsTrue(loadout.IsConfluence);
            Assert.AreEqual(0, loadout.SubArts.Count);
        }
    }
}
