using System;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Spawns a simple, interactable portal marker for each hidden landmark at an authored world position, so the
    /// four locations physically exist and can be entered (through <see cref="LandmarkPortal"/> +
    /// <see cref="LandmarkAccessGate"/>) without hand-placing each in a scene. The markers are primitive placeholders
    /// with a trigger the <see cref="PlayerInteractor"/> detects; swap in real prefabs later, or fold this into
    /// <see cref="WorldSpawnPlacer"/> for biome-driven placement. Thalen'Veyr's marker sits high by default (the
    /// storm-eye descent); the rest sit at surface level.
    /// </summary>
    public sealed class LandmarkPortalPlacer : MonoBehaviour
    {
        [Serializable]
        public struct PortalPlacement
        {
            public Landmark Landmark;
            public Vector3 Position;
        }

        [SerializeField] private PortalPlacement[] placements = DefaultPlacements();
        [Tooltip("World Y treated as sea level for the altitude/depth gate on each portal.")]
        [SerializeField] private float seaLevelY = 0f;
        [Tooltip("Radius of the interaction trigger around each portal marker.")]
        [SerializeField] private float triggerRadius = 3f;
        [SerializeField] private bool spawnOnStart = true;

        private void Start()
        {
            if (spawnOnStart) Place();
        }

        /// <summary>Spawn a portal marker for every configured placement.</summary>
        public void Place()
        {
            if (placements == null) return;
            for (int i = 0; i < placements.Length; i++)
                SpawnPortal(placements[i].Landmark, placements[i].Position);
        }

        private void SpawnPortal(Landmark landmark, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "LandmarkPortal_" + landmark;
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(2f, 3f, 2f);

            // Replace the primitive's solid collider with a trigger the interactor can detect.
            var solid = go.GetComponent<Collider>();
            if (solid != null) Destroy(solid);
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = Mathf.Max(0.5f, triggerRadius);

            var portal = go.AddComponent<LandmarkPortal>();
            portal.Configure(landmark, seaLevelY);
        }

        private static PortalPlacement[] DefaultPlacements()
        {
            return new[]
            {
                new PortalPlacement { Landmark = Landmark.ThalenVeyr,        Position = new Vector3(   0f, 130f,  400f) },
                new PortalPlacement { Landmark = Landmark.AshwindAtoll,      Position = new Vector3( 350f,   0f, -150f) },
                new PortalPlacement { Landmark = Landmark.Ilyrath,           Position = new Vector3(-350f,   0f, -150f) },
                new PortalPlacement { Landmark = Landmark.TidecallerVillage, Position = new Vector3(   0f,   0f, -400f) },
            };
        }
    }
}
