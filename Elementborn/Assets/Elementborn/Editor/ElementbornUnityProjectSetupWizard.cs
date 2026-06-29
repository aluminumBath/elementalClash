#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornUnityProjectSetupWizard
    {
        private static readonly string[] RequiredTags =
        {
            "Player", "Enemy", "Interactable", "Boat", "Boss", "Projectile", "ResourceNode", "QuestObjective"
        };

        private static readonly string[] DesiredLayers =
        {
            "Player", "Enemy", "Interactable", "Water", "Projectile", "Ground", "BossArena", "ResourceNode"
        };

        [MenuItem("Elementborn/Unity Setup/Run Recommended Project Setup")]
        public static void RunRecommendedProjectSetup()
        {
            EnsureGeneratedFolders();
            EnsureTagsAndLayers();
            ConfigureUiTexturesAsSprites();
            EnsureEventSystemInOpenScene();
            AddPlayableSceneToBuildSettingsIfPresent();
            WriteUnitySetupReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Elementborn Unity setup completed. See Assets/Elementborn/Generated/Reports/UnitySetupReport.md");
        }

        [MenuItem("Elementborn/Unity Setup/Ensure Tags and Layers")]
        public static void EnsureTagsAndLayers()
        {
            Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                Debug.LogWarning("Could not load ProjectSettings/TagManager.asset. Add tags/layers manually from the report.");
                return;
            }

            var tagManager = new SerializedObject(tagManagerAssets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            if (tags != null)
            {
                foreach (string tag in RequiredTags)
                {
                    AddTag(tags, tag);
                }
            }

            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers != null)
            {
                int layerSlot = 8;
                foreach (string layer in DesiredLayers)
                {
                    layerSlot = AddLayer(layers, layer, layerSlot);
                }
            }

            tagManager.ApplyModifiedProperties();
        }

        [MenuItem("Elementborn/Unity Setup/Configure UI Textures as Sprites")]
        public static void ConfigureUiTexturesAsSprites()
        {
            string[] roots =
            {
                "Assets/Elementborn/Art/UI/MapIcons",
                "Assets/Elementborn/Art/UI/EquipmentIcons",
                "Assets/Elementborn/Art/UI/CombatIcons",
                "Assets/Elementborn/Art/UI/SpellIcons",
                "Assets/Elementborn/Art/UI/BossIcons",
                "Assets/Elementborn/Art/UI/QuestIcons",
                "Assets/Elementborn/Art/UI/SetupIcons"
            };

            int changed = 0;
            foreach (string root in roots)
            {
                if (!AssetDatabase.IsValidFolder(root))
                {
                    continue;
                }

                foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { root }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                    {
                        continue;
                    }

                    bool dirty = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        dirty = true;
                    }

                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        dirty = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        dirty = true;
                    }

                    if (importer.spritePixelsPerUnit != 128f)
                    {
                        importer.spritePixelsPerUnit = 128f;
                        dirty = true;
                    }

                    if (dirty)
                    {
                        importer.SaveAndReimport();
                        changed++;
                    }
                }
            }

            Debug.Log($"Configured {changed} Elementborn UI texture imports as Sprite assets.");
        }

        [MenuItem("Elementborn/Unity Setup/Ensure EventSystem In Open Scene")]
        public static void EnsureEventSystemInOpenScene()
        {
            if (ElementbornFindUtility.FindFirst<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Elementborn/Unity Setup/Add Playable Test Scene To Build Settings")]
        public static void AddPlayableSceneToBuildSettingsIfPresent()
        {
            string scenePath = "Assets/Elementborn/Generated/Scenes/Elementborn_Playable_Test.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning("Playable test scene does not exist yet. Run Elementborn/Playable Setup/Build Full Test Scene first.");
                return;
            }

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log("Added Elementborn_Playable_Test.unity to Build Settings.");
            }
        }

        [MenuItem("Elementborn/Unity Setup/Write Unity Setup Report")]
        public static void WriteUnitySetupReport()
        {
            EnsureGeneratedFolders();
            string reportPath = "Assets/Elementborn/Generated/Reports/UnitySetupReport.md";
            File.WriteAllText(reportPath, BuildReportText());
            AssetDatabase.ImportAsset(reportPath);
        }

        private static void EnsureGeneratedFolders()
        {
            EnsureFolder("Assets/Elementborn/Generated");
            EnsureFolder("Assets/Elementborn/Generated/Reports");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs");
            EnsureFolder("Assets/Elementborn/Generated/Scenes");
        }

        private static string BuildReportText()
        {
            return @"# Elementborn Unity Setup Report

## Automated setup attempted

- Ensured generated folders exist
- Ensured common Elementborn tags exist
- Ensured common Elementborn layers exist in user layer slots
- Configured patch-generated UI PNGs as Sprite assets
- Ensured an EventSystem exists in the open scene
- Added the playable test scene to Build Settings if it exists

## Recommended manual checks

1. Open `File > Build Settings` and confirm `Elementborn_Playable_Test.unity` is present after building the scene.
2. Open `Project Settings > Tags and Layers` and confirm tags: Player, Enemy, Interactable, Boat, Boss, Projectile, ResourceNode, QuestObjective.
3. Confirm layers: Player, Enemy, Interactable, Water, Projectile, Ground, BossArena, ResourceNode.
4. Open `Project Settings > Player` and confirm your target platform settings.
5. If using the old Input Manager, keep the default Horizontal/Vertical axes. The prototype scripts currently use KeyCode and old Input APIs.
6. If using the new Input System only, either enable Both in Active Input Handling or replace the prototype input scripts.
7. Run `Elementborn > Starter Content > Run All Starter Generators Safely`.
8. Run `Elementborn > Playable Setup > Build Full Test Scene`.
9. Run `Elementborn > Integration > Ensure Diagnostics Object`.
10. Press Play and review Console warnings.
";
        }

        private static void AddTag(SerializedProperty tags, string tag)
        {
            for (int i = 0; i < tags.arraySize; i++)
            {
                SerializedProperty item = tags.GetArrayElementAtIndex(i);
                if (item.stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        }

        private static int AddLayer(SerializedProperty layers, string layer, int preferredStart)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layer)
                {
                    return preferredStart;
                }
            }

            for (int i = Mathf.Max(8, preferredStart); i < Mathf.Min(32, layers.arraySize); i++)
            {
                SerializedProperty item = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrWhiteSpace(item.stringValue))
                {
                    item.stringValue = layer;
                    return i + 1;
                }
            }

            Debug.LogWarning($"No free user layer slot for {layer}. Add it manually if needed.");
            return preferredStart;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string name = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
