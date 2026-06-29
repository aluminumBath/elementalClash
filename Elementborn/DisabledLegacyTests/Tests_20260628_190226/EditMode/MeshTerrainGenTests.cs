using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class MeshTerrainGenTests
    {
        private static TerrainModel RampModel(int res, float sea)
        {
            var h = new float[res, res];
            var b = new BiomeType[res, res];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++)
                {
                    h[z, x] = (x + z) / (float)(2 * (res - 1)); // 0..1 ramp
                    b[z, x] = BiomeType.Plains;
                }
            return new TerrainModel(res, h, b, sea);
        }

        [Test]
        public void ProducesChunksCoveringEveryQuad()
        {
            var model = RampModel(5, 0f); // 4x4 = 16 quads
            var chunks = MeshTerrainGenerator.Build(model, 100f, 50f, 2);

            Assert.AreEqual(4, chunks.Count); // 2x2 chunks

            int tris = 0, verts = 0;
            foreach (var c in chunks)
            {
                Assert.AreEqual(c.Vertices.Length, c.Colors.Length);
                Assert.AreEqual(c.Vertices.Length, c.Normals.Length);
                Assert.AreEqual(c.Vertices.Length, c.Triangles.Length);
                foreach (var idx in c.Triangles) Assert.Less(idx, c.Vertices.Length);
                tris += c.Triangles.Length / 3;
                verts += c.Vertices.Length;
            }
            Assert.AreEqual(16 * 2, tris);  // 2 triangles per quad
            Assert.AreEqual(16 * 6, verts); // 6 unique verts per quad (flat shading)
        }

        [Test]
        public void AllFacesPointUp()
        {
            var chunks = MeshTerrainGenerator.Build(RampModel(9, 0f), 200f, 80f, 4);
            foreach (var c in chunks)
                foreach (var n in c.Normals)
                    Assert.GreaterOrEqual(n.y, 0f);
        }

        [Test]
        public void UnderwaterCellsUseSeabedColor()
        {
            int res = 5;
            var h = new float[res, res];
            var b = new BiomeType[res, res];
            for (int z = 0; z < res; z++)
                for (int x = 0; x < res; x++) { h[z, x] = 0.05f; b[z, x] = BiomeType.Plains; }
            var model = new TerrainModel(res, h, b, 0.12f);

            var chunks = MeshTerrainGenerator.Build(model, 100f, 50f, 4);
            foreach (var c in chunks)
                foreach (var col in c.Colors)
                    Assert.AreEqual(TerrainColors.Seabed.r, col.r);
        }

        [Test]
        public void SampleHeightMatchesCornersAndInterpolates()
        {
            var model = RampModel(5, 0f); // height = (x+z)/8
            Assert.AreEqual(0f, MeshTerrainGenerator.SampleHeight01(model, 0f, 0f), 1e-4f);
            Assert.AreEqual(1f, MeshTerrainGenerator.SampleHeight01(model, 1f, 1f), 1e-4f);
            Assert.AreEqual(0.5f, MeshTerrainGenerator.SampleHeight01(model, 0.5f, 0.5f), 1e-4f);
        }
    }
}
