using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Builds a Unity <see cref="Terrain"/> from a <see cref="WorldMap"/> via <see cref="TerrainGenerator"/>:
    /// sets the heightmap and, if a <see cref="TerrainLayer"/> is assigned per biome, paints a
    /// splatmap from the generated biome grid. Shape works with no layers assigned; the layer
    /// textures and any hand-sculpt refinement are the editor/art layer on top.
    ///
    /// Keep <see cref="terrainSize"/> equal to your world's MapSize so regions, the map, and spawns
    /// line up in world space (the terrain sits at this object's origin, spanning [0, terrainSize]).
    /// </summary>
    public sealed class TerrainBuilder : MonoBehaviour
    {
        [System.Serializable]
        public struct BiomeLayer
        {
            public BiomeType biome;
            public TerrainLayer layer;
        }

        [SerializeField] private WorldMapController worldSource;

        [Header("Shape")]
        [SerializeField] private int resolution = 513;     // power of two + 1
        [SerializeField] private float terrainSize = 1200f; // world units across (match MapSize)
        [SerializeField] private float heightScale = 120f;  // world Y at normalised height 1
        [SerializeField] private float seaLevel = 0.12f;
        [SerializeField] private int generationSeed = 12345;

        [Header("Texturing (assign a TerrainLayer per biome; optional)")]
        [SerializeField] private List<BiomeLayer> biomeLayers = new List<BiomeLayer>();

        [Header("Water (optional)")]
        [SerializeField] private GameObject waterPlanePrefab;
        [Tooltip("Optional: a code-built toon water surface, auto-sized and placed at sea level.")]
        [SerializeField] private WaterSurface waterSurface;

        [SerializeField] private bool buildOnStart = true;

        public Terrain Built { get; private set; }
        public TerrainModel Model { get; private set; }

        private WaterBody _ocean;

        private void Start()
        {
            if (buildOnStart) Build();
        }

        public Terrain Build()
        {
            var world = worldSource != null ? worldSource.World : null;
            if (world == null)
            {
                Debug.LogWarning("TerrainBuilder: no world to build from (assign a WorldMapController).");
                return null;
            }
            return Build(world);
        }

        public Terrain Build(WorldMap world)
        {
            var cfg = new TerrainGenConfig { Resolution = resolution, WorldSize = terrainSize, SeaLevel = seaLevel };
            Model = TerrainGenerator.Generate(world, new SystemRandomSource(generationSeed), cfg);

            var data = new TerrainData { heightmapResolution = Model.Resolution };
            data.size = new Vector3(terrainSize, heightScale, terrainSize);
            data.SetHeights(0, 0, Model.Heights);
            ApplyLayers(data);

            if (Built != null) Destroy(Built.gameObject);
            var go = Terrain.CreateTerrainGameObject(data);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero; // spans [0, terrainSize] in X/Z
            Built = go.GetComponent<Terrain>();

            if (waterPlanePrefab != null)
            {
                var water = Instantiate(waterPlanePrefab, transform);
                water.transform.localPosition =
                    new Vector3(terrainSize * 0.5f, seaLevel * heightScale, terrainSize * 0.5f);
            }

            if (waterSurface != null) waterSurface.Build(terrainSize, seaLevel * heightScale);

            // Sea level is authoritative from here: the pressure body, water mounts, and water creatures all
            // read WorldWater, so the ocean surface, its visual, and everything riding it line up.
            WorldWater.SeaLevelY = seaLevel * heightScale;

            // The ocean is a pressure-bearing body whether or not a visual surface is drawn: dive into a
            // deep basin and the crushing depths apply, unless you're attuned to Water/Earth.
            EnsureOceanBody(WorldWater.SeaLevelY);

            Debug.Log($"[Elementborn] Terrain built: {Model.Resolution}^2 cells, water {Model.WaterFraction():P0}.");
            return Built;
        }

        // Creates (once) and positions a map-sized water volume at sea level so the pressure hazard has
        // an ocean to measure depth against. Footprint matches the terrain's [0, terrainSize] span.
        private void EnsureOceanBody(float surfaceY)
        {
            if (_ocean == null)
            {
                var go = new GameObject("OceanWaterBody");
                go.transform.SetParent(transform, false);
                _ocean = go.AddComponent<WaterBody>();
            }
            float half = terrainSize * 0.5f;
            _ocean.Configure(new Vector2(half, half), new Vector2(half, half), surfaceY);
        }

        private void ApplyLayers(TerrainData data)
        {
            if (biomeLayers == null || biomeLayers.Count == 0) return; // shape only, no textures

            var layers = new TerrainLayer[biomeLayers.Count];
            var index = new Dictionary<BiomeType, int>();
            for (int i = 0; i < biomeLayers.Count; i++)
            {
                layers[i] = biomeLayers[i].layer;
                if (!index.ContainsKey(biomeLayers[i].biome)) index[biomeLayers[i].biome] = i;
            }
            data.terrainLayers = layers;

            int ares = Mathf.Clamp(Model.Resolution - 1, 16, 1024);
            data.alphamapResolution = ares;
            var maps = new float[ares, ares, layers.Length];
            for (int z = 0; z < ares; z++)
                for (int x = 0; x < ares; x++)
                {
                    int hx = Mathf.RoundToInt(x / (float)(ares - 1) * (Model.Resolution - 1));
                    int hz = Mathf.RoundToInt(z / (float)(ares - 1) * (Model.Resolution - 1));
                    int li = index.TryGetValue(Model.BiomeAt(hx, hz), out var idx) ? idx : 0;
                    maps[z, x, li] = 1f;
                }
            data.SetAlphamaps(0, 0, maps);
        }
    }
}
