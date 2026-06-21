using NUnit.Framework;
using System;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class WildlifeTests
    {
        [Test]
        public void IsDeterministic()
        {
            var a = Wildlife.Pick(BiomeType.Volcano, new SystemRandomSource(9));
            var b = Wildlife.Pick(BiomeType.Volcano, new SystemRandomSource(9));
            Assert.AreEqual(a, b);
        }

        [Test]
        public void WildlifeIsNeverACompanion()
        {
            foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
                for (int seed = 0; seed < 50; seed++)
                {
                    var k = Wildlife.Pick(biome, new SystemRandomSource(seed));
                    Assert.IsFalse(CreatureCatalog.For(k).IsCompanion, $"{biome}/{seed} -> {k}");
                }
        }

        [Test]
        public void VolcanoCanSpawnFireDragons()
        {
            bool any = false;
            for (int seed = 0; seed < 100 && !any; seed++)
                if (Wildlife.Pick(BiomeType.Volcano, new SystemRandomSource(seed)) == CreatureKind.FireDragon) any = true;
            Assert.IsTrue(any);
        }

        [Test]
        public void CloudTempleSpawnsAirCreatures()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var k = Wildlife.Pick(BiomeType.CloudTemple, new SystemRandomSource(seed));
                Assert.IsTrue(k == CreatureKind.AirDragonfly || k == CreatureKind.AirJellyfish, $"got {k}");
            }
        }
    }
}
