using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CompanionProfileTests
    {
        private static DamageInfo Hit(Element e, AbilityVariant v) => new DamageInfo(10f, e, v);

        [Test]
        public void WaterCatShrugsWaterButNotIce()
        {
            var imm = CompanionProfiles.For(CreatureKind.WaterCat).Immunity;
            Assert.IsTrue(imm.Blocks(Hit(Element.Water, AbilityVariant.Standard)));
            Assert.IsFalse(imm.Blocks(Hit(Element.Water, AbilityVariant.Ice)));   // ice pierces
            Assert.IsFalse(imm.Blocks(Hit(Element.Fire, AbilityVariant.Standard)));
        }

        [Test]
        public void IceCatShrugsIceButNotWater()
        {
            var imm = CompanionProfiles.For(CreatureKind.IceCat).Immunity;
            Assert.IsTrue(imm.Blocks(Hit(Element.Water, AbilityVariant.Ice)));
            Assert.IsFalse(imm.Blocks(Hit(Element.Water, AbilityVariant.Standard))); // water hurts
            Assert.IsFalse(imm.Blocks(Hit(Element.Fire, AbilityVariant.Standard)));
        }

        [Test]
        public void PhoenixIsImmuneToFire()
        {
            var imm = CompanionProfiles.For(CreatureKind.Phoenix).Immunity;
            Assert.IsTrue(imm.Blocks(Hit(Element.Fire, AbilityVariant.Standard)));
            Assert.IsTrue(imm.Blocks(Hit(Element.Fire, AbilityVariant.Lightning)));
            Assert.IsFalse(imm.Blocks(Hit(Element.Water, AbilityVariant.Standard)));
        }

        [Test]
        public void NoneBlocksNothing()
        {
            var imm = DamageImmunity.None;
            Assert.IsFalse(imm.Blocks(Hit(Element.Fire, AbilityVariant.Standard)));
            Assert.IsFalse(imm.Blocks(Hit(Element.Water, AbilityVariant.Ice)));
        }

        [Test]
        public void ProfileTricksAndStatusesAreSet()
        {
            Assert.IsTrue(CompanionProfiles.For(CreatureKind.Spider).CanWeb);
            Assert.IsTrue(CompanionProfiles.For(CreatureKind.WaterCat).CanBlink);
            Assert.IsTrue(CompanionProfiles.For(CreatureKind.Dog).CanBlink);
            Assert.IsTrue(CompanionProfiles.For(CreatureKind.Phoenix).CanRebirth);
            Assert.AreEqual(StatusKind.Stun, CompanionProfiles.For(CreatureKind.ElectricSquirrel).OnHitStatus);
            Assert.AreEqual(StatusKind.Slow, CompanionProfiles.For(CreatureKind.IceCat).OnHitStatus);
        }

        [Test]
        public void CompanionSpawnsAreCompanionsAndDeterministic()
        {
            Assert.AreEqual(CreatureKind.Phoenix, CompanionSpawns.For(BiomeType.Volcano, new SystemRandomSource(1)));

            for (int seed = 0; seed < 60; seed++)
            {
                var k = CompanionSpawns.For(BiomeType.Mountains, new SystemRandomSource(seed));
                if (k.HasValue) Assert.IsTrue(CreatureCatalog.For(k.Value).IsCompanion);
            }
        }
    }
}
