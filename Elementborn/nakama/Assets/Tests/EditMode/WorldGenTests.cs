using NUnit.Framework;
using System.Linq;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class WorldGenTests
    {
        private static WorldMap Gen(int seed, int regions = 14)
            => WorldGenerator.Generate(new SystemRandomSource(seed), new WorldGenConfig { RegionCount = regions });

        [Test]
        public void ProducesRequestedRegionCount() => Assert.AreEqual(14, Gen(1).Regions.Count);

        [Test]
        public void IsDeterministicForSameSeed()
        {
            var a = Gen(42); var b = Gen(42);
            CollectionAssert.AreEqual(a.Regions.Select(r => r.Name).ToList(), b.Regions.Select(r => r.Name).ToList());
            CollectionAssert.AreEqual(a.Regions.Select(r => r.Biome).ToList(), b.Regions.Select(r => r.Biome).ToList());
        }

        [Test]
        public void DifferentSeedsProduceDifferentWorlds()
        {
            var a = Gen(1).Regions.Select(r => r.Name + r.Biome).ToList();
            var b = Gen(2).Regions.Select(r => r.Name + r.Biome).ToList();
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void HasExactlyOneCapital()
        {
            var world = Gen(7);
            Assert.AreEqual(1, world.Regions.Count(r => r.Biome == BiomeType.CapitalCity));
            Assert.IsNotNull(world.Capital());
        }

        [Test]
        public void EveryRegionIsNamedAndHasPois()
        {
            foreach (var r in Gen(3).Regions)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(r.Name));
                Assert.IsNotEmpty(r.Pois);
            }
        }

        [Test]
        public void RegionNamesAreUnique()
        {
            var names = Gen(9).Regions.Select(r => r.Name).ToList();
            Assert.AreEqual(names.Count, names.Distinct().Count());
        }

        [Test]
        public void NeighborsAreSymmetric()
        {
            var world = Gen(5);
            foreach (var r in world.Regions)
                foreach (var nId in r.Neighbors)
                    Assert.Contains(r.Id, world.Get(nId).Neighbors);
        }

        [Test]
        public void CapitalIsLowestDanger() => Assert.AreEqual(1, Gen(11).Capital().DangerLevel);

        [Test]
        public void WeaponCachesCarryRealWeapons()
        {
            foreach (var cache in Gen(4).WeaponCaches())
                Assert.AreNotEqual(WeaponType.None, cache.WeaponType);
        }

        [Test]
        public void AllBiomesAreDefinedEnumValues()
        {
            foreach (var r in Gen(6).Regions)
                Assert.IsTrue(System.Enum.IsDefined(typeof(BiomeType), r.Biome));
        }
    }
}
