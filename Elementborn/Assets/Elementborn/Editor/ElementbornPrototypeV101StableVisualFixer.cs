#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeV101StableVisualFixer
    {
        private const string ChestFolder = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest_0623200731";
        private const string ChannelerFolder = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/ChannelerHeroNone_0624153647";
        private const string AxolotlFolder = "Assets/Elementborn/Art/Models/MeshyImported/PinkEyeAxolotl";

        [MenuItem("Elementborn/Assets/V101 Diagnose Exact Model Imports")]
        public static void DiagnoseExactModelImports()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("Elementborn V101 exact model import diagnosis");
            AppendFolderReport(report, "Chest", ChestFolder, "treasurechest0623200731");
            AppendFolderReport(report, "Channeler", ChannelerFolder, "channelerheronone0624153647");
            AppendFolderReport(report, "Axolotl", AxolotlFolder, "axolotl");
            Debug.Log(report.ToString());
        }

        [MenuItem("Elementborn/Assets/V101 Apply Stable Visual Fixes")]
        public static void ApplyStableVisualFixesMenu()
        {
            RestoreAllChestVisibility(false);
            ApplyVisibleProceduralChests(false);
            ApplyChannelerOrProceduralPlayer(false);
            RepairMovementBlockingDecorativeColliders(false);
            MarkSceneDirtyAndSave();
        }

        [MenuItem("Elementborn/Assets/V101 Restore All Chest Visibility")]
        public static void RestoreAllChestVisibilityMenu()
        {
            RestoreAllChestVisibility(true);
        }

        [MenuItem("Elementborn/Assets/V101 Apply Visible Procedural Chests")]
        public static void ApplyVisibleProceduralChestsMenu()
        {
            ApplyVisibleProceduralChests(true);
        }

        [MenuItem("Elementborn/Assets/V101 Apply Channeler Or Procedural Player")]
        public static void ApplyChannelerOrProceduralPlayerMenu()
        {
            ApplyChannelerOrProceduralPlayer(true);
        }

        [MenuItem("Elementborn/Assets/V101 Apply Axolotl Or Procedural Player")]
        public static void ApplyAxolotlOrProceduralPlayerMenu()
        {
            ApplyAxolotlOrProceduralPlayer(true);
        }

        [MenuItem("Elementborn/Assets/V101 Force Procedural Robed Player")]
        public static void ForceProceduralRobedPlayerMenu()
        {
            ForceProceduralRobedPlayer(true);
        }

        [MenuItem("Elementborn/Visuals/V101 Repair Movement Blocking Decorative Colliders")]
        public static void RepairMovementBlockingDecorativeCollidersMenu()
        {
            RepairMovementBlockingDecorativeColliders(true);
        }

        public static void RestoreAllChestVisibility(bool save)
        {
            int restored = 0;
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || !LooksLikeChest(go))
                {
                    continue;
                }

                Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++)
                {
                    Renderer renderer = renderers[r];
                    if (renderer == null)
                    {
                        continue;
                    }

                    bool keepHidden =
                        renderer.gameObject.name.Contains("Old Hidden") ||
                        renderer.gameObject.name.Contains("Disabled");

                    if (!keepHidden && !renderer.enabled)
                    {
                        renderer.enabled = true;
                        restored++;
                    }
                }
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("V101 restored chest renderers: " + restored);
        }

        public static void ApplyVisibleProceduralChests(bool save)
        {
            int count = 0;

            ElementbornPrototypeInteractable[] interactables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null)
                {
                    continue;
                }

                if (IsChestInteractable(interactable))
                {
                    CreateOrReplaceStableChestVisual(interactable.transform);
                    count++;
                }
            }

            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || !LooksLikeChest(go))
                {
                    continue;
                }

                if (go.GetComponent<ElementbornPrototypeInteractable>() != null)
                {
                    continue;
                }

                CreateOrReplaceStableChestVisual(go.transform);
                count++;
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("V101 applied stable visible procedural chest visuals: " + count);
        }

        public static void ApplyChannelerOrProceduralPlayer(bool save)
        {
            bool applied = ApplyImportedPlayerVisual(ChannelerFolder, "channelerheronone0624153647", "V101 Channeler Player Visual", 1.65f, ElementbornPrototypeModelAnimationMode.Idle);
            if (!applied)
            {
                ForceProceduralRobedPlayer(false);
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }
        }

        public static void ApplyAxolotlOrProceduralPlayer(bool save)
        {
            bool applied = ApplyImportedPlayerVisual(AxolotlFolder, "axolotl", "V101 Axolotl Player Visual", 1.25f, ElementbornPrototypeModelAnimationMode.Swim);
            if (!applied)
            {
                ForceProceduralRobedPlayer(false);
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }
        }

        public static void ForceProceduralRobedPlayer(bool save)
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                Debug.LogWarning("V101 could not find Prototype Player.");
                return;
            }

            RemoveExistingV101PlayerVisuals(player.transform);

            GameObject robe = CreateProceduralRobedPlayerVisual(player.transform);
            if (HasVisibleRenderers(robe))
            {
                Renderer playerRenderer = player.GetComponent<Renderer>();
                if (playerRenderer != null)
                {
                    playerRenderer.enabled = false;
                }

                RemoveKnownCharacterAccessoryChildren(player.transform);
                Debug.Log("V101 applied procedural robed player visual.");
            }
            else
            {
                Renderer playerRenderer = player.GetComponent<Renderer>();
                if (playerRenderer != null)
                {
                    playerRenderer.enabled = true;
                }

                Debug.LogWarning("V101 procedural player visual was not visible. Keeping blocky player visible.");
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }
        }

        public static void RepairMovementBlockingDecorativeColliders(bool save)
        {
            int disabled = 0;
            string[] visualRootNames =
            {
                "Elementborn Clean Fantasy Hub",
                "Elementborn Art Direction Pass",
                "Generated Asset Review Gallery",
                "Generated Asset Showcase Gallery",
                "Generated Asset Safe Decoration",
                "Generated Asset Scene Decoration"
            };

            for (int i = 0; i < visualRootNames.Length; i++)
            {
                GameObject root = GameObject.Find(visualRootNames[i]);
                if (root != null)
                {
                    disabled += DisableDecorativeCollidersUnder(root.transform);
                }
            }

            // Disable imported/model child visual colliders everywhere, but keep root gameplay colliders.
            ElementbornPrototypeImportedModelTag[] importedTags =
                Object.FindObjectsByType<ElementbornPrototypeImportedModelTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < importedTags.Length; i++)
            {
                if (importedTags[i] != null)
                {
                    disabled += DisableDecorativeCollidersUnder(importedTags[i].transform);
                }
            }

            RestoreCoreInteractionColliders();

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("V101 disabled decorative/model colliders: " + disabled);
        }

        private static bool ApplyImportedPlayerVisual(string folder, string normalizedHint, string visualName, float targetHeight, ElementbornPrototypeModelAnimationMode mode)
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                Debug.LogWarning("V101 could not find Prototype Player.");
                return false;
            }

            string fbx = FindFirstFbx(folder, normalizedHint);
            if (string.IsNullOrWhiteSpace(fbx))
            {
                Debug.LogWarning("V101 could not find imported player FBX under " + folder + " with hint " + normalizedHint);
                return false;
            }

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (modelAsset == null)
            {
                Debug.LogWarning("V101 found FBX but Unity could not load it as a model: " + fbx);
                return false;
            }

            RemoveExistingV101PlayerVisuals(player.transform);

            GameObject root = new GameObject(visualName);
            root.transform.SetParent(player.transform, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null)
            {
                model = Object.Instantiate(modelAsset);
            }

            model.name = "Imported Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            NormalizeInstance(root.transform, targetHeight, new Vector3(0f, -1.0f, 0f));
            DisableDecorativeCollidersUnder(root.transform);

            ElementbornPrototypeImportedModelAnimator animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = mode;
            animator.bobAmplitude = mode == ElementbornPrototypeModelAnimationMode.Swim ? 0.04f : 0.018f;
            animator.boneWiggleDegrees = mode == ElementbornPrototypeModelAnimationMode.Swim ? 5.0f : 1.6f;

            ElementbornPrototypeImportedModelTag tag = root.AddComponent<ElementbornPrototypeImportedModelTag>();
            tag.sourceAssetPath = fbx;
            tag.modelRole = "PlayerVisual";
            tag.notes = "V101 direct imported player child visual. Gameplay remains on Prototype Player.";

            if (!HasVisibleRenderers(root))
            {
                Object.DestroyImmediate(root);
                Debug.LogWarning("V101 imported player visual had no visible renderers: " + fbx);
                return false;
            }

            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.enabled = false;
            }

            RemoveKnownCharacterAccessoryChildren(player.transform);
            Debug.Log("V101 applied imported player visual from " + fbx);
            return true;
        }

        private static GameObject CreateProceduralRobedPlayerVisual(Transform player)
        {
            RemoveExistingV101PlayerVisuals(player);

            GameObject root = new GameObject("V101 Procedural Robed Player Visual");
            root.transform.SetParent(player, false);
            root.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Material robe = CreateMaterial("V101 Player Robe Blue", new Color(0.08f, 0.18f, 0.48f));
            Material trim = CreateMaterial("V101 Player Trim Gold", new Color(0.95f, 0.68f, 0.25f));
            Material skin = CreateMaterial("V101 Player Skin", new Color(0.86f, 0.62f, 0.46f));
            Material glow = CreateMaterial("V101 Player Element Glow", new Color(0.50f, 0.90f, 1.0f));

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Robe Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            body.transform.localScale = new Vector3(0.55f, 0.78f, 0.55f);
            body.GetComponent<Renderer>().sharedMaterial = robe;
            DestroyCollider(body);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            head.transform.localScale = new Vector3(0.34f, 0.34f, 0.34f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            DestroyCollider(head);

            GameObject mantle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mantle.name = "Gold Mantle";
            mantle.transform.SetParent(root.transform, false);
            mantle.transform.localPosition = new Vector3(0f, 1.28f, 0f);
            mantle.transform.localScale = new Vector3(0.42f, 0.035f, 0.42f);
            mantle.GetComponent<Renderer>().sharedMaterial = trim;
            DestroyCollider(mantle);

            GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gem.name = "Element Gem";
            gem.transform.SetParent(root.transform, false);
            gem.transform.localPosition = new Vector3(0f, 1.18f, -0.38f);
            gem.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            gem.GetComponent<Renderer>().sharedMaterial = glow;
            DestroyCollider(gem);

            GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "Wide Hat";
            hat.transform.SetParent(root.transform, false);
            hat.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            hat.transform.localScale = new Vector3(0.45f, 0.035f, 0.45f);
            hat.GetComponent<Renderer>().sharedMaterial = robe;
            DestroyCollider(hat);

            ElementbornPrototypeImportedModelAnimator animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = ElementbornPrototypeModelAnimationMode.Idle;
            animator.bobAmplitude = 0.018f;
            animator.boneWiggleDegrees = 1.2f;

            return root;
        }

        private static GameObject CreateOrReplaceStableChestVisual(Transform chest)
        {
            RemoveV101ChestVisual(chest);

            GameObject root = new GameObject("V101 Stable Chest Visual");
            root.transform.SetParent(chest, false);
            root.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Material wood = CreateMaterial("V101 Chest Wood", new Color(0.42f, 0.20f, 0.07f));
            Material darkWood = CreateMaterial("V101 Chest Dark Wood", new Color(0.23f, 0.10f, 0.035f));
            Material gold = CreateMaterial("V101 Chest Gold", new Color(0.95f, 0.65f, 0.20f));

            GameObject baseBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseBox.name = "Stable Chest Base";
            baseBox.transform.SetParent(root.transform, false);
            baseBox.transform.localPosition = new Vector3(0f, 0.20f, 0f);
            baseBox.transform.localScale = new Vector3(1.0f, 0.38f, 0.65f);
            baseBox.GetComponent<Renderer>().sharedMaterial = wood;
            DestroyCollider(baseBox);

            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = "Stable Chest Lid";
            lid.transform.SetParent(root.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.50f, 0f);
            lid.transform.localScale = new Vector3(1.05f, 0.24f, 0.70f);
            lid.GetComponent<Renderer>().sharedMaterial = darkWood;
            DestroyCollider(lid);

            GameObject bandA = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bandA.name = "Gold Band A";
            bandA.transform.SetParent(root.transform, false);
            bandA.transform.localPosition = new Vector3(-0.28f, 0.36f, -0.36f);
            bandA.transform.localScale = new Vector3(0.10f, 0.58f, 0.04f);
            bandA.GetComponent<Renderer>().sharedMaterial = gold;
            DestroyCollider(bandA);

            GameObject bandB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bandB.name = "Gold Band B";
            bandB.transform.SetParent(root.transform, false);
            bandB.transform.localPosition = new Vector3(0.28f, 0.36f, -0.36f);
            bandB.transform.localScale = new Vector3(0.10f, 0.58f, 0.04f);
            bandB.GetComponent<Renderer>().sharedMaterial = gold;
            DestroyCollider(bandB);

            GameObject lockPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lockPlate.name = "Gold Lock Plate";
            lockPlate.transform.SetParent(root.transform, false);
            lockPlate.transform.localPosition = new Vector3(0f, 0.27f, -0.39f);
            lockPlate.transform.localScale = new Vector3(0.22f, 0.18f, 0.05f);
            lockPlate.GetComponent<Renderer>().sharedMaterial = gold;
            DestroyCollider(lockPlate);

            // Keep the original root renderer visible for one round for safety; the new visual sits above it.
            Renderer rootRenderer = chest.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = true;
            }

            ElementbornPrototypeGeneratedAssetSlot slot = chest.GetComponent<ElementbornPrototypeGeneratedAssetSlot>();
            if (slot == null)
            {
                slot = chest.gameObject.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            }

            slot.slotName = chest.name;
            slot.preferredPrefabName = "V101StableProceduralChest";
            slot.role = "ChestVisual";
            slot.element = "None";
            slot.visualApplied = HasVisibleRenderers(root);

            return root;
        }

        private static void RemoveV101ChestVisual(Transform chest)
        {
            for (int i = chest.childCount - 1; i >= 0; i--)
            {
                Transform child = chest.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                bool remove =
                    child.name.Contains("V101 Stable Chest Visual") ||
                    child.name.Contains("Fallback Treasure Chest Visual") ||
                    child.name.Contains("Exact Treasure Chest Visual") ||
                    child.name.Contains("Specific Treasure Chest Visual");

                if (remove)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void RemoveExistingV101PlayerVisuals(Transform player)
        {
            for (int i = player.childCount - 1; i >= 0; i--)
            {
                Transform child = player.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                bool remove =
                    child.name.Contains("V101 ") ||
                    child.name.Contains("Channeler Player Visual") ||
                    child.name.Contains("Axolotl Player Visual") ||
                    child.name.Contains("Imported Prototype Player Visual") ||
                    child.name.Contains("Procedural Robed Player Visual") ||
                    child.GetComponent<ElementbornPrototypeImportedModelTag>() != null;

                bool keep =
                    child.name.Contains("Prototype Quest Marker") ||
                    child.GetComponentInChildren<Camera>(true) != null;

                if (remove && !keep)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void RemoveKnownCharacterAccessoryChildren(Transform character)
        {
            string[] names = { "Clean Character Details", "Art Direction Accessories" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform child = character.Find(names[i]);
                if (child != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void AppendFolderReport(StringBuilder report, string label, string folder, string hint)
        {
            report.AppendLine(label + ":");
            report.AppendLine("  Folder: " + folder);
            report.AppendLine("  Folder exists: " + AssetDatabase.IsValidFolder(folder));
            string fbx = FindFirstFbx(folder, hint);
            report.AppendLine("  FBX: " + (string.IsNullOrWhiteSpace(fbx) ? "(missing)" : fbx));

            if (!string.IsNullOrWhiteSpace(fbx))
            {
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
                report.AppendLine("  Loadable GameObject: " + (asset != null));
                if (asset != null)
                {
                    Renderer[] renderers = asset.GetComponentsInChildren<Renderer>(true);
                    report.AppendLine("  Renderer count on imported asset: " + renderers.Length);
                }
            }
        }

        private static string FindFirstFbx(string folder, string normalizedHint)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { folder });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return path;
                    }
                }
            }

            string[] searchFolders =
            {
                "Assets/Elementborn/Art/Models/MeshyImported/AutoImported",
                "Assets/Elementborn/Art/Models/MeshyImported"
            };

            for (int s = 0; s < searchFolders.Length; s++)
            {
                if (!AssetDatabase.IsValidFolder(searchFolders[s]))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { searchFolders[s] });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string normalized = NormalizeName(path);
                    if (normalized.Contains(normalizedHint))
                    {
                        return path;
                    }
                }
            }

            return "";
        }

        private static bool LooksLikeChest(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            string lower = go.name.ToLowerInvariant();
            if (lower.Contains("chest"))
            {
                return true;
            }

            ElementbornPrototypeInteractable interactable = go.GetComponent<ElementbornPrototypeInteractable>();
            return interactable != null && IsChestInteractable(interactable);
        }

        private static bool IsChestInteractable(ElementbornPrototypeInteractable interactable)
        {
            if (interactable == null)
            {
                return false;
            }

            string lowerName = interactable.name.ToLowerInvariant();
            string display = interactable.displayName != null ? interactable.displayName.ToLowerInvariant() : "";
            return
                interactable.kind == ElementbornPrototypeInteractableKind.LootChest ||
                lowerName.Contains("chest") ||
                display.Contains("chest");
        }

        private static int DisableDecorativeCollidersUnder(Transform root)
        {
            int count = 0;
            if (root == null)
            {
                return count;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    continue;
                }

                if (IsCoreCollider(collider.gameObject))
                {
                    continue;
                }

                collider.enabled = false;
                count++;
            }

            return count;
        }

        private static void RestoreCoreInteractionColliders()
        {
            string[] coreNames =
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

            for (int i = 0; i < coreNames.Length; i++)
            {
                GameObject core = GameObject.Find(coreNames[i]);
                if (core == null)
                {
                    continue;
                }

                Collider collider = core.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            ElementbornPrototypeInteractable[] interactables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                if (interactables[i] != null)
                {
                    interactables[i].EnsureInteractionRadius();
                }
            }
        }

        private static bool IsCoreCollider(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            string name = go.name;
            return
                name == "Elementborn Test Arena" ||
                name == "Prototype Ground" ||
                name.Contains("Ground") ||
                name.Contains("Quarter") ||
                name.Contains("Road") ||
                go.GetComponent<CharacterController>() != null ||
                go.GetComponent<ElementbornPrototypeInteractable>() != null;
        }

        private static bool HasVisibleRenderers(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.enabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static void NormalizeInstance(Transform modelRoot, float targetHeight, Vector3 desiredLocalOffset)
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                modelRoot.localPosition = desiredLocalOffset;
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
            float scale = Mathf.Clamp(targetHeight / height, 0.00005f, 1.0f);
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

        private static void DestroyCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Hidden/InternalErrorShader"));
            material.name = name;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            material.color = color;
            return material;
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string lower = value.ToLowerInvariant();
            StringBuilder builder = new StringBuilder();
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
