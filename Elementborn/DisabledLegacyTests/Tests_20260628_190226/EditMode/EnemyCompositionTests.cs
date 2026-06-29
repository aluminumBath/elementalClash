using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class EnemyCompositionTests
    {
        [Test]
        public void BiomeElementMapsExpectedBiomes()
        {
            Assert.AreEqual(Element.Fire, EnemyComposition.BiomeElement(BiomeType.Volcano));
            Assert.AreEqual(Element.Earth, EnemyComposition.BiomeElement(BiomeType.Desert));
            Assert.AreEqual(Element.Air, EnemyComposition.BiomeElement(BiomeType.CloudTemple));
            Assert.IsFalse(EnemyComposition.BiomeElement(BiomeType.Plains).HasValue);
        }

        [Test]
        public void PickIsDeterministic()
        {
            var a = EnemyComposition.Pick(BiomeType.Volcano, 3, new SystemRandomSource(7));
            var b = EnemyComposition.Pick(BiomeType.Volcano, 3, new SystemRandomSource(7));
            Assert.AreEqual(a.Faction, b.Faction);
            Assert.AreEqual(a.Element, b.Element);
            Assert.AreEqual(a.Kind, b.Kind);
        }

        [Test]
        public void WildAlwaysHasElementAndBanditNeverDoes()
        {
            var biomes = new[] { BiomeType.Volcano, BiomeType.Desert, BiomeType.Plains, BiomeType.CloudTemple, BiomeType.Swamp };
            for (int seed = 0; seed < 200; seed++)
            {
                var biome = biomes[seed % biomes.Length];
                int danger = (seed % 5) + 1;
                var p = EnemyComposition.Pick(biome, danger, new SystemRandomSource(seed));

                if (p.Faction == Faction.Wild) Assert.IsTrue(p.Element.HasValue, "Wild must have an element");
                if (p.Faction == Faction.Bandit)
                {
                    Assert.IsFalse(p.Element.HasValue, "Bandit must not have an element");
                    Assert.AreNotEqual(EnemyKind.Elementalist, p.Kind, "Bandit can't be an Elementalist");
                }
            }
        }

        [Test]
        public void VolcanoProducesFireWilds()
        {
            bool anyFireWild = false;
            for (int seed = 0; seed < 200 && !anyFireWild; seed++)
            {
                var p = EnemyComposition.Pick(BiomeType.Volcano, 5, new SystemRandomSource(seed));
                if (p.Faction == Faction.Wild && p.Element == Element.Fire) anyFireWild = true;
            }
            Assert.IsTrue(anyFireWild);
        }
    }
}
