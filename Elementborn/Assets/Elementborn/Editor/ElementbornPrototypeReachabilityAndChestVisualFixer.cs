#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeReachabilityAndChestVisualFixer
    {
        private const string SpecificTreasureChestPrefabPath =
            "Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved/TreasureChest_0623200731.prefab";

        private const string SpecificTreasureChestExtractRoot =
            "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest_0623200731";

        [MenuItem("Elementborn/Visuals/Repair Prototype Reachability")]
        public static void RepairPrototypeReachabilityMenu()
        {
            RepairReachability(true);
        }

        [MenuItem("Elementborn/Assets/Build Specific Treasure Chest Prefab")]
        public static void BuildSpecificTreasureChestPrefabMenu()
        {
            BuildSpecificTreasureChestPrefab();
        }

        [MenuItem("Elementborn/Assets/Apply Specific Treasure Chest Visuals To All Chests")]
        public static void ApplySpecificTreasureChestVisualsMenu()
        {
            ApplyTreasureChestVisualsToAllChests(true);
        }

        [MenuItem("Elementborn/Visuals/Repair Reachability And Chest Visuals")]
        public static void RepairReachabilityAndChestVisualsMenu()
        {
            RepairReachability(false);
            BuildSpecificTreasureChestPrefab();
            ApplyTreasureChestVisualsToAllChests(false);
            MarkSceneDirtyAndSave();
        }

        public static void RepairReachability(bool save)
        {
            int disabled = 0;

            string[] decorativeRoots =
            {
                "Elementborn Clean Fantasy Hub",
                "Elementborn Art Direction Pass",
                "Generated Asset Safe Decoration",
                "Generated Asset Scene Decoration",
                "Generated Asset Showcase Gallery",
                "Generated Asset Review Gallery"
            };

            for (int i = 0; i < decorativeRoots.Length; i++)
            {
                GameObject root = GameObject.Find(decorativeRoots[i]);
                if (root != null)
                {
                    disabled += DisableCollidersUnder(root.transform);
                }
            }

            string[] decorativeNameFragments =
            {
                "Clean Character Details",
                "Clean Hostile Details",
                "Clean Gate Accent",
                "Art Direction Accessories",
                "Art Direction Hostile Details",
                "Painterly Terrain Shapes",
                "Fire Landmark",
                "Water Landmark",
                "Earth Landmark",
                "Air Landmark",
                "Village Dressing",
                "Atmosphere Dressing",
                "Path Stone",
                "Grass Tuft",
                "Brazier",
                "Crystal",
                "Tree ",
                "Boulder",
                "Pool",
                "Reed",
                "Wind Ring",
                "Cloud",
                "Stall",
                "Banner",
                "Lantern"
            };

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null)
                {
                    continue;
                }

                for (int f = 0; f < decorativeNameFragments.Length; f++)
                {
                    if (go.name.Contains(decorativeNameFragments[f]))
                    {
                        Collider collider = go.GetComponent<Collider>();
                        if (collider != null && !IsCoreFloorOrGameplayCollider(go))
                        {
                            collider.enabled = false;
                            disabled++;
                        }

                        break;
                    }
                }
            }

            RestoreCoreFloorColliders();
            RestoreInteractableTriggerColliders();

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("Prototype reachability repaired. Disabled decorative colliders=" + disabled);
        }

        public static void BuildSpecificTreasureChestPrefab()
        {
            EnsureFolder("Assets/Elementborn/Generated");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs/ImportedModels");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved");

            string fbxPath = FindSpecificTreasureChestFbx();
            if (string.IsNullOrWhiteSpace(fbxPath))
            {
                Debug.LogWarning(
                    "Could not find specific treasure chest FBX. Expected an extracted FBX matching Meshy_AI_Treasure_Chest_0623200731_texture_fbx under AutoImported/TreasureChest_0623200731 or generated assets extraction folders.");
                return;
            }

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (modelAsset == null)
            {
                Debug.LogWarning("Found treasure chest FBX path, but Unity could not load it as a model: " + fbxPath);
                return;
            }

            GameObject root = new GameObject("TreasureChest_0623200731");
            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null)
            {
                model = Object.Instantiate(modelAsset);
            }

            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            NormalizeInstance(model.transform, 0.95f, Vector3.zero);
            DisableCollidersUnder(model.transform);

            ElementbornPrototypeImportedModelTag tag = root.AddComponent<ElementbornPrototypeImportedModelTag>();
            tag.sourceAssetPath = fbxPath;
            tag.modelRole = "TreasureChest/Specific";
            tag.notes = "Specific requested chest visual: Meshy_AI_Treasure_Chest_0623200731_texture_fbx.";

            ElementbornPrototypeGeneratedAssetSlot slot = root.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            slot.slotName = "Treasure Chest 0623200731";
            slot.preferredPrefabName = "TreasureChest_0623200731";
            slot.role = "Prop";
            slot.element = "None";
            slot.visualApplied = true;

            PrefabUtility.SaveAsPrefabAsset(root, SpecificTreasureChestPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Built specific treasure chest prefab from " + fbxPath + " -> " + SpecificTreasureChestPrefabPath);
        }

        public static void ApplyTreasureChestVisualsToAllChests(bool save)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpecificTreasureChestPrefabPath);
            if (prefab == null)
            {
                BuildSpecificTreasureChestPrefab();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpecificTreasureChestPrefabPath);
            }

            if (prefab == null)
            {
                Debug.LogWarning("Specific treasure chest prefab is not available yet. Run IMPORT_V99_TREASURE_CHEST_0623200731.ps1 or Build Specific Treasure Chest Prefab after the model imports.");
                return;
            }

            int applied = 0;
            ElementbornPrototypeInteractable[] interactables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null)
                {
                    continue;
                }

                bool isChest =
                    interactable.kind == ElementbornPrototypeInteractableKind.LootChest ||
                    interactable.name.ToLowerInvariant().Contains("chest") ||
                    interactable.displayName.ToLowerInvariant().Contains("chest");

                if (!isChest)
                {
                    continue;
                }

                ApplyTreasureChestVisual(interactable.gameObject, prefab);
                applied++;
            }

            // Fallback for any chest objects that do not have the interactable component.
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null || !go.name.ToLowerInvariant().Contains("chest"))
                {
                    continue;
                }

                if (go.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null)
                {
                    continue;
                }

                ApplyTreasureChestVisual(go, prefab);
                applied++;
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("Applied specific treasure chest visual to chests. Count=" + applied);
        }

        private static void ApplyTreasureChestVisual(GameObject chest, GameObject prefab)
        {
            if (chest == null || prefab == null)
            {
                return;
            }

            RemoveOldChestVisuals(chest.transform);

            // Hide old blocky chest visuals, but keep the parent object and interaction trigger.
            Renderer rootRenderer = chest.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }

            for (int i = 0; i < chest.transform.childCount; i++)
            {
                Transform child = chest.transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                bool keepChild =
                    child.name.Contains("Prototype Interaction Radius") ||
                    child.name.Contains("Prototype Quest Marker") ||
                    child.GetComponent<TextMesh>() != null ||
                    child.GetComponentInChildren<TextMesh>(true) != null;

                if (keepChild)
                {
                    continue;
                }

                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++)
                {
                    if (renderers[r] != null)
                    {
                        renderers[r].enabled = false;
                    }
                }

                Collider[] colliders = child.GetComponentsInChildren<Collider>(true);
                for (int c = 0; c < colliders.Length; c++)
                {
                    if (colliders[c] != null)
                    {
                        colliders[c].enabled = false;
                    }
                }
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab);
            }

            instance.name = "Specific Treasure Chest Visual";
            instance.transform.SetParent(chest.transform, false);
            instance.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            NormalizeInstance(instance.transform, 0.95f, new Vector3(0f, -0.05f, 0f));
            DisableCollidersUnder(instance.transform);

            ElementbornPrototypeGeneratedAssetSlot slot = chest.GetComponent<ElementbornPrototypeGeneratedAssetSlot>();
            if (slot == null)
            {
                slot = chest.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            }

            slot.slotName = chest.name;
            slot.preferredPrefabName = "TreasureChest_0623200731";
            slot.role = "Prop";
            slot.element = "None";
            slot.visualApplied = true;
        }

        private static void RemoveOldChestVisuals(Transform chest)
        {
            for (int i = chest.childCount - 1; i >= 0; i--)
            {
                Transform child = chest.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                string lower = child.name.ToLowerInvariant();
                bool remove =
                    lower.Contains("treasure") ||
                    lower.Contains("chest visual") ||
                    lower.Contains("generated treasure") ||
                    lower.Contains("safe treasure") ||
                    lower.Contains("preset treasure") ||
                    child.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null;

                bool keep =
                    child.name.Contains("Prototype Interaction Radius") ||
                    child.name.Contains("Prototype Quest Marker") ||
                    child.GetComponent<TextMesh>() != null ||
                    child.GetComponentInChildren<TextMesh>(true) != null;

                if (remove && !keep)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static string FindSpecificTreasureChestFbx()
        {
            string direct = FindFirstFbxUnder("Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest_0623200731");
            if (!string.IsNullOrWhiteSpace(direct))
            {
                return direct;
            }

            direct = FindFirstFbxUnder("Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChestSpecific");
            if (!string.IsNullOrWhiteSpace(direct))
            {
                return direct;
            }

            direct = FindFirstFbxUnder("Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest");
            if (!string.IsNullOrWhiteSpace(direct))
            {
                return direct;
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new string[]
            {
                "Assets/Elementborn/Art/Models/MeshyImported/AutoImported",
                "Assets/Elementborn/Art/Models/MeshyImported"
            });

            string fallback = "";
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string normalized = Normalize(path);
                bool looksLikeTreasureChest = normalized.Contains("treasure") && normalized.Contains("chest");
                bool hasRequestedId = normalized.Contains("0623200731");

                if (looksLikeTreasureChest && hasRequestedId)
                {
                    return path;
                }

                if (looksLikeTreasureChest && string.IsNullOrWhiteSpace(fallback))
                {
                    fallback = path;
                }
            }

            return fallback;
        }

        private static string FindFirstFbxUnder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return "";
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return "";
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string lower = value.ToLowerInvariant();
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static int DisableCollidersUnder(Transform root)
        {
            if (root == null)
            {
                return 0;
            }

            int count = 0;
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    continue;
                }

                if (IsCoreFloorOrGameplayCollider(collider.gameObject))
                {
                    continue;
                }

                collider.enabled = false;
                count++;
            }

            return count;
        }

        private static bool IsCoreFloorOrGameplayCollider(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            string name = go.name;
            return
                name == "Elementborn Test Arena" ||
                name == "Prototype Ground" ||
                name == "Fire Quarter" ||
                name == "Water Quarter" ||
                name == "Earth Quarter" ||
                name == "Air Quarter" ||
                name == "Fire Ground" ||
                name == "Water Ground" ||
                name == "Earth Ground" ||
                name == "Air Ground" ||
                name.Contains("Road") ||
                go.GetComponent<CharacterController>() != null ||
                go.GetComponent<ElementbornPrototypeInteractable>() != null;
        }

        private static void RestoreCoreFloorColliders()
        {
            string[] floors =
            {
                "Elementborn Test Arena",
                "Prototype Ground",
                "Fire Quarter",
                "Water Quarter",
                "Earth Quarter",
                "Air Quarter",
                "Fire Ground",
                "Water Ground",
                "Earth Ground",
                "Air Ground",
                "North Road",
                "South Road",
                "East Road",
                "West Road"
            };

            for (int i = 0; i < floors.Length; i++)
            {
                GameObject floor = GameObject.Find(floors[i]);
                if (floor == null)
                {
                    continue;
                }

                Collider collider = floor.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        private static void RestoreInteractableTriggerColliders()
        {
            ElementbornPrototypeInteractable[] interactables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null)
                {
                    continue;
                }

                interactable.EnsureInteractionRadius();
            }
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
            float scale = Mathf.Clamp(targetHeight / height, 0.00005f, 0.75f);
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

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
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
