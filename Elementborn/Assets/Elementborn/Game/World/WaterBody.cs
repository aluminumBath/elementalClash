using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>A box-footprint body of water with a flat surface. While a position falls inside the XZ
    /// footprint and below the surface, <see cref="SubmersionDepth"/> reports how deep it sits — which is
    /// exactly what <see cref="EnvironmentHazardController"/> feeds into the pressure ramp. The open ocean
    /// registers one of these at sea level (see <c>TerrainBuilder</c>); the flooded Sunken Gate interior
    /// registers another high overhead (see <c>SiteInteriorController</c>). The vertical math is the pure
    /// <see cref="Submersion"/> core; this adds only the footprint test and a global registry.</summary>
    public sealed class WaterBody : MonoBehaviour
    {
        private static readonly List<WaterBody> Bodies = new List<WaterBody>();

        [Tooltip("World Y of the flat water surface; depth is measured below this.")]
        [SerializeField] private float surfaceY;
        [Tooltip("World XZ centre of the footprint (Y ignored).")]
        [SerializeField] private Vector2 center;
        [Tooltip("Half-extents of the XZ footprint, in world units.")]
        [SerializeField] private Vector2 halfExtents = new Vector2(600f, 600f);

        public float SurfaceY => surfaceY;

        /// <summary>Place/resize this body in world space. Call after spawning it in code.</summary>
        public void Configure(Vector2 worldCenter, Vector2 worldHalfExtents, float worldSurfaceY)
        {
            center = worldCenter;
            halfExtents = worldHalfExtents;
            surfaceY = worldSurfaceY;
        }

        private void OnEnable() { if (!Bodies.Contains(this)) Bodies.Add(this); }
        private void OnDisable() { Bodies.Remove(this); }

        /// <summary>Depth of <paramref name="pos"/> below this body's surface, or 0 when the point is
        /// outside the footprint or above the surface.</summary>
        public float DepthAt(Vector3 pos)
        {
            if (Mathf.Abs(pos.x - center.x) > halfExtents.x) return 0f;
            if (Mathf.Abs(pos.z - center.y) > halfExtents.y) return 0f;
            return Submersion.Depth(surfaceY, pos.y);
        }

        /// <summary>Greatest submersion depth across every active body at <paramref name="pos"/>
        /// (0 when the point is in none). Deepest wins, so overlapping bodies behave sensibly.</summary>
        public static float SubmersionDepth(Vector3 pos)
        {
            float best = 0f;
            for (int i = 0; i < Bodies.Count; i++)
            {
                float d = Bodies[i].DepthAt(pos);
                if (d > best) best = d;
            }
            return best;
        }
    }
}
