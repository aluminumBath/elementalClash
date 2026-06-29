using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>Per-biome terrain shaping parameters (in normalised 0..1 height space).</summary>
    public readonly struct BiomeTerrainProfile
    {
        public readonly float BaseHeight; // target ground height for the biome
        public readonly float Roughness;  // noise amplitude
        public readonly float Peak;       // cone strength at the region centre (mountains/volcanoes)

        public BiomeTerrainProfile(float baseHeight, float roughness, float peak)
        {
            BaseHeight = baseHeight; Roughness = roughness; Peak = peak;
        }
    }

    /// <summary>Maps each biome to a terrain profile. Tweak here to reshape the world's look.</summary>
    public static class TerrainProfiles
    {
        public static BiomeTerrainProfile For(BiomeType biome) => biome switch
        {
            BiomeType.CapitalCity  => new BiomeTerrainProfile(0.32f, 0.02f, 0.00f), // flat, buildable
            BiomeType.Plains       => new BiomeTerrainProfile(0.30f, 0.03f, 0.00f),
            BiomeType.Mountains    => new BiomeTerrainProfile(0.55f, 0.18f, 0.35f),
            BiomeType.Volcano      => new BiomeTerrainProfile(0.55f, 0.10f, 0.55f), // tall cone + crater
            BiomeType.Desert       => new BiomeTerrainProfile(0.28f, 0.08f, 0.05f), // dunes
            BiomeType.ForestTemple => new BiomeTerrainProfile(0.40f, 0.10f, 0.10f), // rolling
            BiomeType.Swamp        => new BiomeTerrainProfile(0.16f, 0.02f, 0.00f), // low, flat
            BiomeType.Marsh        => new BiomeTerrainProfile(0.15f, 0.02f, 0.00f),
            BiomeType.Beach        => new BiomeTerrainProfile(0.11f, 0.02f, 0.00f), // just above sea
            BiomeType.Island       => new BiomeTerrainProfile(0.30f, 0.06f, 0.15f),
            BiomeType.CoralReefForest => new BiomeTerrainProfile(0.26f, 0.09f, 0.12f), // raised coral shelves
            BiomeType.CloudTemple  => new BiomeTerrainProfile(0.70f, 0.05f, 0.10f), // high plateau
            _                      => new BiomeTerrainProfile(0.30f, 0.03f, 0.00f)
        };
    }

    /// <summary>Tunable parameters for terrain generation.</summary>
    public sealed class TerrainGenConfig
    {
        public int Resolution = 513;          // heightmap resolution (use a power of two + 1)
        public float WorldSize = 1200f;       // world XZ extent the terrain covers (match WorldGenConfig.MapSize)
        public float SeaLevel = 0.12f;        // normalised water line
        public float ShoreDepth = 0.10f;      // how far below sea the open seabed sits
        public float InfluenceFalloff = 2.0f; // a region's influence reaches radius * falloff
        public float NoiseScale = 0.004f;     // world units -> noise frequency
        public int NoiseOctaves = 4;
    }

    /// <summary>A generated heightmap + dominant-biome map (normalised). Pure data; no Unity Terrain.</summary>
    public sealed class TerrainModel
    {
        public int Resolution { get; }
        public float[,] Heights { get; }    // [z, x], 0..1 (matches Unity's SetHeights layout)
        public BiomeType[,] Biomes { get; } // [z, x] dominant biome per cell
        public float SeaLevel { get; }

        public TerrainModel(int resolution, float[,] heights, BiomeType[,] biomes, float seaLevel)
        {
            Resolution = resolution; Heights = heights; Biomes = biomes; SeaLevel = seaLevel;
        }

        public float HeightAt(int x, int z) => Heights[z, x];
        public BiomeType BiomeAt(int x, int z) => Biomes[z, x];
        public bool IsWater(int x, int z) => Heights[z, x] < SeaLevel;

        public float WaterFraction()
        {
            int water = 0, total = Resolution * Resolution;
            for (int z = 0; z < Resolution; z++)
                for (int x = 0; x < Resolution; x++)
                    if (Heights[z, x] < SeaLevel) water++;
            return total > 0 ? water / (float)total : 0f;
        }
    }

    /// <summary>
    /// Turns a <see cref="WorldMap"/> into terrain: each region projects its biome's height profile
    /// onto a grid (blended by distance so biomes transition), volcanoes get cones with craters, and
    /// areas with no region fall to the seabed so regions read as landmasses. Fractal noise adds
    /// detail. Deterministic and pure, so it is unit-tested. The Unity Terrain object is built
    /// separately by TerrainBuilder.
    /// </summary>
    public static class TerrainGenerator
    {
        public static TerrainModel Generate(WorldMap world, IRandomSource rng, TerrainGenConfig cfg)
        {
            cfg ??= new TerrainGenConfig();
            int res = Mathf.Max(2, cfg.Resolution);
            var heights = new float[res, res];
            var biomes = new BiomeType[res, res];
            float seaFloor = Mathf.Max(0f, cfg.SeaLevel - cfg.ShoreDepth);
            float noiseOffset = (float)(rng.NextUnit() * 1000.0);
            var regions = world.Regions;

            for (int zi = 0; zi < res; zi++)
            {
                float v = zi / (float)(res - 1);
                for (int xi = 0; xi < res; xi++)
                {
                    float u = xi / (float)(res - 1);
                    var wp = new Vector2(u * cfg.WorldSize, v * cfg.WorldSize);

                    float totalW = 0f, baseSum = 0f, roughSum = 0f, peak = 0f, bestW = -1f;
                    BiomeType dom = BiomeType.Beach;

                    foreach (var r in regions)
                    {
                        float d = Vector2.Distance(wp, r.MapPosition);
                        float infl = Influence(d, r.Radius, cfg.InfluenceFalloff);
                        if (infl <= 0f) continue;

                        var p = TerrainProfiles.For(r.Biome);
                        totalW += infl;
                        baseSum += p.BaseHeight * infl;
                        roughSum += p.Roughness * infl;

                        float cone = p.Peak * Mathf.Clamp01(1f - d / Mathf.Max(1f, r.Radius));
                        if (r.Biome == BiomeType.Volcano) // crater dip at the very centre
                            cone -= p.Peak * 0.55f * Mathf.Clamp01(1f - d / Mathf.Max(1f, r.Radius * 0.25f));
                        peak += cone * infl;

                        if (infl > bestW) { bestW = infl; dom = r.Biome; }
                    }

                    float coverage = Mathf.Clamp01(totalW);
                    float landBase = totalW > 0f ? baseSum / totalW : 0f;
                    float rough = totalW > 0f ? roughSum / totalW : 0f;

                    float n = FractalNoise(wp * cfg.NoiseScale, noiseOffset, cfg.NoiseOctaves);
                    float h = Mathf.Lerp(seaFloor, landBase, coverage)
                              + (n - 0.5f) * 2f * rough * coverage
                              + peak;

                    heights[zi, xi] = Mathf.Clamp01(h);
                    biomes[zi, xi] = dom;
                }
            }

            return new TerrainModel(res, heights, biomes, cfg.SeaLevel);
        }

        // Smooth 1-at-centre to 0-at-(radius*falloff) influence curve.
        private static float Influence(float d, float radius, float falloff)
        {
            float outer = Mathf.Max(radius + 1f, radius * falloff);
            if (d >= outer) return 0f;
            if (d <= radius) return 1f;
            float t = (d - radius) / (outer - radius);
            return 1f - Mathf.SmoothStep(0f, 1f, t);
        }

        private static float FractalNoise(Vector2 p, float seedOffset, int octaves)
        {
            float sum = 0f, amp = 1f, freq = 1f, norm = 0f;
            for (int o = 0; o < Mathf.Max(1, octaves); o++)
            {
                sum += amp * Mathf.PerlinNoise(p.x * freq + seedOffset, p.y * freq + seedOffset);
                norm += amp; amp *= 0.5f; freq *= 2f;
            }
            return norm > 0f ? sum / norm : 0.5f;
        }
    }
}
