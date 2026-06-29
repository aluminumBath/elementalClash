using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>Region archetypes that make up the world (all original, no IP names).</summary>
    public enum BiomeType
    {
        Plains,
        Mountains,
        CapitalCity,
        Swamp,
        Marsh,
        Beach,
        Desert,
        ForestTemple,
        Volcano,
        Island,
        CoralReefForest,
        CloudTemple
    }

    /// <summary>Kinds of points of interest placed within a region.</summary>
    public enum PoiType
    {
        City,
        Village,
        Temple,
        Shrine,
        Dungeon,
        Market,
        Landmark,
        Dock,
        Camp,
        Arena,
        WeaponCache
    }

    /// <summary>A point of interest: where things happen and spawn within a region.</summary>
    public sealed class PointOfInterest
    {
        public string Id { get; }
        public string Name { get; }
        public PoiType Type { get; }
        public Vector2 MapPosition { get; }   // absolute position on the world map
        public int EnemySpawnCount { get; }

        public bool HasWeaponCache { get; }
        public WeaponType WeaponType { get; }
        public WeaponMaterial WeaponMaterial { get; }

        public PointOfInterest(string id, string name, PoiType type, Vector2 mapPosition,
            int enemySpawnCount, bool hasWeaponCache,
            WeaponType weaponType = WeaponType.None, WeaponMaterial weaponMaterial = WeaponMaterial.Wood)
        {
            Id = id; Name = name; Type = type; MapPosition = mapPosition;
            EnemySpawnCount = enemySpawnCount;
            HasWeaponCache = hasWeaponCache; WeaponType = weaponType; WeaponMaterial = weaponMaterial;
        }
    }

    /// <summary>A named region with a biome, a position/extent, neighbour links, and POIs.</summary>
    public sealed class WorldRegion
    {
        public string Id { get; }
        public string Name { get; }
        public BiomeType Biome { get; }
        public Vector2 MapPosition { get; }
        public float Radius { get; }
        public int DangerLevel { get; }          // 1 (near capital) .. higher (remote)
        public List<string> Neighbors { get; }   // connected region ids
        public List<PointOfInterest> Pois { get; }

        public WorldRegion(string id, string name, BiomeType biome, Vector2 mapPosition, float radius, int dangerLevel)
        {
            Id = id; Name = name; Biome = biome; MapPosition = mapPosition;
            Radius = radius; DangerLevel = dangerLevel;
            Neighbors = new List<string>();
            Pois = new List<PointOfInterest>();
        }
    }

    /// <summary>The whole world: regions with id lookup, neighbour traversal, and queries.</summary>
    public sealed class WorldMap
    {
        private readonly Dictionary<string, WorldRegion> _byId;
        public IReadOnlyList<WorldRegion> Regions { get; }

        public WorldMap(IEnumerable<WorldRegion> regions)
        {
            Regions = regions.ToList();
            _byId = Regions.ToDictionary(r => r.Id);
        }

        public WorldRegion Get(string id) => _byId.TryGetValue(id, out var r) ? r : null;

        public IEnumerable<WorldRegion> NeighborsOf(WorldRegion region)
        {
            foreach (var id in region.Neighbors)
            {
                var n = Get(id);
                if (n != null) yield return n;
            }
        }

        public WorldRegion Capital() => Regions.FirstOrDefault(r => r.Biome == BiomeType.CapitalCity);
        public IEnumerable<PointOfInterest> AllPois() => Regions.SelectMany(r => r.Pois);
        public IEnumerable<PointOfInterest> WeaponCaches() => AllPois().Where(p => p.HasWeaponCache);

        public Rect Bounds()
        {
            if (Regions.Count == 0) return new Rect(0, 0, 0, 0);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var r in Regions)
            {
                minX = Mathf.Min(minX, r.MapPosition.x); minY = Mathf.Min(minY, r.MapPosition.y);
                maxX = Mathf.Max(maxX, r.MapPosition.x); maxY = Mathf.Max(maxY, r.MapPosition.y);
            }
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
