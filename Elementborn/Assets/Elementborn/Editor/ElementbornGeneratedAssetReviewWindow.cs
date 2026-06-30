#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public sealed class ElementbornGeneratedAssetReviewWindow : EditorWindow
    {
        private Vector2 scroll;
        private string search = "";
        private string selectedSafeName = "";
        private string note = "";
        private string report = "";

        [MenuItem("Elementborn/Assets/Generated Asset Review Window")]
        public static void Open()
        {
            ElementbornGeneratedAssetReviewWindow window = GetWindow<ElementbornGeneratedAssetReviewWindow>("Asset Review");
            window.minSize = new Vector2(720f, 560f);
            window.RefreshReport();
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Elementborn Generated Asset Review", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Approve only models that look good: correct scale/orientation, usable materials, no corrupt textures, no huge white blobs. Presets can then be restricted to approved assets.",
                MessageType.Info);

            DrawToolbar();

            EditorGUILayout.Space(6);
            search = EditorGUILayout.TextField("Search", search);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            ElementbornGeneratedAssetLibraryBuilder.Entry[] entries = ElementbornGeneratedAssetLibraryBuilder.Entries;
            for (int i = 0; i < entries.Length; i++)
            {
                ElementbornGeneratedAssetLibraryBuilder.Entry entry = entries[i];
                if (!PassesSearch(entry))
                {
                    continue;
                }

                DrawEntry(entry);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6);
            DrawSelectedActions();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sanitize", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetImportSanitizer.SanitizeGeneratedAssetImports();
            }

            if (GUILayout.Button("Build Prefabs", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetLibraryBuilder.BuildGeneratedAssetLibraryFromExtractedFbxs();
            }

            if (GUILayout.Button("Create Review Gallery", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetReviewGalleryBuilder.CreateReviewGallery(search, true);
            }

            if (GUILayout.Button("Clear Gallery", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetReviewGalleryBuilder.ClearReviewGallery();
            }

            if (GUILayout.Button("Refresh", GUILayout.Height(26)))
            {
                RefreshReport();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.TextArea(report, GUILayout.Height(86));
        }

        private void DrawEntry(ElementbornGeneratedAssetLibraryBuilder.Entry entry)
        {
            bool approved = ElementbornGeneratedAssetApprovalDatabase.IsApproved(entry.SafeName);
            bool rejected = ElementbornGeneratedAssetApprovalDatabase.IsRejected(entry.SafeName);
            string prefabPath = ElementbornGeneratedAssetLibraryBuilder.GetPrefabPathBySafeName(entry.SafeName);
            bool hasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;

            GUIStyle style = entry.SafeName == selectedSafeName ? EditorStyles.helpBox : GUI.skin.box;

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(entry.SafeName == selectedSafeName, "", GUILayout.Width(20)))
            {
                if (selectedSafeName != entry.SafeName)
                {
                    selectedSafeName = entry.SafeName;
                    note = ElementbornGeneratedAssetApprovalDatabase.GetNote(entry.SafeName);
                }
            }

            EditorGUILayout.LabelField(entry.DisplayName, EditorStyles.boldLabel, GUILayout.Width(190));
            EditorGUILayout.LabelField(entry.Role + " / " + entry.Element, GUILayout.Width(140));
            EditorGUILayout.LabelField(hasPrefab ? "Prefab ✓" : "Prefab —", GUILayout.Width(70));
            EditorGUILayout.LabelField(approved ? "APPROVED" : rejected ? "REJECTED" : "UNREVIEWED", GUILayout.Width(105));

            if (GUILayout.Button("Ping", GUILayout.Width(52)))
            {
                PingPrefab(entry.SafeName);
            }

            EditorGUILayout.EndHorizontal();

            if (entry.SafeName == selectedSafeName)
            {
                EditorGUILayout.LabelField("Safe Name", entry.SafeName);
                EditorGUILayout.LabelField("Notes", entry.Notes);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedActions()
        {
            if (string.IsNullOrWhiteSpace(selectedSafeName))
            {
                EditorGUILayout.HelpBox("Select an asset to approve/reject or preview.", MessageType.None);
                return;
            }

            ElementbornGeneratedAssetLibraryBuilder.Entry entry = ElementbornGeneratedAssetLibraryBuilder.GetEntry(selectedSafeName);
            EditorGUILayout.LabelField("Selected: " + entry.DisplayName, EditorStyles.boldLabel);

            note = EditorGUILayout.TextField("Review Note", note);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Approve", GUILayout.Height(32)))
            {
                ElementbornGeneratedAssetApprovalDatabase.Approve(selectedSafeName, note);
                RefreshReport();
            }

            if (GUILayout.Button("Reject", GUILayout.Height(32)))
            {
                ElementbornGeneratedAssetApprovalDatabase.Reject(selectedSafeName, note);
                RefreshReport();
            }

            if (GUILayout.Button("Clear Review", GUILayout.Height(32)))
            {
                ElementbornGeneratedAssetApprovalDatabase.ClearReview(selectedSafeName);
                note = "";
                RefreshReport();
            }

            if (GUILayout.Button("Place Preview", GUILayout.Height(32)))
            {
                PlacePreview(entry);
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool PassesSearch(ElementbornGeneratedAssetLibraryBuilder.Entry entry)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            string haystack = (entry.SafeName + " " + entry.DisplayName + " " + entry.Role + " " + entry.Element + " " + entry.Notes).ToLowerInvariant();
            return haystack.Contains(search.ToLowerInvariant());
        }

        private void PlacePreview(ElementbornGeneratedAssetLibraryBuilder.Entry entry)
        {
            GameObject prefab = ElementbornGeneratedAssetLibraryBuilder.LoadAutoPrefab(entry.SafeName);
            if (prefab == null)
            {
                Debug.LogWarning("No prefab found for " + entry.SafeName + ". Build prefabs first.");
                return;
            }

            Vector3 position = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                position = SceneView.lastActiveSceneView.pivot;
            }

            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Review Preview " + entry.SafeName;
            pedestal.transform.position = position;
            pedestal.transform.localScale = new Vector3(0.75f, 0.2f, 0.75f);

            ElementbornGeneratedAssetSceneDecorator.AttachVisual(
                pedestal.transform,
                entry.SafeName,
                "Review Preview Visual",
                Vector3.up * 0.25f,
                Mathf.Clamp(entry.TargetHeight, 0.45f, 1.35f),
                false);

            Selection.activeGameObject = pedestal;
        }

        private void PingPrefab(string safeName)
        {
            string prefabPath = ElementbornGeneratedAssetLibraryBuilder.GetPrefabPathBySafeName(safeName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                EditorGUIUtility.PingObject(prefab);
                Selection.activeObject = prefab;
            }
        }

        private void RefreshReport()
        {
            report = ElementbornGeneratedAssetApprovalDatabase.BuildApprovalReport();
            Repaint();
        }
    }
}
#endif
