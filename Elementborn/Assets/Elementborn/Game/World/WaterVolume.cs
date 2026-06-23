using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// A body of water. Put it on an object with a (trigger) Collider — the collider's box is the water and its
    /// top is the surface. Anything inside the box is "submerged". Registered statically so any actor's
    /// <see cref="UnderwaterController"/> can ask "am I under water?" without references.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class WaterVolume : MonoBehaviour
    {
        private static readonly List<WaterVolume> All = new List<WaterVolume>();
        private Collider _col;

        private void Awake() => _col = GetComponent<Collider>();
        private void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        private void OnDisable() => All.Remove(this);

        public float Surface => _col != null ? _col.bounds.max.y : transform.position.y;
        public bool Contains(Vector3 p) => _col != null && _col.bounds.Contains(p);

        /// <summary>The water volume a point is submerged in, or null.</summary>
        public static WaterVolume Submerged(Vector3 p)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i] != null && All[i].Contains(p)) return All[i];
            return null;
        }
    }
}
