using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class MeshTerrainBlendTests
    {
        // Left half Plains, right half Desert; flat and above sea level.
        private static TerrainModel TwoBiome(int res)
        {
            var h = new float[res, res];
            var b = new BiomeType[res, res];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    h[z, x] = 0.5f;
                    b[z, x] = x < res / 2 ? BiomeType.Plains : BiomeType.Desert;
                }
            return new TerrainModel(res, h, b, 0f);
        }

        [Test]
        public void BlendProducesIntermediateColorsAtBoundary()
        {
            var model = TwoBiome(7);
            byte plains = TerrainColors.ForBiome(BiomeType.Plains).r;
            byte desert = TerrainColors.ForBiome(BiomeType.Desert).r;

            bool intermediate = false;
            foreach (var c in MeshTerrainGenerator.Build(model, 100f, 10f, 3, blend: true))
                foreach (var col in c.Colors)
                    if (col.r != plains && col.r != desert) intermediate = true;

            Assert.IsTrue(intermediate, "blending should yield vertex colours between the two biomes");
        }

        [Test]
        public void NoBlendKeepsCrispPerQuadColors()
        {
            var model = TwoBiome(7);
            byte plains = TerrainColors.ForBiome(BiomeType.Plains).r;
            byte desert = TerrainColors.ForBiome(BiomeType.Desert).r;

            foreach (var c in MeshTerrainGenerator.Build(model, 100f, 10f, 3, blend: false))
                foreach (var col in c.Colors)
                    Assert.IsTrue(col.r == plains || col.r == desert,
                        "without blending every vertex should keep an exact biome colour");
        }
    }
}
