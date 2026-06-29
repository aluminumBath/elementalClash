using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>Per-biome flat colours for the vertex-coloured low-poly terrain (bright cel-shaded toon palette).</summary>
    public static class TerrainColors
    {
        public static Color32 Seabed => new Color32(190, 180, 150, 255); // wet sand under the water plane

        public static Color32 ForBiome(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Plains:       return new Color32(110, 165,  75, 255);
                case BiomeType.CapitalCity:  return new Color32(132, 150, 112, 255);
                case BiomeType.Mountains:    return new Color32(130, 120, 110, 255);
                case BiomeType.Volcano:      return new Color32( 72,  62,  62, 255);
                case BiomeType.Desert:       return new Color32(210, 190, 130, 255);
                case BiomeType.ForestTemple: return new Color32( 70, 130,  70, 255);
                case BiomeType.Swamp:        return new Color32( 90, 110,  80, 255);
                case BiomeType.Marsh:        return new Color32(110, 120,  85, 255);
                case BiomeType.Beach:        return new Color32(225, 210, 160, 255);
                case BiomeType.Island:       return new Color32(100, 160,  90, 255);
                case BiomeType.CoralReefForest: return new Color32(120, 205, 180, 255);
                case BiomeType.CloudTemple:  return new Color32(180, 190, 205, 255);
                default:                     return new Color32(110, 165,  75, 255);
            }
        }
    }

    /// <summary>One terrain chunk's mesh data: flat-shaded, vertex-coloured, ready for a Unity Mesh.</summary>
    public readonly struct MeshChunkData
    {
        public readonly Vector3[] Vertices;
        public readonly Vector3[] Normals;
        public readonly Color32[] Colors;
        public readonly int[] Triangles;

        public MeshChunkData(Vector3[] vertices, Vector3[] normals, Color32[] colors, int[] triangles)
        {
            Vertices = vertices; Normals = normals; Colors = colors; Triangles = triangles;
        }
    }

    /// <summary>
    /// Turns a <see cref="TerrainModel"/> into faceted, vertex-coloured low-poly chunk meshes for the
    /// Wind-Waker ground (each triangle gets its own vertices and a flat normal, so the toon shader bands
    /// and outline read crisply). Pure and deterministic, so it is unit-tested; the Unity Mesh objects are
    /// built by MeshTerrainBuilder. Positions are in the terrain's local space, spanning [0, worldSize].
    /// </summary>
    public static class MeshTerrainGenerator
    {
        public static List<MeshChunkData> Build(TerrainModel model, float worldSize, float heightScale, int chunkCells, bool blend = true)
        {
            var chunks = new List<MeshChunkData>();
            if (model == null) return chunks;

            int cells = model.Resolution - 1; // quads per axis
            if (cells < 1) return chunks;

            chunkCells = Mathf.Clamp(chunkCells, 1, cells);
            float step = worldSize / cells;

            for (int cz = 0; cz < cells; cz += chunkCells)
                for (int cx = 0; cx < cells; cx += chunkCells)
                {
                    int x1 = Mathf.Min(cx + chunkCells, cells);
                    int z1 = Mathf.Min(cz + chunkCells, cells);
                    chunks.Add(BuildChunk(model, cx, cz, x1, z1, step, heightScale, blend));
                }

            return chunks;
        }

        private static MeshChunkData BuildChunk(TerrainModel model, int x0, int z0, int x1, int z1,
            float step, float heightScale, bool blend)
        {
            int quads = (x1 - x0) * (z1 - z0);
            int n = quads * 6; // two triangles, three unique vertices each (flat shading)
            var verts = new Vector3[n];
            var norms = new Vector3[n];
            var cols = new Color32[n];
            var tris = new int[n];
            int vi = 0;

            for (int z = z0; z < z1; z++)
                for (int x = x0; x < x1; x++)
                {
                    Vector3 p00 = Corner(model, x, z, step, heightScale);
                    Vector3 p10 = Corner(model, x + 1, z, step, heightScale);
                    Vector3 p01 = Corner(model, x, z + 1, step, heightScale);
                    Vector3 p11 = Corner(model, x + 1, z + 1, step, heightScale);

                    Color32 c00, c10, c01, c11;
                    if (blend)
                    {
                        c00 = VertexColor(model, x, z);
                        c10 = VertexColor(model, x + 1, z);
                        c01 = VertexColor(model, x, z + 1);
                        c11 = VertexColor(model, x + 1, z + 1);
                    }
                    else
                    {
                        c00 = c10 = c01 = c11 = CellColor(model, x, z);
                    }

                    AddTriangle(verts, norms, cols, tris, ref vi, p00, p01, p11, c00, c01, c11);
                    AddTriangle(verts, norms, cols, tris, ref vi, p00, p11, p10, c00, c11, c10);
                }

            return new MeshChunkData(verts, norms, cols, tris);
        }

        private static Vector3 Corner(TerrainModel model, int x, int z, float step, float heightScale)
        {
            int cx = Mathf.Clamp(x, 0, model.Resolution - 1);
            int cz = Mathf.Clamp(z, 0, model.Resolution - 1);
            return new Vector3(x * step, model.HeightAt(cx, cz) * heightScale, z * step);
        }

        private static Color32 CellColor(TerrainModel model, int x, int z) =>
            model.HeightAt(x, z) < model.SeaLevel ? TerrainColors.Seabed : TerrainColors.ForBiome(model.BiomeAt(x, z));

        // Average of the (up to four) cell colours meeting at grid corner (x,z); softens biome/shoreline seams.
        private static Color32 VertexColor(TerrainModel model, int x, int z)
        {
            int lastCell = model.Resolution - 2; // cells are sampled at their lower-left vertex, 0..Resolution-2
            if (lastCell < 0) return CellColor(model, 0, 0);

            int r = 0, g = 0, b = 0, count = 0;
            for (int dz = -1; dz <= 0; dz++)
                for (int dx = -1; dx <= 0; dx++)
                {
                    int cx = Mathf.Clamp(x + dx, 0, lastCell);
                    int cz = Mathf.Clamp(z + dz, 0, lastCell);
                    Color32 c = CellColor(model, cx, cz);
                    r += c.r; g += c.g; b += c.b; count++;
                }
            return new Color32((byte)(r / count), (byte)(g / count), (byte)(b / count), 255);
        }

        // Emits one triangle with its own three vertices, a flat normal, and per-vertex colours; flips
        // winding (and the matching colours) so the face points up.
        private static void AddTriangle(Vector3[] verts, Vector3[] norms, Color32[] cols, int[] tris,
            ref int vi, Vector3 a, Vector3 b, Vector3 c, Color32 ca, Color32 cb, Color32 cc)
        {
            Vector3 nrm = Vector3.Cross(b - a, c - a).normalized;
            if (nrm.y < 0f)
            {
                var tmp = b; b = c; c = tmp;
                var tc = cb; cb = cc; cc = tc;
                nrm = -nrm;
            }

            verts[vi] = a; verts[vi + 1] = b; verts[vi + 2] = c;
            norms[vi] = norms[vi + 1] = norms[vi + 2] = nrm;
            cols[vi] = ca; cols[vi + 1] = cb; cols[vi + 2] = cc;
            tris[vi] = vi; tris[vi + 1] = vi + 1; tris[vi + 2] = vi + 2;
            vi += 3;
        }

        /// <summary>Bilinear height (0..1) at normalised position (u,v) — used for ground snapping.</summary>
        public static float SampleHeight01(TerrainModel model, float u, float v)
        {
            int res = model.Resolution;
            float fx = Mathf.Clamp01(u) * (res - 1);
            float fz = Mathf.Clamp01(v) * (res - 1);
            int x0 = Mathf.FloorToInt(fx), z0 = Mathf.FloorToInt(fz);
            int x1 = Mathf.Min(x0 + 1, res - 1), z1 = Mathf.Min(z0 + 1, res - 1);
            float tx = fx - x0, tz = fz - z0;

            float a = Mathf.Lerp(model.HeightAt(x0, z0), model.HeightAt(x1, z0), tx);
            float b = Mathf.Lerp(model.HeightAt(x0, z1), model.HeightAt(x1, z1), tx);
            return Mathf.Lerp(a, b, tz);
        }
    }
}
