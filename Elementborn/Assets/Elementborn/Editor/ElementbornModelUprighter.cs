#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    /// <summary>
    /// Stands up models that imported lying on their face/back — the ones Bake Axis Conversion couldn't fix. Select
    /// the offending model assets (the flat .fbx) in the Project window, then run this. For each, it measures which
    /// 90° rotation makes the model tallest (i.e. upright), then bakes that into a corrected <b>prefab</b>: an
    /// identity-rotation root with the model as a child holding the rotation. The raw .fbx is moved into a sibling
    /// <c>raw/</c> folder so the new prefab owns the original name — so anything that loaded the model by name
    /// (ModelLibrary / Resources) now gets the upright prefab, and <c>ModelLibrary.Attach</c>'s identity-reset of the
    /// root no longer flattens it (the rotation lives on the child).
    ///
    /// It only touches the models you SELECT, so naturally long models (quadrupeds, boats) are never mis-rotated.
    /// Reversible: delete the prefab and move the .fbx back out of <c>raw/</c>.
    /// Menu: Elementborn/Model Fix/Upright Selected Models
    /// </summary>
    public static class ElementbornModelUprighter
    {
        private static readonly Quaternion[] Candidates =
        {
            Quaternion.identity,
            Quaternion.Euler(-90f, 0f, 0f),
            Quaternion.Euler(90f, 0f, 0f),
            Quaternion.Euler(0f, 0f, 90f),
            Quaternion.Euler(0f, 0f, -90f),
        };

        [MenuItem("Elementborn/Model Fix/Upright Selected Models")]
        public static void UprightSelected()
        {
            var models = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            if (models == null || models.Length == 0)
            {
                EditorUtility.DisplayDialog("Upright models",
                    "Select one or more model assets (the flat .fbx) in the Project window first.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Upright selected models",
                $"Stand up {models.Length} selected model(s) by baking a corrective rotation into a prefab?\n\n" +
                "The raw .fbx is moved into a sibling 'raw/' folder and a same-named prefab replaces it. Reversible.",
                "Upright them", "Cancel")) return;

            var sb = new StringBuilder("=== Elementborn Upright ===\n");
            int done = 0, skipped = 0;

            foreach (var model in models)
            {
                string path = AssetDatabase.GetAssetPath(model);
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext != ".fbx" && ext != ".obj" && ext != ".blend")
                {
                    sb.AppendLine("  [skip] not a model: " + path); skipped++; continue;
                }

                // Measure which candidate rotation makes it tallest (= upright).
                var probe = (GameObject)PrefabUtility.InstantiatePrefab(model);
                Quaternion best = Quaternion.identity;
                float bestY = float.NegativeInfinity;
                foreach (var rot in Candidates)
                {
                    probe.transform.rotation = rot;
                    Bounds b = WorldBounds(probe);
                    if (b.size.y > bestY) { bestY = b.size.y; best = rot; }
                }
                Object.DestroyImmediate(probe);

                if (best == Quaternion.identity)
                {
                    sb.AppendLine("  [ok] already upright: " + Path.GetFileName(path)); skipped++; continue;
                }

                string dir = Path.GetDirectoryName(path).Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(path);
                string rawDir = dir + "/raw";
                if (!AssetDatabase.IsValidFolder(rawDir)) AssetDatabase.CreateFolder(dir, "raw");

                string movedPath = rawDir + "/" + Path.GetFileName(path);
                string err = AssetDatabase.MoveAsset(path, movedPath);
                if (!string.IsNullOrEmpty(err)) { sb.AppendLine("  [fail] move: " + err); skipped++; continue; }

                var moved = AssetDatabase.LoadAssetAtPath<GameObject>(movedPath);
                var root = new GameObject(name);
                var child = (GameObject)PrefabUtility.InstantiatePrefab(moved);
                child.transform.SetParent(root.transform, false);
                child.transform.localRotation = best;

                string prefabPath = dir + "/" + name + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Object.DestroyImmediate(root);

                sb.AppendLine($"  [upright] {name}  ->  {prefabPath}  (rot {best.eulerAngles})");
                done++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            sb.AppendLine($"Uprighted {done}, skipped {skipped}. Re-run the Model Audit to confirm.");
            Debug.Log(sb.ToString());
        }

        private static Bounds WorldBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }
    }
}
#endif
