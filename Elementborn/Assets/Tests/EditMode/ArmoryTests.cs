using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ArmoryTests
    {
        // ---- stones & fusion ----
        [Test]
        public void DefeatedChannelerDropsTheirElementStone()
        {
            var s = StoneDrops.ForDefeatedChanneler(Element.Fire);
            Assert.AreEqual(Element.Fire, s.Primary);
            Assert.IsFalse(s.IsFused);
        }

        [Test]
        public void TwoDifferentStonesFuseIntoADualStone()
        {
            Assert.IsTrue(ElementalStone.TryFuse(ElementalStone.Of(Element.Fire), ElementalStone.Of(Element.Water), out var fused));
            Assert.IsTrue(fused.IsFused);
            Assert.AreEqual(Element.Fire, fused.Primary);
            Assert.AreEqual(Element.Water, fused.Secondary);
        }

        [Test]
        public void CannotFuseTwoOfTheSameElement()
        {
            Assert.IsFalse(ElementalStone.TryFuse(ElementalStone.Of(Element.Air), ElementalStone.Of(Element.Air), out _));
        }

        [Test]
        public void CannotFuseAnAlreadyFusedStone() // only two at a time, never undone
        {
            ElementalStone.TryFuse(ElementalStone.Of(Element.Fire), ElementalStone.Of(Element.Water), out var dual);
            Assert.IsFalse(ElementalStone.TryFuse(dual, ElementalStone.Of(Element.Earth), out _));
            Assert.IsFalse(ElementalStone.TryFuse(ElementalStone.Of(Element.Earth), dual, out _));
        }

        // ---- imbuement ----
        [Test]
        public void ImbuingUpgradesDamageDurabilityAndAddsAnEffect()
        {
            var w = Imbuement.Imbue(WeaponType.Sword, ElementalStone.Of(Element.Fire));
            Assert.AreEqual(WeaponType.Sword, w.BaseType);
            Assert.Greater(w.DamageBonus, 0f);
            Assert.Greater(w.DurabilityBonus, 0f);
            Assert.AreEqual(StatusKind.Burn, w.Effect);
        }

        [Test]
        public void FusedStoneImbuesHarderThanASingle()
        {
            ElementalStone.TryFuse(ElementalStone.Of(Element.Fire), ElementalStone.Of(Element.Water), out var dual);
            float single = Imbuement.Imbue(WeaponType.Sword, ElementalStone.Of(Element.Fire)).DamageBonus;
            float fused = Imbuement.Imbue(WeaponType.Sword, dual).DamageBonus;
            Assert.Greater(fused, single);
        }

        [Test]
        public void EachElementGivesItsImbueEffect()
        {
            Assert.AreEqual(StatusKind.Burn, Imbuement.EffectFor(Element.Fire));
            Assert.AreEqual(StatusKind.Slow, Imbuement.EffectFor(Element.Water));
            Assert.AreEqual(StatusKind.Control, Imbuement.EffectFor(Element.Earth));
            Assert.AreEqual(StatusKind.None, Imbuement.EffectFor(Element.Air));
        }

        // ---- wands ----
        [Test]
        public void SingleElementWandGivesThatElementsSpellsAndBlocksItems()
        {
            var wand = WandCrafting.Make(ElementalStone.Of(Element.Fire));
            Assert.IsTrue(wand.BlocksOtherItems);
            Assert.AreEqual(3, wand.Spells.Count);
            foreach (var sp in wand.Spells) Assert.AreEqual(Element.Fire, sp.Element);
        }

        [Test]
        public void FusedWandMergesBothSetsCappedAtSix()
        {
            ElementalStone.TryFuse(ElementalStone.Of(Element.Fire), ElementalStone.Of(Element.Water), out var dual);
            var wand = WandCrafting.Make(dual);
            Assert.LessOrEqual(wand.Spells.Count, WandCrafting.MaxSpells);
            Assert.AreEqual(6, wand.Spells.Count); // 3 fire + 3 water
            bool hasFire = false, hasWater = false;
            foreach (var sp in wand.Spells)
            {
                if (sp.Element == Element.Fire) hasFire = true;
                if (sp.Element == Element.Water) hasWater = true;
            }
            Assert.IsTrue(hasFire && hasWater);
        }

        [Test]
        public void WandNeverExceedsTheSpellCap()
        {
            ElementalStone.TryFuse(ElementalStone.Of(Element.Earth), ElementalStone.Of(Element.Air), out var dual);
            Assert.LessOrEqual(WandCrafting.Make(dual).Spells.Count, WandCrafting.MaxSpells);
        }

        // ---- smith ----
        [Test]
        public void SmithOffersRealWeaponsNotNone()
        {
            Assert.Greater(WeaponSmith.Offered.Length, 0);
            foreach (var t in WeaponSmith.Offered) Assert.AreNotEqual(WeaponType.None, t);
        }
    }
}
