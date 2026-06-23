using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Spawns a <see cref="LeylineRiftObject"/> with a simple floating crystal marker for each canonical
    /// rift in <see cref="WorldMap"/>, snapped to the terrain. The bootstrap scene adds one.</summary>
    public sealed class LeylineRiftSpawner : MonoBehaviour
    {
        [SerializeField] private float markerScale = 1.6f;
        private static readonly Color RiftCyan = new Color(0.45f, 0.85f, 1f, 1f);

        private void Start()
        {
            foreach (var rift in WorldMap.Rifts)
            {
                var go = new GameObject("Rift_" + rift.Id);
                go.transform.SetParent(transform, false);

                var obj = go.AddComponent<LeylineRiftObject>();
                obj.Configure(rift);                       // sets XZ from the canonical position
                var p = go.transform.position;
                p.y = TerrainHeight.Sample(p);             // snap to the ground
                go.transform.position = p;

                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Crystal";
                var col = marker.GetComponent<Collider>();
                if (col != null) Destroy(col);             // discovery is distance-based; don't block the player
                marker.transform.SetParent(go.transform, false);
                marker.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                marker.transform.localScale = Vector3.one * markerScale;
                var r = marker.GetComponent<Renderer>();
                if (r != null) r.material.color = RiftCyan;
            }
        }
    }
}
