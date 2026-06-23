using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Spawns a <see cref="CheckpointObject"/> with a simple glowing obelisk marker for each canonical
    /// checkpoint in <see cref="WorldMap"/>, snapped to the terrain. The bootstrap scene adds one.</summary>
    public sealed class CheckpointSpawner : MonoBehaviour
    {
        [SerializeField] private float markerScale = 1.2f;
        private static readonly Color ShrineAmber = new Color(1f, 0.62f, 0.22f, 1f);

        private void Start()
        {
            foreach (var cp in WorldMap.Checkpoints)
            {
                var go = new GameObject("Checkpoint_" + cp.Id);
                go.transform.SetParent(transform, false);

                var obj = go.AddComponent<CheckpointObject>();
                obj.Configure(cp);                          // sets XZ from the canonical position
                var p = go.transform.position;
                p.y = TerrainHeight.Sample(p);              // snap to the ground
                go.transform.position = p;

                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "Obelisk";
                var col = marker.GetComponent<Collider>();
                if (col != null) Destroy(col);              // interaction is distance-based; don't block the player
                marker.transform.SetParent(go.transform, false);
                marker.transform.localScale = new Vector3(0.5f, 1.4f, 0.5f) * markerScale;
                marker.transform.localPosition = new Vector3(0f, marker.transform.localScale.y, 0f);
                var r = marker.GetComponent<Renderer>();
                if (r != null) r.material.color = ShrineAmber;
            }
        }
    }
}
