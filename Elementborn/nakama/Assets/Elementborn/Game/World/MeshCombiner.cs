using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Merges the descendant mesh renderers under a root into one combined mesh per material — a
    /// structure (or a whole batch) collapses from dozens of renderers to a handful, which is the
    /// difference between a smooth and a stuttering town. Since <see cref="ToonPalette"/> shares
    /// materials, a building usually ends up as just a few draw calls. Editor-safe so it can be
    /// unit-tested and used from tooling.
    /// </summary>
    public static class MeshCombiner
    {
        /// <summary>Combine everything under <paramref name="root"/>; returns the number of combined
        /// meshes created (one per distinct material).</summary>
        public static int CombineHierarchy(GameObject root, bool addMeshCollider = true, bool destroyOriginals = true)
        {
            if (root == null) return 0;

            var groups = new Dictionary<Material, List<MeshFilter>>();
            var originals = new List<GameObject>();

            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.transform == root.transform) continue;
                if (mf.gameObject.name.StartsWith("Combined_")) continue;
                if (mf.sharedMesh == null) continue;
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr == null || mr.sharedMaterial == null) continue;

                if (!groups.TryGetValue(mr.sharedMaterial, out var list))
                {
                    list = new List<MeshFilter>();
                    groups[mr.sharedMaterial] = list;
                }
                list.Add(mf);
                if (!originals.Contains(mf.gameObject)) originals.Add(mf.gameObject);
            }
            if (groups.Count == 0) return 0;

            var worldToLocal = root.transform.worldToLocalMatrix;
            int created = 0;
            foreach (var kv in groups)
            {
                var combines = new CombineInstance[kv.Value.Count];
                for (int i = 0; i < kv.Value.Count; i++)
                    combines[i] = new CombineInstance
                    {
                        mesh = kv.Value[i].sharedMesh,
                        transform = worldToLocal * kv.Value[i].transform.localToWorldMatrix
                    };

                var combined = new Mesh
                {
                    name = "CombinedMesh",
                    indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
                };
                combined.CombineMeshes(combines, true, true, false);
                combined.RecalculateBounds();

                var go = new GameObject("Combined_" + kv.Key.name, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(root.transform, false);
                go.GetComponent<MeshFilter>().sharedMesh = combined;
                go.GetComponent<MeshRenderer>().sharedMaterial = kv.Key;
                if (addMeshCollider) go.AddComponent<MeshCollider>().sharedMesh = combined;
                created++;
            }

            if (destroyOriginals)
                foreach (var go in originals) DestroySafe(go);

            return created;
        }

        public static int Combine(GameObject root) => CombineHierarchy(root, true, true);

        private static void DestroySafe(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) Object.Destroy(o);
            else Object.DestroyImmediate(o);
        }
    }
}
