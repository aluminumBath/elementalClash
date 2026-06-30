#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public sealed class ElementbornGeneratedAssetPresetWindow : EditorWindow
    {
        private ElementbornGeneratedVisualPreset preset = ElementbornGeneratedVisualPreset.PropsOnly;
        private Vector2 scroll;
        private string report = "";

        [MenuItem("Elementborn/Assets/Generated Visual Presets Window")]
        public static void Open()
        {
            ElementbornGeneratedAssetPresetWindow window = GetWindow<ElementbornGeneratedAssetPresetWindow>("Visual Presets");
            window.minSize = new Vector2(600f, 460f);
            window.RefreshReport();
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Elementborn Generated Visual Presets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use this before placing many models by hand. It applies small curated sets only, with a report first. This is safer than the old auto-decorate flow.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            preset = (ElementbornGeneratedVisualPreset)EditorGUILayout.EnumPopup("Preset", preset);
            ElementbornGeneratedAssetSceneDecorator.requireApprovedGeneratedAssets = EditorGUILayout.Toggle("Require Approved Assets", ElementbornGeneratedAssetSceneDecorator.requireApprovedGeneratedAssets);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshReport();
            }

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sanitize Imports", GUILayout.Height(28)))
            {
                ElementbornGeneratedAssetImportSanitizer.SanitizeGeneratedAssetImports();
            }

            if (GUILayout.Button("Build Prefabs", GUILayout.Height(28)))
            {
                ElementbornGeneratedAssetLibraryBuilder.BuildGeneratedAssetLibraryFromExtractedFbxs();
                RefreshReport();
            }

            if (GUILayout.Button("Asset Review", GUILayout.Height(28)))
            {
                ElementbornGeneratedAssetReviewWindow.Open();
            }

            if (GUILayout.Button("Refresh Report", GUILayout.Height(28)))
            {
                RefreshReport();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear Decorations", GUILayout.Height(30)))
            {
                ElementbornGeneratedAssetSceneDecorator.ClearGeneratedAssetDecorationsInOpenScene();
                RefreshReport();
            }

            if (GUILayout.Button("Apply Selected Preset", GUILayout.Height(30)))
            {
                ElementbornGeneratedAssetSceneDecorator.ApplyPreset(preset, true);
                RefreshReport();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Preset Report", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void RefreshReport()
        {
            report = ElementbornGeneratedAssetSceneDecorator.BuildPresetReport(preset);
            Repaint();
        }
    }
}
#endif
