using NUnit.Framework;
using System;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CreatureCatalogTests
    {
        [Test]
        public void EveryKindHasValidInfo()
        {
            foreach (CreatureKind k in Enum.GetValues(typeof(CreatureKind)))
            {
                var info = CreatureCatalog.For(k);
                Assert.IsFalse(string.IsNullOrEmpty(info.Name), $"{k} name");
                Assert.Greater(info.TameChance, 0f, $"{k} tame chance");
                Assert.LessOrEqual(info.TameChance, 1f, $"{k} tame chance");
                Assert.GreaterOrEqual(info.Price, 0, $"{k} price");
                if (info.Purchasable) Assert.Greater(info.Price, 0, $"{k} purchasable price");
            }
        }

        [Test]
        public void CompanionsAreFlaggedAndTameOnly()
        {
            var companions = new[]
            {
                CreatureKind.Spider, CreatureKind.WaterCat, CreatureKind.IceCat,
                CreatureKind.Phoenix, CreatureKind.ElectricSquirrel, CreatureKind.Dog
            };
            foreach (var c in companions)
            {
                var info = CreatureCatalog.For(c);
                Assert.IsTrue(info.IsCompanion, $"{c} should be a companion");
                Assert.IsFalse(info.Purchasable, $"{c} should be tame-only");
            }

            Assert.IsFalse(CreatureCatalog.For(CreatureKind.FireDragon).IsCompanion);
            Assert.IsTrue(CreatureCatalog.For(CreatureKind.FireDragon).Purchasable);
        }

        [Test]
        public void RideableFlagsMakeSense()
        {
            Assert.IsTrue(CreatureCatalog.For(CreatureKind.FireDragon).Rideable);
            Assert.IsTrue(CreatureCatalog.For(CreatureKind.Horse).Rideable);
            Assert.IsFalse(CreatureCatalog.For(CreatureKind.EarthCat).Rideable);
        }

        [Test]
        public void ElementLockedCreaturesRequireTheirElement()
        {
            Assert.AreEqual(Element.Fire, CreatureCatalog.For(CreatureKind.FireDragon).RequiredElement);
            Assert.AreEqual(Element.Water, CreatureCatalog.For(CreatureKind.Mermaid).RequiredElement);
            Assert.AreEqual(Element.Air, CreatureCatalog.For(CreatureKind.AirJellyfish).RequiredElement);
        }
    }
}
