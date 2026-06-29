using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class TerrainGenTests
    {
        private static WorldMap World(int seed = 1) =>
            WorldGenerator.Generate(new SystemRandomSource(seed), new WorldGenConfig { RegionCount = 14 });

        private static TerrainModel Terrain(int seed = 1, int res = 129) =>
            TerrainGenerator.Generate(World(seed), new SystemRandomSource(seed),
                new TerrainGenConfig { Resolution = res });

        [Test]
        public void ResolutionMatchesConfig() => Assert.AreEqual(129, Terrain(1, 129).Resolution);

        [Test]
        public void AllHeightsAreNormalised()
        {
            var t = Terrain();
            for (int z = 0; z < t.Resolution; z++)
                for (int x = 0; x < t.Resolution; x++)
                {
                    float h = t.HeightAt(x, z);
                    Assert.GreaterOrEqual(h, 0f);
                    Assert.LessOrEqual(h, 1f);
                }
        }

        [Test]
        public void IsDeterministicForSameSeed()
        {
            var a = Terrain(7);
            var b = Terrain(7);
            for (int z = 0; z < a.Resolution; z++)
                for (int x = 0; x < a.Resolution; x++)
                    Assert.AreEqual(a.HeightAt(x, z), b.HeightAt(x, z), 1e-6f);
        }

        [Test]
        public void HasBothLandAndWater()
        {
            float water = Terrain().WaterFraction();
            Assert.Greater(water, 0f); // open sea at the edges
            Assert.Less(water, 1f);    // regions are land
        }

        [Test]
        public void BiomeMapHasValidEnums()
        {
            var t = Terrain();
            for (int z = 0; z < t.Resolution; z += 7)
                for (int x = 0; x < t.Resolution; x += 7)
                    Assert.IsTrue(System.Enum.IsDefined(typeof(BiomeType), t.BiomeAt(x, z)));
        }

        [Test]
        public void DifferentSeedsDifferInShape()
        {
            var a = Terrain(1);
            var b = Terrain(2);
            bool anyDiff = false;
            for (int z = 0; z < a.Resolution && !anyDiff; z += 5)
                for (int x = 0; x < a.Resolution; x += 5)
                    if (Mathf.Abs(a.HeightAt(x, z) - b.HeightAt(x, z)) > 1e-4f) { anyDiff = true; break; }
            Assert.IsTrue(anyDiff);
        }
    }
}
