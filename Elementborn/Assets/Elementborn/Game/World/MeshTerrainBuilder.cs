using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A drop-in alternative to <see cref="TerrainBuilder"/> that builds a faceted, vertex-coloured
    /// low-poly mesh terrain (the full Wind-Waker ground) instead of a Unity Terrain — so it renders with
    /// <c>Elementborn/ToonLit</c>, getting the cel bands and the inverted-hull outline a Unity Terrain
    /// can't. It generates a coarse heightmap via <see cref="TerrainGenerator"/>, splits it into chunks
    /// (each its own mesh + collider for culling and walking), and colours faces per biome. Use this OR
    /// the Unity TerrainBuilder, not both. Ground snapping for spawns/structures/creatures keeps working
    /// because everything samples height through <see cref="TerrainHeight"/>, which prefers this builder.
    ///
    /// Keep <see cref="terrainSize"/> equal to your world's MapSize so regions, the map, and spawns line up.
    /// </summary>
    public sealed class MeshTerrainBuilder : MonoBehaviour
    {
        public static MeshTerrainBuilder Instance { get; private set; }

        [SerializeField] private WorldMapController worldSource;

        [Header("Shape (coarse = low-poly)")]
        [Tooltip("Grid points per axis. Low-poly wants this coarse (e.g. 65–129), not the 513 a Unity Terrain uses.")]
        [SerializeField] private int meshResolution = 97;
        [SerializeField] private float terrainSize = 1200f; // world units across (match MapSize)
        [SerializeField] private float heightScale = 120f;  // world Y at normalised height 1
        [SerializeField] private float seaLevel = 0.12f;
        [SerializeField] private int generationSeed = 12345;
        [Tooltip("Cells per chunk edge. Smaller = more, lighter chunks (better culling).")]
        [SerializeField] private int chunkCells = 16;

        [Header("Toon look")]
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private float outlineWidth = 0.02f;
        [SerializeField, Range(1, 5)] private int rampSteps = 2;

        [Header("Water (optional)")]
        [Tooltip("Optional: a code-built toon water surface, auto-sized and placed at sea level.")]
        [SerializeField] private WaterSurface waterSurface;

        [SerializeField] private bool buildOnStart = true;

        public TerrainModel Model { get; private set; }

        private Material _material;
        private readonly List<GameObject> _chunks = new List<GameObject>();

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (buildOnStart) Build();
        }

        public void Build()
        {
            var world = worldSource != null ? worldSource.World : null;
            if (world == null)
            {
                Debug.LogWarning("MeshTerrainBuilder: no world to build from (assign a WorldMapController).");
                return;
            }
            Build(world);
        }

        public void Build(WorldMap world)
        {
            foreach (var c in _chunks) if (c != null) Destroy(c);
            _chunks.Clear();

            var cfg = new TerrainGenConfig
            {
                Resolution = Mathf.Max(2, meshResolution),
                WorldSize = terrainSize,
                SeaLevel = seaLevel,
            };
            Model = TerrainGenerator.Generate(world, new SystemRandomSource(generationSeed), cfg);

            EnsureMaterial();
            var datas = MeshTerrainGenerator.Build(Model, terrainSize, heightScale, chunkCells);

            for (int i = 0; i < datas.Count; i++)
            {
                var d = datas[i];
                var go = new GameObject($"TerrainChunk_{i}", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero; // vertices already span [0, terrainSize]

                var mesh = new Mesh { name = go.name };
                if (d.Vertices.Length > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = d.Vertices;
                mesh.normals = d.Normals;
                mesh.colors32 = d.Colors;
                mesh.triangles = d.Triangles;
                mesh.RecalculateBounds();

                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                go.GetComponent<MeshRenderer>().sharedMaterial = _material;
                go.GetComponent<MeshCollider>().sharedMesh = mesh;
                _chunks.Add(go);
            }

            Instance = this;
            if (waterSurface != null) waterSurface.Build(terrainSize, seaLevel * heightScale);

            Debug.Log($"[Elementborn] Mesh terrain built: {datas.Count} chunks, {Model.Resolution}^2 grid, " +
                      $"water {Model.WaterFraction():P0}.");
        }

        private void EnsureMaterial()
        {
            if (_material != null) return;

            var shader = Shader.Find("Elementborn/ToonLit")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard");
            _material = new Material(shader) { name = "MeshTerrain (ToonLit vertex colours)" };

            if (shader != null && shader.name == "Elementborn/ToonLit")
            {
                _material.EnableKeyword("_VERTEXCOLOR_ON");
                _material.SetFloat("_VertexColorOn", 1f);
                _material.SetColor("_BaseColor", Color.white);
                _material.SetFloat("_RampSteps", rampSteps);
                if (_material.HasProperty("_OutlineColor")) _material.SetColor("_OutlineColor", outlineColor);
                if (_material.HasProperty("_OutlineWidth")) _material.SetFloat("_OutlineWidth", outlineWidth);
            }
        }

        /// <summary>World-space ground height at a position, for snapping and locomotion.</summary>
        public bool TrySample(Vector3 world, out float y)
        {
            y = world.y;
            if (Model == null || terrainSize <= 0f) return false;

            Vector3 local = world - transform.position;
            float h = MeshTerrainGenerator.SampleHeight01(Model, local.x / terrainSize, local.z / terrainSize);
            y = transform.position.y + h * heightScale;
            return true;
        }
    }
}
