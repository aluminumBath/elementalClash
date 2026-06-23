using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// POI-driven structure placement: for each point of interest the world generator produced, picks
    /// a structure kind, generates a plan (seeded by the POI so it's stable and varied), and builds it
    /// on the terrain. Capped via <see cref="maxStructures"/>; mesh-combining/streaming for large
    /// worlds is a later optimisation.
    /// </summary>
    public sealed class StructurePlacer : MonoBehaviour
    {
        [SerializeField] private WorldMapController worldSource;
        [SerializeField] private StructureBuilder builder;
        [SerializeField] private float mapToWorldScale = 1f;
        [SerializeField] private float groundY = 0f;
        [Tooltip("If a Terrain is active, place structures on its surface instead of using groundY.")]
        [SerializeField] private bool snapToTerrain = true;
        [Tooltip("Merge each structure's parts per material (far fewer draw calls).")]
        [SerializeField] private bool combineStructures = true;
        [SerializeField] private int maxStructures = 150;
        [SerializeField] private bool buildOnStart = false;

        private void Start()
        {
            if (buildOnStart) Place();
        }

        public void Place() => Place(worldSource != null ? worldSource.World : null);

        public void Place(WorldMap world)
        {
            if (world == null) return;
            if (builder == null) builder = gameObject.AddComponent<StructureBuilder>();

            int count = 0;
            foreach (var poi in world.AllPois())
            {
                if (count++ >= maxStructures) break;
                var rng = new SystemRandomSource(StableSeed(poi.Id));
                var plan = StructureGenerator.Generate(poi, rng, new StructureGenConfig());
                var root = builder.Build(plan, transform, ToWorld(poi.MapPosition));
                if (combineStructures) MeshCombiner.CombineHierarchy(root, addMeshCollider: true, destroyOriginals: true);
            }
            Debug.Log($"[Elementborn] Placed {Mathf.Min(count, maxStructures)} structures from POIs.");
        }

        private Vector3 ToWorld(Vector2 mapPos)
        {
            var p = new Vector3(mapPos.x * mapToWorldScale, groundY, mapPos.y * mapToWorldScale);
            if (snapToTerrain) p.y = TerrainHeight.Sample(p);
            return p;
        }

        // Stable, process-independent hash so a POI always seeds the same building.
        private static int StableSeed(string s)
        {
            unchecked
            {
                int h = 17;
                if (s != null)
                    foreach (char ch in s) h = h * 31 + ch;
                return h;
            }
        }
    }
}
