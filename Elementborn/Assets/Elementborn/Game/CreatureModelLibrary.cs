using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Loads per-creature display models from <c>Resources/Models/Creatures/</c> (paths from
    /// <see cref="CreatureModelNames"/>) and attaches them to a spawned creature, hiding the primitive placeholder
    /// only when a real model is actually found. With no model present the caller keeps its placeholder, so the
    /// game runs unchanged until art is dropped in. Lookups (hits and misses) are cached to avoid repeat disk hits.
    /// </summary>
    public static class CreatureModelLibrary
    {
        private static readonly Dictionary<CreatureKind, GameObject> _cache = new Dictionary<CreatureKind, GameObject>();
        private static readonly HashSet<CreatureKind> _missing = new HashSet<CreatureKind>();

        private const string AttachedPrefix = "Model_";

        /// <summary>The model prefab for a kind, or null if none exists in Resources.</summary>
        public static GameObject Load(CreatureKind kind)
        {
            if (_cache.TryGetValue(kind, out var prefab)) return prefab;
            if (_missing.Contains(kind)) return null;

            prefab = Resources.Load<GameObject>(CreatureModelNames.ResourcePath(kind));
            if (prefab != null) _cache[kind] = prefab;
            else _missing.Add(kind);
            return prefab;
        }

        /// <summary>True if a model exists for the kind.</summary>
        public static bool Has(CreatureKind kind) => Load(kind) != null;

        /// <summary>
        /// Instantiate the kind's model as a child of <paramref name="host"/> and (by default) hide the host's
        /// existing placeholder renderers. Returns the model instance, or null when no model exists (host left
        /// untouched, so its placeholder still shows). Idempotent: at most one model is attached per host.
        /// </summary>
        public static GameObject Attach(CreatureKind kind, GameObject host, bool hidePlaceholder = true)
        {
            if (host == null) return null;

            // Already wired (possibly by another spawn path) — don't stack a second mesh.
            foreach (Transform child in host.transform)
                if (child.name.StartsWith(AttachedPrefix)) return child.gameObject;

            var prefab = Load(kind);
            if (prefab == null) return null;

            // Snapshot placeholder renderers BEFORE adding the model so we never disable the model itself.
            Renderer[] placeholder = hidePlaceholder ? host.GetComponentsInChildren<Renderer>(true) : null;

            var model = Object.Instantiate(prefab, host.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.name = AttachedPrefix + kind;

            if (placeholder != null)
                foreach (var r in placeholder)
                    if (r != null) r.enabled = false;

            return model;
        }

        /// <summary>Drop cached lookups (tests / asset reloads).</summary>
        public static void ClearCache()
        {
            _cache.Clear();
            _missing.Clear();
        }
    }
}
