using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>Tunable parameters for world generation.</summary>
    public sealed class WorldGenConfig
    {
        public int RegionCount = 14;
        public float MapSize = 1200f;          // world map spans MapSize x MapSize (map units)
        public float MinRegionSpacing = 130f;  // rejection-sampling spacing between region centres
        public float RegionRadius = 90f;       // rough region extent (POIs scatter within this)
        public float PoiDensity = 1f;          // scales POI counts per region
        public double WeaponCacheChance = 0.45;// base chance a non-cache POI also holds a weapon
        public int MaxDangerLevel = 5;
    }

    /// <summary>
    /// Deterministic, seeded world builder. Scatters regions on a plane, assigns biomes by position
    /// (coastal vs inland vs remote), names everything from original name pools, links nearest
    /// neighbours, and fills each region with POIs (enemy spawns + weapon caches). Pure logic, so it
    /// is unit-tested. All names are original to avoid IP issues.
    /// </summary>
    public static class WorldGenerator
    {
        public static WorldMap Generate(IRandomSource rng, WorldGenConfig config)
        {
            config ??= new WorldGenConfig();
            int n = Mathf.Max(1, config.RegionCount);

            var positions = ScatterPositions(rng, config, n);
            var biomes = AssignBiomes(positions, config);
            var names = new NameBank();

            var center = new Vector2(config.MapSize * 0.5f, config.MapSize * 0.5f);
            float maxDist = config.MapSize * 0.6f;

            var regions = new List<WorldRegion>(positions.Count);
            for (int i = 0; i < positions.Count; i++)
            {
                int danger = biomes[i] == BiomeType.CapitalCity
                    ? 1
                    : Mathf.Clamp(1 + Mathf.RoundToInt(Vector2.Distance(positions[i], center) / maxDist * (config.MaxDangerLevel - 1)),
                                  1, config.MaxDangerLevel);
                regions.Add(new WorldRegion($"region_{i:00}", names.RegionName(rng, biomes[i]), biomes[i],
                    positions[i], config.RegionRadius, danger));
            }

            ConnectNeighbors(regions, 3);
            PopulatePois(rng, config, regions, names);
            return new WorldMap(regions);
        }

        // ---- placement: Poisson-ish rejection sampling --------------------------------------

        private static List<Vector2> ScatterPositions(IRandomSource rng, WorldGenConfig config, int n)
        {
            var pts = new List<Vector2>(n);
            float lo = config.RegionRadius, hi = config.MapSize - config.RegionRadius;
            float spacing2 = config.MinRegionSpacing * config.MinRegionSpacing;
            int attempts = 0, maxAttempts = n * 200;

            while (pts.Count < n && attempts++ < maxAttempts)
            {
                var p = new Vector2(RangeFloat(rng, lo, hi), RangeFloat(rng, lo, hi));
                bool ok = true;
                foreach (var q in pts)
                    if ((p - q).sqrMagnitude < spacing2) { ok = false; break; }
                if (ok) pts.Add(p);
            }
            while (pts.Count < n) // spacing too tight to satisfy: fill the remainder loosely
                pts.Add(new Vector2(RangeFloat(rng, lo, hi), RangeFloat(rng, lo, hi)));
            return pts;
        }

        // ---- biome assignment driven by position --------------------------------------------

        private static BiomeType[] AssignBiomes(List<Vector2> positions, WorldGenConfig config)
        {
            int n = positions.Count;
            var biome = new BiomeType[n];
            var assigned = new bool[n];
            var center = new Vector2(config.MapSize * 0.5f, config.MapSize * 0.5f);

            // capital = region nearest the centre
            int capital = NearestIndex(positions, center, assigned);
            biome[capital] = BiomeType.CapitalCity; assigned[capital] = true;

            // edgeness = distance to nearest map border (small => coastal)
            float Edge(Vector2 p) => Mathf.Min(Mathf.Min(p.x, config.MapSize - p.x), Mathf.Min(p.y, config.MapSize - p.y));

            var remaining = Enumerable.Range(0, n).Where(i => !assigned[i]).ToList();

            void Take(IEnumerable<int> from, int count, BiomeType b)
            {
                int taken = 0;
                foreach (var i in from)
                {
                    if (taken >= count) break;
                    if (assigned[i]) continue;
                    biome[i] = b; assigned[i] = true; taken++;
                }
            }

            int Per(int divisor) => Mathf.Max(1, n / divisor);

            // coastal -> islands, then beaches
            var coastal = remaining.OrderBy(i => Edge(positions[i]));
            Take(coastal, Per(7), BiomeType.Island);
            // A single giant reef-island: coral has grown into a forest above the sea.
            Take(remaining.Where(i => !assigned[i]).OrderBy(i => Edge(positions[i])), 1, BiomeType.CoralReefForest);
            Take(remaining.Where(i => !assigned[i]).OrderBy(i => Edge(positions[i])), Per(8), BiomeType.Beach);

            // most inland -> volcanoes, then deserts
            Take(remaining.Where(i => !assigned[i]).OrderByDescending(i => Edge(positions[i])), Per(10), BiomeType.Volcano);
            Take(remaining.Where(i => !assigned[i]).OrderByDescending(i => Edge(positions[i])), Per(8), BiomeType.Desert);

            // farthest from the capital -> remote temples
            var capPos = positions[capital];
            Take(remaining.Where(i => !assigned[i]).OrderByDescending(i => Vector2.Distance(positions[i], capPos)), Per(12), BiomeType.CloudTemple);
            Take(remaining.Where(i => !assigned[i]).OrderByDescending(i => Vector2.Distance(positions[i], capPos)), Per(10), BiomeType.ForestTemple);

            // wetlands + connective filler
            Take(remaining.Where(i => !assigned[i]), Per(9), BiomeType.Swamp);
            Take(remaining.Where(i => !assigned[i]), Per(9), BiomeType.Marsh);
            foreach (var i in remaining.Where(i => !assigned[i]))
            {
                biome[i] = Edge(positions[i]) > config.MapSize * 0.25f ? BiomeType.Mountains : BiomeType.Plains;
                assigned[i] = true;
            }
            return biome;
        }

        private static int NearestIndex(List<Vector2> pts, Vector2 to, bool[] skip)
        {
            int best = 0; float bestD = float.MaxValue;
            for (int i = 0; i < pts.Count; i++)
            {
                if (skip[i]) continue;
                float d = (pts[i] - to).sqrMagnitude;
                if (d < bestD) { bestD = d; best = i; }
            }
            return best;
        }

        // ---- connectivity: link each region to its nearest neighbours ------------------------

        private static void ConnectNeighbors(List<WorldRegion> regions, int k)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                var near = regions
                    .Select((r, idx) => (idx, d: (r.MapPosition - regions[i].MapPosition).sqrMagnitude))
                    .Where(t => t.idx != i)
                    .OrderBy(t => t.d)
                    .Take(k)
                    .Select(t => t.idx);
                foreach (var j in near)
                {
                    if (!regions[i].Neighbors.Contains(regions[j].Id)) regions[i].Neighbors.Add(regions[j].Id);
                    if (!regions[j].Neighbors.Contains(regions[i].Id)) regions[j].Neighbors.Add(regions[i].Id); // mutual
                }
            }
        }

        // ---- POIs ---------------------------------------------------------------------------

        private static void PopulatePois(IRandomSource rng, WorldGenConfig config, List<WorldRegion> regions, NameBank names)
        {
            foreach (var region in regions)
            {
                var plan = PoiPlanFor(region.Biome);
                int count = Mathf.Max(1, Mathf.RoundToInt(plan.Count * config.PoiDensity));
                for (int i = 0; i < count; i++)
                {
                    var type = plan[i % plan.Count];

                    float ang = RangeFloat(rng, 0f, Mathf.PI * 2f);
                    float rad = RangeFloat(rng, 0f, region.Radius);
                    var pos = region.MapPosition + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * rad;

                    int enemies = EnemyCountFor(rng, region.DangerLevel, type);

                    bool cache = type == PoiType.WeaponCache || Chance(rng, config.WeaponCacheChance * WeaponLikelihood(type));
                    WeaponType wt = WeaponType.None; WeaponMaterial wm = WeaponMaterial.Wood;
                    if (cache) { wt = RandomWeapon(rng); wm = RandomMaterial(rng, region.DangerLevel); }

                    region.Pois.Add(new PointOfInterest($"{region.Id}_poi_{i:00}", names.PoiName(rng, type), type,
                        pos, enemies, cache, wt, wm));
                }
            }
        }

        private static List<PoiType> PoiPlanFor(BiomeType biome) => biome switch
        {
            BiomeType.CapitalCity  => new List<PoiType> { PoiType.City, PoiType.Market, PoiType.Arena, PoiType.Dock, PoiType.Shrine },
            BiomeType.CloudTemple  => new List<PoiType> { PoiType.Temple, PoiType.Shrine },
            BiomeType.ForestTemple => new List<PoiType> { PoiType.Temple, PoiType.Shrine, PoiType.Landmark },
            BiomeType.Volcano      => new List<PoiType> { PoiType.Dungeon, PoiType.Camp, PoiType.WeaponCache },
            BiomeType.Desert       => new List<PoiType> { PoiType.Landmark, PoiType.Camp, PoiType.WeaponCache },
            BiomeType.Swamp        => new List<PoiType> { PoiType.Dungeon, PoiType.Village },
            BiomeType.Marsh        => new List<PoiType> { PoiType.Village, PoiType.Landmark },
            BiomeType.Beach        => new List<PoiType> { PoiType.Dock, PoiType.Village },
            BiomeType.Island       => new List<PoiType> { PoiType.Dock, PoiType.Landmark, PoiType.WeaponCache },
            BiomeType.CoralReefForest => new List<PoiType> { PoiType.Dock, PoiType.Village, PoiType.Shrine, PoiType.Landmark },
            BiomeType.Mountains    => new List<PoiType> { PoiType.Camp, PoiType.Dungeon },
            _                      => new List<PoiType> { PoiType.Village, PoiType.Camp }
        };

        private static float WeaponLikelihood(PoiType type) => type switch
        {
            PoiType.Dungeon => 1.4f,
            PoiType.Camp => 1.2f,
            PoiType.Market => 1.0f,
            PoiType.Landmark => 0.8f,
            PoiType.Village => 0.6f,
            PoiType.City => 0.5f,
            _ => 0.3f
        };

        private static int EnemyCountFor(IRandomSource rng, int danger, PoiType type)
        {
            int baseline = type switch
            {
                PoiType.Arena => 5,
                PoiType.Dungeon => 4,
                PoiType.Camp => 3,
                PoiType.City => 0,
                PoiType.Market => 0,
                PoiType.Dock => 1,
                PoiType.Landmark => 1,
                _ => 2
            };
            return Mathf.Max(0, baseline + danger - 1 + RangeInt(rng, 0, 2));
        }

        private static WeaponType RandomWeapon(IRandomSource rng)
        {
            var options = new[]
            {
                WeaponType.Hammer, WeaponType.Sword, WeaponType.LongBow,
                WeaponType.Shield, WeaponType.Dagger, WeaponType.Sai
            };
            return options[RangeInt(rng, 0, options.Length)];
        }

        private static WeaponMaterial RandomMaterial(IRandomSource rng, int danger)
        {
            double roll = rng.NextUnit();
            double iceChance = 0.08;
            double metalChance = Mathf.Clamp01(0.12f + danger * 0.08f);
            if (roll < iceChance) return WeaponMaterial.Ice;
            if (roll < iceChance + metalChance) return WeaponMaterial.Metal;
            return WeaponMaterial.Wood;
        }

        // ---- random helpers -----------------------------------------------------------------

        private static int RangeInt(IRandomSource r, int minInclusive, int maxExclusive)
            => maxExclusive <= minInclusive ? minInclusive : minInclusive + (int)(r.NextUnit() * (maxExclusive - minInclusive));
        private static float RangeFloat(IRandomSource r, float min, float max) => min + (float)r.NextUnit() * (max - min);
        private static bool Chance(IRandomSource r, double p) => r.NextUnit() < p;

        // ---- original name pools (consumed so names stay unique within a world) --------------

        private sealed class NameBank
        {
            private readonly Dictionary<BiomeType, List<string>> _regions = BuildRegionPools();
            private readonly Dictionary<PoiType, List<string>> _pois = BuildPoiPools();
            private int _fallback = 1;

            public string RegionName(IRandomSource rng, BiomeType biome)
                => Draw(rng, _regions.TryGetValue(biome, out var p) ? p : null, biome.ToString());

            public string PoiName(IRandomSource rng, PoiType type)
                => Draw(rng, _pois.TryGetValue(type, out var p) ? p : null, type.ToString());

            private string Draw(IRandomSource rng, List<string> pool, string fallbackStem)
            {
                if (pool == null || pool.Count == 0) return $"{fallbackStem} {_fallback++}";
                int idx = (int)(rng.NextUnit() * pool.Count);
                if (idx >= pool.Count) idx = pool.Count - 1;
                string name = pool[idx];
                pool.RemoveAt(idx);
                return name;
            }

            private static Dictionary<BiomeType, List<string>> BuildRegionPools() => new Dictionary<BiomeType, List<string>>
            {
                [BiomeType.CapitalCity]  = L("Aurelmark", "Highholt", "Goldmere Court", "the Spire Seat"),
                [BiomeType.CloudTemple]  = L("Skyhold", "Cloudreach", "Aetherperch", "the Nimbus Cloister", "Stratovault", "Windcrown"),
                [BiomeType.Volcano]      = L("Cinderpeak", "Emberfall", "Ashmaw", "the Vorth Caldera", "Pyrecrown", "Smokereach"),
                [BiomeType.Island]       = L("Saltmere Isle", "Gull's Rest", "Tidewatch", "Coralhold", "Marrow Cay", "the Sunder Isles"),
                [BiomeType.CoralReefForest] = L("Neritha Reefwood"),
                [BiomeType.Beach]        = L("Gullshore", "Pale Strand", "Driftwen", "Seabright Cove", "Foamreach"),
                [BiomeType.Desert]       = L("Sunscar", "Dunmarch", "the Glasslands", "Parchwaste", "Khaldune", "the Amber Reach"),
                [BiomeType.Swamp]        = L("Mirewood", "Blackfen", "Sloughmere", "the Drownreach", "Gloombog"),
                [BiomeType.Marsh]        = L("Reedmoor", "Fenwick", "Saltmarsh Hollow", "the Quag", "Willowmire"),
                [BiomeType.ForestTemple] = L("Thornveil Sanctum", "the Verdant Cloister", "Greenmoot", "Eldwood Shrine", "the Mossgate"),
                [BiomeType.Plains]       = L("Westmeadow", "Tallgrass", "the Wend", "Larkfield", "Brightfield"),
                [BiomeType.Mountains]    = L("Stonecrest", "the Ironridge", "Frosthorn", "Greypeak", "the Hollow Range")
            };

            private static Dictionary<PoiType, List<string>> BuildPoiPools() => new Dictionary<PoiType, List<string>>
            {
                [PoiType.City]       = L("Eastgate", "Millcross", "Stonewick", "Harrow's End", "Fairstead"),
                [PoiType.Village]    = L("Oakhollow", "Redferry", "Thistledown", "Marrowby", "Coldbrook", "Hently", "Ashby", "Wend's Cross", "Nacre Village", "Pearlroot Haven"),
                [PoiType.Temple]     = L("the Quiet Temple", "Ember Sanctum", "the Tidal Temple", "Windward Temple", "the Old Sanctum"),
                [PoiType.Shrine]     = L("Wind Shrine", "the Sunken Shrine", "Ashen Shrine", "the Glade Shrine", "Stone Shrine", "Tideward Shrine", "the Tideglass Shrine"),
                [PoiType.Dungeon]    = L("the Sunken Vault", "Gravecrawl", "the Black Warren", "Ruin of Talreth", "the Deepliar", "Hollowdeep"),
                [PoiType.Market]     = L("the Grand Bazaar", "Trade Row", "the Night Market"),
                [PoiType.Landmark]   = L("the Standing Stones", "the Broken Arch", "the Watchspire", "the Weeping Statue", "the Old Beacon", "the Coral Canopy", "the Reefwood Crown"),
                [PoiType.Dock]       = L("the Wharf", "Tidedock", "the Salt Quay", "Anchorhold"),
                [PoiType.Camp]       = L("a bandit camp", "a hunters' camp", "a war camp", "an outlaw roost", "a raider hold"),
                [PoiType.Arena]      = L("the Proving Grounds", "the Grand Arena"),
                [PoiType.WeaponCache]= L("a hidden cache", "an old armoury", "a buried stash", "a smuggler's hold")
            };

            private static List<string> L(params string[] s) => new List<string>(s);
        }
    }
}
