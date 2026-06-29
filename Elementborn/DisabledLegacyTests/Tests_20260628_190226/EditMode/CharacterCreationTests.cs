using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CharacterCreationTests
    {
        private static GachaConfig Config() =>
            new GachaConfig { ConfluenceChance = 0.001, SubArtChance = 0.05, ConfluenceIncludesSubArts = true };

        [Test]
        public void Jackpot_RevealsConfluence()
        {
            var rng = new ScriptedRandomSource(0.5, 0.0);
            var r = CharacterCreationService.CreateChanneler(Element.Fire, rng, Config());
            Assert.AreEqual(RevealTier.Confluence, r.Tier);
            Assert.IsTrue(r.Loadout.IsConfluence);
        }

        [Test]
        public void SubArtBand_RevealsSubArt()
        {
            var rng = new ScriptedRandomSource(0.5, 0.01);
            var r = CharacterCreationService.CreateChanneler(Element.Water, rng, Config());
            Assert.AreEqual(RevealTier.SubArt, r.Tier);
            Assert.IsTrue(r.Loadout.HasSubArt(SubArt.SanguineGrip));
            Assert.AreEqual(Element.Water, r.ChosenElement);
        }

        [Test]
        public void HighRoll_RevealsBaseElement()
        {
            var rng = new ScriptedRandomSource(0.5, 0.9);
            var r = CharacterCreationService.CreateChanneler(Element.Earth, rng, Config());
            Assert.AreEqual(RevealTier.BaseElement, r.Tier);
            Assert.AreEqual(0, r.Loadout.SubArts.Count);
        }

        [Test]
        public void Weapon_RevealsWeaponTier()
        {
            var r = CharacterCreationService.CreateWeaponUser(WeaponType.LongBow);
            Assert.AreEqual(RevealTier.Weapon, r.Tier);
            Assert.IsFalse(r.Loadout.IsChanneler);
            Assert.AreEqual(WeaponType.LongBow, r.Loadout.Weapon);
        }
    }
}
