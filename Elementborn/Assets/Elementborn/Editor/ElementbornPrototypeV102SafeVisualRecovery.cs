#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeV102SafeVisualRecovery
    {
        private const string ChestFolder = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest_0623200731";
        private const string ChannelerFolder = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/ChannelerHeroNone_0624153647";
        private const string AxolotlFolder = "Assets/Elementborn/Art/Models/MeshyImported/PinkEyeAxolotl";

        [MenuItem("Elementborn/Assets/V102 Full Safe Visual Recovery")]
        public static void FullSafeVisualRecoveryMenu()
        {
            FullSafeVisualRecovery(true);
        }

        [MenuItem("Elementborn/Assets/V102 Diagnose Exact Imports")]
        public static void DiagnoseExactImportsMenu()
        {
            Debug.Log(BuildImportReport());
        }

        [MenuItem("Elementborn/Assets/V102 Force Visible Chests")]
        public static void ForceVisibleChestsMenu()
        {
            ForceVisibleChests(true);
        }

        [MenuItem("Elementborn/Assets/V102 Force Robed Player")]
        public static void ForceRobedPlayerMenu()
        {
            ForceRobedPlayer(true);
        }

        [MenuItem("Elementborn/Assets/V102 Try Channeler Player Then Robed")]
        public static void TryChannelerThenRobedMenu()
        {
            TryChannelerThenRobed(true);
        }

        [MenuItem("Elementborn/Assets/V102 Try Axolotl Player Then Robed")]
        public static void TryAxolotlThenRobedMenu()
        {
            TryAxolotlThenRobed(true);
        }

        [MenuItem("Elementborn/Visuals/V102 Repair Movement Blockers")]
        public static void RepairMovementBlockersMenu()
        {
            RepairMovementBlockers(true);
        }

        public static void FullSafeVisualRecovery(bool save)
        {
            ForceVisibleChests(false);
            TryChannelerThenRobed(false);
            RepairMovementBlockers(false);

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("V102 full safe visual recovery applied.");
        }

        public static void ForceVisibleChests(bool save)
        {
            int fixedCount = 0;

            ElementbornPrototypeInteractable[] interactables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null)
                {
                    continue;
                }

                if (IsChest(interactable.gameObject, interactable))
                {
                    ForceOneChestVisible(interactable.gameObject);
                    fixedCount++;
                }
            }

            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                GameObject go = all[i];
                if (go == null || !go.name.ToLowerInvariant().Contains("chest"))
                {
                    continue;
                }

                if (go.GetComponent<ElementbornPrototypeInteractable>() != null)
                {
                    continue;
                }

                ForceOneChestVisible(go);
                fixedCount++;
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("V102 forced visible chests: " + fixedCount);
        }

        public static void TryChannelerThenRobed(bool save)
        {
            bool imported = TryImportedPlayerVisual(ChannelerFolder, "channelerheronone0624153647", "V102 Channeler Player Visual", 1.65f, ElementbornPrototypeModelAnimationMode.Idle);
            if (!imported)
            {
                ForceRobedPlayer(false);
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }
        }

        public static void TryAxolotlThenRobed(bool save)
        {
            bool imported = TryImportedPlayerVisual(AxolotlFolder, "axolotl", "V102 Axolotl Player Visual", 1.25f, ElementbornPrototypeModelAnimationMode.Swim);
            if (!imported)
            {
                ForceRobedPlayer(false);
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }
        }

        public static void ForceRobedPlayer(bool save)
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                Debug.LogWarning("V102 could not find Prototype Player.");
                return;
            }

            RemoveUnsafePlayerVisuals(player.transform);

            GameObject visual = BuildRobedPlayerVisual(player.transform);
            if (HasVisibleRenderers(visual))
            {
                Renderer rootRenderer = player.GetComponent<Renderer>();
                if (rootRenderer != null)
                {
                    rootRenderer.enabled = false;
                }

                RemoveAccessoryChildren(player.transform);
                Debug.Log("V102 forced visible robed player visual.");
            }
            else
            {
                Renderer rootRenderer = player.GetComponent<Renderer>();
                if (rootRenderer != null)
                {
                    rootRenderer.enabled = true;
                }

                Debug.LogWarning("V102 robed player visual was not visible; kept root player renderer visible.");
            }

            if (save)
            {
                MarkSceneDirtyAndSave();
            }
        }

        public static void RepairMovementBlockers(bool save)
        {
            int disabled = 0;

            string[] visualRoots =
            {
                "Elementborn Clean Fantasy Hub",
                "Elementborn Art Direction Pass",
                "Generated Asset Review Gallery",
                "Generated Asset Showcase Gallery",
                "Generated Asset Safe Decoration",
                "Generated Asset Scene Decoration"
            };

            for (int i = 0; i < visualRoots.Length; i++)
            {
                GameObject root = GameObject.Find(visualRoots[i]);
                if (root != null)
                {
                    disabled += DisableNonGameplayColliders(root.transform);
                }
            }

            ElementbornPrototypeImportedModelTag[] imported =
                Object.FindObjectsByType<ElementbornPrototypeImportedModelTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < imported.Length; i++)
            {
                if (imported[i] != null)
                {
                    disabled += DisableNonGameplayColliders(imported[i].transform);
                }
            }

            RestoreGameplayColliders();

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("V102 repaired movement blockers. Disabled decorative colliders=" + disabled);
        }

        public static string BuildImportReport()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("Elementborn V102 Exact Import Diagnosis");
            AppendModelReport(report, "Chest", ChestFolder, "treasurechest0623200731");
            AppendModelReport(report, "Channeler", ChannelerFolder, "channelerheronone0624153647");
            AppendModelReport(report, "Axolotl", AxolotlFolder, "axolotl");
            return report.ToString();
        }

        private static void ForceOneChestVisible(GameObject chest)
        {
            if (chest == null)
            {
                return;
            }

            // Critical: undo old V99/V100 behavior first.
            Renderer rootRenderer = chest.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = true;
                rootRenderer.sharedMaterial = CreateMaterial("V102 Chest Root Wood", new Color(0.40f, 0.20f, 0.08f));
            }

            RemoveBadChestChildren(chest.transform);
            BuildStableChestVisual(chest.transform);

            ElementbornPrototypeGeneratedAssetSlot slot = chest.GetComponent<ElementbornPrototypeGeneratedAssetSlot>();
            if (slot == null)
            {
                slot = chest.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            }

            slot.slotName = chest.name;
            slot.preferredPrefabName = "V102StableChest";
            slot.role = "ChestVisual";
            slot.element = "None";
            slot.visualApplied = true;
        }

        private static void RemoveBadChestChildren(Transform chest)
        {
            for (int i = chest.childCount - 1; i >= 0; i--)
            {
                Transform child = chest.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                bool keep =
                    child.name.Contains("Prototype Interaction Radius") ||
                    child.name.Contains("Prototype Quest Marker") ||
                    child.GetComponentInChildren<TextMesh>(true) != null;

                bool remove =
                    child.name.Contains("Exact Treasure Chest Visual") ||
                    child.name.Contains("Specific Treasure Chest Visual") ||
                    child.name.Contains("Fallback Treasure Chest Visual") ||
                    child.name.Contains("V101 Stable Chest Visual") ||
                    child.name.Contains("V102 Stable Chest Visual") ||
                    child.name.Contains("Chest Visual") ||
                    child.name.Contains("Treasure") ||
                    child.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null;

                if (remove && !keep)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static GameObject BuildStableChestVisual(Transform chest)
        {
            GameObject root = new GameObject("V102 Stable Chest Visual");
            root.transform.SetParent(chest, false);
            root.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Material wood = CreateMaterial("V102 Chest Wood", new Color(0.42f, 0.20f, 0.07f));
            Material dark = CreateMaterial("V102 Chest Lid Dark Wood", new Color(0.23f, 0.10f, 0.035f));
            Material gold = CreateMaterial("V102 Chest Gold", new Color(0.95f, 0.65f, 0.20f));

            CreateBox(root.transform, "Chest Base", new Vector3(0f, 0.20f, 0f), new Vector3(1.05f, 0.38f, 0.65f), wood);
            CreateBox(root.transform, "Chest Lid", new Vector3(0f, 0.50f, 0f), new Vector3(1.10f, 0.25f, 0.70f), dark);
            CreateBox(root.transform, "Gold Band Left", new Vector3(-0.28f, 0.36f, -0.36f), new Vector3(0.10f, 0.58f, 0.05f), gold);
            CreateBox(root.transform, "Gold Band Right", new Vector3(0.28f, 0.36f, -0.36f), new Vector3(0.10f, 0.58f, 0.05f), gold);
            CreateBox(root.transform, "Lock Plate", new Vector3(0f, 0.27f, -0.40f), new Vector3(0.24f, 0.18f, 0.06f), gold);

            return root;
        }

        private static bool TryImportedPlayerVisual(string folder, string normalizedHint, string visualName, float targetHeight, ElementbornPrototypeModelAnimationMode mode)
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null)
            {
                Debug.LogWarning("V102 could not find Prototype Player.");
                return false;
            }

            string fbx = FindFirstFbx(folder, normalizedHint);
            if (string.IsNullOrWhiteSpace(fbx))
            {
                Debug.LogWarning("V102 could not find imported player FBX: " + folder + " hint=" + normalizedHint);
                return false;
            }

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (modelAsset == null)
            {
                Debug.LogWarning("V102 found FBX but Unity could not load it: " + fbx);
                return false;
            }

            RemoveUnsafePlayerVisuals(player.transform);

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
            DisableNonGameplayColliders(root.transform);

            ElementbornPrototypeImportedModelAnimator animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = mode;
            animator.bobAmplitude = mode == ElementbornPrototypeModelAnimationMode.Swim ? 0.04f : 0.018f;
            animator.boneWiggleDegrees = mode == ElementbornPrototypeModelAnimationMode.Swim ? 5.0f : 1.6f;

            ElementbornPrototypeImportedModelTag tag = root.AddComponent<ElementbornPrototypeImportedModelTag>();
            tag.sourceAssetPath = fbx;
            tag.modelRole = "V102 Player Visual";
            tag.notes = "V102 imported visual only; gameplay remains on Prototype Player.";

            if (!HasVisibleRenderers(root))
            {
                Object.DestroyImmediate(root);
                return false;
            }

            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.enabled = false;
            }

            RemoveAccessoryChildren(player.transform);
            Debug.Log("V102 applied imported player visual from " + fbx);
            return true;
        }

        private static GameObject BuildRobedPlayerVisual(Transform player)
        {
            RemoveUnsafePlayerVisuals(player);

            GameObject root = new GameObject("V102 Procedural Robed Player Visual");
            root.transform.SetParent(player, false);
            root.transform.localPosition = new Vector3(0f, -0.95f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Material robe = CreateMaterial("V102 Player Robe Blue", new Color(0.08f, 0.18f, 0.48f));
            Material trim = CreateMaterial("V102 Player Trim Gold", new Color(0.95f, 0.68f, 0.25f));
            Material skin = CreateMaterial("V102 Player Skin", new Color(0.86f, 0.62f, 0.46f));
            Material glow = CreateMaterial("V102 Player Element Glow", new Color(0.50f, 0.90f, 1.0f));

            CreateCapsule(root.transform, "Robe Body", new Vector3(0f, 0.88f, 0f), new Vector3(0.55f, 0.78f, 0.55f), robe);
            CreateSphere(root.transform, "Head", new Vector3(0f, 1.75f, 0f), new Vector3(0.34f, 0.34f, 0.34f), skin);
            CreateCylinder(root.transform, "Gold Mantle", new Vector3(0f, 1.28f, 0f), new Vector3(0.42f, 0.035f, 0.42f), trim);
            CreateSphere(root.transform, "Element Gem", new Vector3(0f, 1.18f, -0.38f), new Vector3(0.12f, 0.12f, 0.12f), glow);
            CreateCylinder(root.transform, "Wide Hat", new Vector3(0f, 2.0f, 0f), new Vector3(0.45f, 0.035f, 0.45f), robe);

            ElementbornPrototypeImportedModelAnimator animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = ElementbornPrototypeModelAnimationMode.Idle;
            animator.bobAmplitude = 0.018f;
            animator.boneWiggleDegrees = 1.2f;

            return root;
        }

        private static void RemoveUnsafePlayerVisuals(Transform player)
        {
            for (int i = player.childCount - 1; i >= 0; i--)
            {
                Transform child = player.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                bool keep =
                    child.name.Contains("Prototype Quest Marker") ||
                    child.GetComponentInChildren<Camera>(true) != null;

                bool remove =
                    child.name.Contains("V10") ||
                    child.name.Contains("Player Visual") ||
                    child.name.Contains("Imported Model") ||
                    child.GetComponent<ElementbornPrototypeImportedModelTag>() != null ||
                    child.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null;

                if (remove && !keep)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void RemoveAccessoryChildren(Transform character)
        {
            string[] names =
            {
                "Clean Character Details",
                "Art Direction Accessories"
            };

            for (int i = 0; i < names.Length; i++)
            {
                Transform child = character.Find(names[i]);
                if (child != null)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static bool IsChest(GameObject go, ElementbornPrototypeInteractable interactable)
        {
            if (go == null)
            {
                return false;
            }

            string name = go.name.ToLowerInvariant();
            string display = interactable != null && interactable.displayName != null ? interactable.displayName.ToLowerInvariant() : "";
            return
                name.Contains("chest") ||
                display.Contains("chest") ||
                (interactable != null && interactable.kind == ElementbornPrototypeInteractableKind.LootChest);
        }

        private static void AppendModelReport(StringBuilder report, string label, string folder, string hint)
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
                    report.AppendLine("  Renderer count: " + renderers.Length);
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

            string[] roots =
            {
                "Assets/Elementborn/Art/Models/MeshyImported/AutoImported",
                "Assets/Elementborn/Art/Models/MeshyImported"
            };

            for (int r = 0; r < roots.Length; r++)
            {
                if (!AssetDatabase.IsValidFolder(roots[r]))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { roots[r] });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (NormalizeName(path).Contains(normalizedHint))
                    {
                        return path;
                    }
                }
            }

            return "";
        }

        private static int DisableNonGameplayColliders(Transform root)
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

                if (IsGameplayCollider(collider.gameObject))
                {
                    continue;
                }

                collider.enabled = false;
                count++;
            }

            return count;
        }

        private static void RestoreGameplayColliders()
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
                GameObject go = GameObject.Find(coreNames[i]);
                if (go == null)
                {
                    continue;
                }

                Collider collider = go.GetComponent<Collider>();
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

        private static bool IsGameplayCollider(GameObject go)
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

        private static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            DestroyCollider(go);
            return go;
        }

        private static GameObject CreateSphere(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            DestroyCollider(go);
            return go;
        }

        private static GameObject CreateCapsule(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            DestroyCollider(go);
            return go;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            DestroyCollider(go);
            return go;
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
