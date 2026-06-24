using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Path-based model attach shared by NPCs, sidekick pets, weapon pickups, the player model, and props —
    /// the non-creature counterpart to <see cref="CreatureModelLibrary"/>. Loads a model from Resources,
    /// instantiates it as a child of the host (idempotent — one model per host), and hides any placeholder
    /// renderers when a model is actually found (so the primitive shows only when no model exists). Caches hits
    /// and misses, so a missing file is probed once.
    /// </summary>
    public static class ModelLibrary
    {
        private const string AttachedPrefix = "Model_";
        private static readonly Dictionary<string, GameObject> _cache = new Dictionary<string, GameObject>();

        public static GameObject Load(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return null;
            if (_cache.TryGetValue(resourcePath, out var cached)) return cached;
            var prefab = Resources.Load<GameObject>(resourcePath);
            _cache[resourcePath] = prefab; // may be null — cache the miss too
            return prefab;
        }

        public static bool Has(string resourcePath) => Load(resourcePath) != null;

        public static GameObject Attach(string resourcePath, GameObject host, string label = "Model", bool hidePlaceholder = true)
        {
            if (host == null) return null;
            foreach (Transform child in host.transform)
                if (child.name.StartsWith(AttachedPrefix)) return child.gameObject; // already wired

            var prefab = Load(resourcePath);
            if (prefab == null) return null; // keep the primitive placeholder

            Renderer[] placeholder = hidePlaceholder ? host.GetComponentsInChildren<Renderer>(true) : null;

            var model = Object.Instantiate(prefab, host.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.name = AttachedPrefix + label;

            if (placeholder != null)
                foreach (var r in placeholder)
                    if (r != null) r.enabled = false;

            return model;
        }

        public static void ClearCache() => _cache.Clear();
    }
}
