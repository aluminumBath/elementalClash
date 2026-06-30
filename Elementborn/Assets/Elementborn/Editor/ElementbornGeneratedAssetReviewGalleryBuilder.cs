#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornGeneratedAssetReviewGalleryBuilder
    {
        public const string ReviewRootName = "Generated Asset Review Gallery";

        [MenuItem("Elementborn/Assets/Create Generated Asset Review Gallery")]
        public static void CreateReviewGalleryMenu()
        {
            CreateReviewGallery("", true);
        }

        [MenuItem("Elementborn/Assets/Clear Generated Asset Review Gallery")]
        public static void ClearReviewGalleryMenu()
        {
            ClearReviewGallery();
        }

        public static void CreateReviewGallery(string search, bool saveScene)
        {
            ClearReviewGallery();

            GameObject root = new GameObject(ReviewRootName);
            root.transform.position = new Vector3(0f, 0f, 24f);

            ElementbornGeneratedAssetLibraryBuilder.Entry[] entries = ElementbornGeneratedAssetLibraryBuilder.Entries;
            int placed = 0;

            for (int i = 0; i < entries.Length; i++)
            {
                ElementbornGeneratedAssetLibraryBuilder.Entry entry = entries[i];
                if (!PassesSearch(entry, search))
                {
                    continue;
                }

                GameObject prefab = ElementbornGeneratedAssetLibraryBuilder.LoadAutoPrefab(entry.SafeName);
                if (prefab == null)
                {
                    continue;
                }

                int col = placed % 6;
                int row = placed / 6;
                Vector3 position = root.transform.position + new Vector3((col - 2.5f) * 3.8f, 0f, row * 4.2f);

                CreateReviewStand(root.transform, entry, prefab, position);
                placed++;
            }

            if (saveScene)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("Generated asset review gallery created. Placed=" + placed);
        }

        public static void ClearReviewGallery()
        {
            GameObject root = GameObject.Find(ReviewRootName);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            MarkSceneDirtyAndSave();
        }

        private static bool PassesSearch(ElementbornGeneratedAssetLibraryBuilder.Entry entry, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            string haystack = (entry.SafeName + " " + entry.DisplayName + " " + entry.Role + " " + entry.Element + " " + entry.Notes).ToLowerInvariant();
            return haystack.Contains(search.ToLowerInvariant());
        }

        private static void CreateReviewStand(Transform root, ElementbornGeneratedAssetLibraryBuilder.Entry entry, GameObject prefab, Vector3 position)
        {
            GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stand.name = "Review " + entry.SafeName;
            stand.transform.SetParent(root, true);
            stand.transform.position = position;
            stand.transform.localScale = new Vector3(0.9f, 0.18f, 0.9f);
            SetMaterial(stand, "Review Stand Material", GetStatusColor(entry.SafeName));

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab);
            }

            instance.name = "Model " + entry.SafeName;
            instance.transform.SetParent(stand.transform, false);
            instance.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            NormalizeInstance(instance.transform, Mathf.Clamp(entry.TargetHeight, 0.45f, 1.35f), new Vector3(0f, 0.35f, 0f));

            ElementbornPrototypeGeneratedAssetReviewTag reviewTag = stand.AddComponent<ElementbornPrototypeGeneratedAssetReviewTag>();
            reviewTag.safeName = entry.SafeName;
            reviewTag.displayName = entry.DisplayName;
            reviewTag.role = entry.Role;
            reviewTag.element = entry.Element;
            reviewTag.approved = ElementbornGeneratedAssetApprovalDatabase.IsApproved(entry.SafeName);
            reviewTag.rejected = ElementbornGeneratedAssetApprovalDatabase.IsRejected(entry.SafeName);

            AddLabel(stand.transform, entry.DisplayName + "\n" + entry.Role + " / " + entry.Element, new Vector3(0f, Mathf.Clamp(entry.TargetHeight, 0.45f, 1.35f) + 0.85f, 0f));
        }

        private static Color GetStatusColor(string safeName)
        {
            if (ElementbornGeneratedAssetApprovalDatabase.IsApproved(safeName))
            {
                return new Color(0.12f, 0.55f, 0.22f);
            }

            if (ElementbornGeneratedAssetApprovalDatabase.IsRejected(safeName))
            {
                return new Color(0.55f, 0.12f, 0.12f);
            }

            return new Color(0.16f, 0.16f, 0.22f);
        }

        private static void NormalizeInstance(Transform modelRoot, float targetHeight, Vector3 desiredLocalOffset)
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            float height = Mathf.Max(0.001f, bounds.size.y);
            float scale = Mathf.Clamp(targetHeight / height, 0.00005f, 0.35f);
            modelRoot.localScale *= scale;

            renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            Vector3 worldDesired = modelRoot.parent != null ? modelRoot.parent.TransformPoint(desiredLocalOffset) : desiredLocalOffset;
            Vector3 correction = worldDesired - bounds.center;
            correction.y += bounds.extents.y;
            modelRoot.position += correction;
        }

        private static void AddLabel(Transform parent, string text, Vector3 localPosition)
        {
            GameObject labelGo = new GameObject("Review Label");
            labelGo.transform.SetParent(parent, false);
            labelGo.transform.localPosition = localPosition;
            labelGo.transform.localScale = Vector3.one * 0.13f;

            TextMesh label = labelGo.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 34;
            label.characterSize = 0.24f;
            label.color = Color.white;
        }

        private static void SetMaterial(GameObject go, string materialName, Color color)
        {
            Renderer renderer = go != null ? go.GetComponent<Renderer>() : null;
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
            {
                return;
            }

            Material material = new Material(shader);
            material.name = materialName;
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private static void MarkSceneDirtyAndSave()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
