#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public sealed class ElementbornGeneratedAssetAssignmentWindow : EditorWindow
    {
        private Vector2 scroll;
        private string search = "";
        private int selectedIndex;
        private Vector3 localOffset = Vector3.zero;
        private float targetHeight = 1.2f;
        private bool hideSelectedRenderer = true;
        private bool addAnimator = true;
        private bool createPreviewPedestal = true;

        [MenuItem("Elementborn/Assets/Generated Asset Assignment Window")]
        public static void Open()
        {
            ElementbornGeneratedAssetAssignmentWindow window = GetWindow<ElementbornGeneratedAssetAssignmentWindow>("Generated Assets");
            window.minSize = new Vector2(620f, 520f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Elementborn Generated Asset Assignment", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use this window to place or assign one generated FBX prefab at a time. It supports your renamed files once you run the fuzzy extractor and build the library.",
                MessageType.Info);

            DrawToolbar();
            EditorGUILayout.Space(8);

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

                DrawEntryRow(i, entry);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            DrawSelectionActions();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sanitize Imports", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetImportSanitizer.SanitizeGeneratedAssetImports();
            }

            if (GUILayout.Button("Repair Folder Names", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetLibraryBuilder.RepairGeneratedAssetFolderNames();
            }

            if (GUILayout.Button("Report Matches", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetLibraryBuilder.ReportGeneratedAssetLibraryMatches();
            }

            if (GUILayout.Button("Build Prefabs", GUILayout.Height(26)))
            {
                ElementbornGeneratedAssetLibraryBuilder.BuildGeneratedAssetLibraryFromExtractedFbxs();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear Decorations", GUILayout.Height(24)))
            {
                ElementbornGeneratedAssetSceneDecorator.ClearGeneratedAssetDecorationsInOpenScene();
            }

            if (GUILayout.Button("Open AutoImported Folder", GUILayout.Height(24)))
            {
                Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/Elementborn/Art/Models/MeshyImported/AutoImported");
                if (folder != null)
                {
                    EditorGUIUtility.PingObject(folder);
                    Selection.activeObject = folder;
                }
            }

            if (GUILayout.Button("Open Prefab Folder", GUILayout.Height(24)))
            {
                Object folder = AssetDatabase.LoadAssetAtPath<Object>("Assets/Elementborn/Generated/Prefabs/ImportedModels/AutoImported");
                if (folder != null)
                {
                    EditorGUIUtility.PingObject(folder);
                    Selection.activeObject = folder;
                }
            }

            if (GUILayout.Button("Visual Presets", GUILayout.Height(24)))
            {
                ElementbornGeneratedAssetPresetWindow.Open();
            }

            if (GUILayout.Button("Asset Review", GUILayout.Height(24)))
            {
                ElementbornGeneratedAssetReviewWindow.Open();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntryRow(int index, ElementbornGeneratedAssetLibraryBuilder.Entry entry)
        {
            string prefabPath = ElementbornGeneratedAssetLibraryBuilder.GetPrefabPathBySafeName(entry.SafeName);
            string fbxPath = ElementbornGeneratedAssetLibraryBuilder.FindFirstFbxForSafeName(entry.SafeName);
            bool hasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            bool hasFbx = !string.IsNullOrWhiteSpace(fbxPath);

            GUIStyle style = index == selectedIndex ? EditorStyles.helpBox : GUI.skin.box;

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Toggle(index == selectedIndex, "", GUILayout.Width(20)))
            {
                selectedIndex = index;
                targetHeight = entry.TargetHeight;
            }

            EditorGUILayout.LabelField(entry.DisplayName, EditorStyles.boldLabel, GUILayout.Width(190));
            EditorGUILayout.LabelField(entry.Role + " / " + entry.Element + " / " + entry.AnimationMode, GUILayout.Width(210));
            EditorGUILayout.LabelField(hasFbx ? "FBX ✓" : "FBX —", GUILayout.Width(55));
            EditorGUILayout.LabelField(hasPrefab ? "Prefab ✓" : "Prefab —", GUILayout.Width(75));

            EditorGUILayout.EndHorizontal();

            if (index == selectedIndex)
            {
                EditorGUILayout.LabelField("Safe Name", entry.SafeName);
                EditorGUILayout.LabelField("Old Pattern", entry.OldPattern);
                EditorGUILayout.LabelField("Renamed Pattern", entry.RenamedPattern);
                EditorGUILayout.LabelField("Notes", entry.Notes);
                EditorGUILayout.LabelField("FBX", string.IsNullOrWhiteSpace(fbxPath) ? "(not found)" : fbxPath);
                EditorGUILayout.LabelField("Prefab", prefabPath);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectionActions()
        {
            ElementbornGeneratedAssetLibraryBuilder.Entry[] entries = ElementbornGeneratedAssetLibraryBuilder.Entries;
            if (selectedIndex < 0 || selectedIndex >= entries.Length)
            {
                selectedIndex = 0;
            }

            ElementbornGeneratedAssetLibraryBuilder.Entry entry = entries[selectedIndex];

            EditorGUILayout.LabelField("Selected: " + entry.DisplayName, EditorStyles.boldLabel);

            localOffset = EditorGUILayout.Vector3Field("Local Offset", localOffset);
            targetHeight = EditorGUILayout.FloatField("Target Height", targetHeight);
            hideSelectedRenderer = EditorGUILayout.Toggle("Hide Selected Renderer", hideSelectedRenderer);
            addAnimator = EditorGUILayout.Toggle("Ensure Animator", addAnimator);
            createPreviewPedestal = EditorGUILayout.Toggle("Create Preview Pedestal", createPreviewPedestal);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Place Preview In Scene", GUILayout.Height(32)))
            {
                PlacePreview(entry);
            }

            if (GUILayout.Button("Assign To Selected Object", GUILayout.Height(32)))
            {
                AssignToSelected(entry);
            }

            if (GUILayout.Button("Ping Prefab", GUILayout.Height(32)))
            {
                PingPrefab(entry);
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
                Debug.LogWarning("No prefab found for " + entry.SafeName + ". Run Build Generated Asset Library From Extracted FBXs first.");
                return;
            }

            Vector3 position = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                position = SceneView.lastActiveSceneView.pivot;
            }

            GameObject parent = null;
            if (createPreviewPedestal)
            {
                parent = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                parent.name = "Preview " + entry.DisplayName;
                parent.transform.position = position;
                parent.transform.localScale = new Vector3(0.75f, 0.2f, 0.75f);
                Renderer renderer = parent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = CreateEditorMaterial("Preview Pedestal", new Color(0.12f, 0.12f, 0.18f));
                }
            }
            else
            {
                parent = new GameObject("Preview " + entry.DisplayName);
                parent.transform.position = position;
            }

            ElementbornGeneratedAssetSceneDecorator.AttachVisual(
                parent.transform,
                entry.SafeName,
                "Preview Generated Visual",
                localOffset + Vector3.up * 0.25f,
                Mathf.Max(0.1f, targetHeight),
                false);

            Selection.activeGameObject = parent;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void AssignToSelected(ElementbornGeneratedAssetLibraryBuilder.Entry entry)
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("Select a GameObject in the scene first.");
                return;
            }

            GameObject attached = ElementbornGeneratedAssetSceneDecorator.AttachVisual(
                selected.transform,
                entry.SafeName,
                "Assigned " + entry.SafeName + " Visual",
                localOffset,
                Mathf.Max(0.1f, targetHeight),
                hideSelectedRenderer);

            if (attached != null && addAnimator)
            {
                ElementbornPrototypeImportedModelAnimator animator = attached.GetComponent<ElementbornPrototypeImportedModelAnimator>();
                if (animator == null)
                {
                    animator = attached.AddComponent<ElementbornPrototypeImportedModelAnimator>();
                }

                animator.mode = entry.AnimationMode;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void PingPrefab(ElementbornGeneratedAssetLibraryBuilder.Entry entry)
        {
            string prefabPath = ElementbornGeneratedAssetLibraryBuilder.GetPrefabPathBySafeName(entry.SafeName);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("No prefab found at " + prefabPath);
                return;
            }

            EditorGUIUtility.PingObject(prefab);
            Selection.activeObject = prefab;
        }

        private Material CreateEditorMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }
    }
}
#endif
