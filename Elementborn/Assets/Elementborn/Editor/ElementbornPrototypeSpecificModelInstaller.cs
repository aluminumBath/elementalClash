
#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeSpecificModelInstaller
    {
        private const string ChestFolder = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/TreasureChest_0623200731";
        private const string ChannelerFolder = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported/ChannelerHeroNone_0624153647";
        private const string AxolotlFolder = "Assets/Elementborn/Art/Models/MeshyImported/PinkEyeAxolotl";
        private const string ChestPrefab = "Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved/TreasureChest_0623200731.prefab";
        private const string ChannelerPrefab = "Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved/ChannelerHeroNone_0624153647.prefab";
        private const string AxolotlPrefab = "Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved/AxolotlPlayerFallback.prefab";

        [MenuItem("Elementborn/Assets/V100 Build Exact Chest And Channeler Prefabs")]
        public static void BuildExactChestAndChannelerPrefabs()
        {
            BuildPrefab(ChestFolder, "treasurechest0623200731", ChestPrefab, "TreasureChest_0623200731", 0.9f, ElementbornPrototypeModelAnimationMode.Idle, "Exact chest ZIP: Treasure_Chest_0623200731_texture_fbx.zip");
            BuildPrefab(ChannelerFolder, "channelerheronone0624153647", ChannelerPrefab, "ChannelerHeroNone_0624153647", 1.75f, ElementbornPrototypeModelAnimationMode.Idle, "Exact channeler ZIP: Channeler_Hero_None_3_0624153647_texture_fbx.zip");
            BuildPrefab(AxolotlFolder, "axolotl", AxolotlPrefab, "AxolotlPlayerFallback", 1.35f, ElementbornPrototypeModelAnimationMode.Swim, "Fallback player visual from uploaded axolotl example.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Elementborn/Assets/V100 Apply Channeler Visual To Player")]
        public static void ApplyChannelerVisualToPlayerMenu()
        {
            BuildExactChestAndChannelerPrefabs();
            ApplyPlayerVisual(ChannelerPrefab, "Channeler Player Visual", 1.65f, true);
        }

        [MenuItem("Elementborn/Assets/V100 Apply Axolotl Visual To Player")]
        public static void ApplyAxolotlVisualToPlayerMenu()
        {
            BuildExactChestAndChannelerPrefabs();
            ApplyPlayerVisual(AxolotlPrefab, "Axolotl Player Visual", 1.25f, true);
        }

        [MenuItem("Elementborn/Assets/V100 Apply Exact Chest Visuals")]
        public static void ApplyExactChestVisualsMenu()
        {
            BuildExactChestAndChannelerPrefabs();
            ApplyChestVisuals(true);
        }

        [MenuItem("Elementborn/Assets/V100 Restore Visible Fallback Chests")]
        public static void RestoreVisibleFallbackChestsMenu() { RestoreVisibleFallbackChests(true); }

        [MenuItem("Elementborn/Assets/V100 Restore Blocky Player")]
        public static void RestoreBlockyPlayerMenu() { RestoreBlockyPlayer(true); }

        [MenuItem("Elementborn/Assets/V100 Apply Channeler Player And Chest Visuals")]
        public static void ApplyChannelerPlayerAndChestVisualsMenu()
        {
            BuildExactChestAndChannelerPrefabs();
            RestoreVisibleFallbackChests(false);
            ApplyChestVisuals(false);
            ApplyPlayerVisual(ChannelerPrefab, "Channeler Player Visual", 1.65f, false);
            MarkSceneDirtyAndSave();
        }

        [MenuItem("Elementborn/Assets/V100 Apply Axolotl Player And Chest Visuals")]
        public static void ApplyAxolotlPlayerAndChestVisualsMenu()
        {
            BuildExactChestAndChannelerPrefabs();
            RestoreVisibleFallbackChests(false);
            ApplyChestVisuals(false);
            ApplyPlayerVisual(AxolotlPrefab, "Axolotl Player Visual", 1.25f, false);
            MarkSceneDirtyAndSave();
        }

        public static void ApplyPlayerVisual(string prefabPath, string visualName, float targetHeight, bool save)
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null) { Debug.LogWarning("Could not find Prototype Player."); return; }
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) { Debug.LogWarning("Player visual prefab not found: " + prefabPath); EnsureRootRendererVisible(player); return; }
            RemoveExistingPlayerVisuals(player.transform);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) instance = Object.Instantiate(prefab);
            instance.name = visualName;
            instance.transform.SetParent(player.transform, false);
            instance.transform.localPosition = new Vector3(0f, -1.0f, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            NormalizeInstance(instance.transform, targetHeight, new Vector3(0f, -1.0f, 0f));
            DisableCollidersUnder(instance.transform);
            if (HasVisibleRenderers(instance))
            {
                Renderer r = player.GetComponent<Renderer>();
                if (r != null) r.enabled = false;
                RemoveDecorativeCharacterChildren(player.transform);
                Debug.Log("Applied player visual: " + prefabPath);
            }
            else
            {
                Object.DestroyImmediate(instance);
                EnsureRootRendererVisible(player);
                Debug.LogWarning("Imported player visual had no visible renderers; keeping blocky player visible.");
            }
            if (save) MarkSceneDirtyAndSave();
        }

        public static void ApplyChestVisuals(bool save)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ChestPrefab);
            int imported = 0, fallback = 0;
            ElementbornPrototypeInteractable[] interactables = Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < interactables.Length; i++)
            {
                var it = interactables[i];
                if (it != null && IsChest(it.gameObject, it)) { if (ApplyChestVisual(it.gameObject, prefab)) imported++; else fallback++; }
            }
            if (save) MarkSceneDirtyAndSave();
            Debug.Log("Applied chest visuals. Imported=" + imported + " Fallback=" + fallback);
        }

        public static bool ApplyChestVisual(GameObject chest, GameObject prefab)
        {
            if (chest == null) return false;
            RemoveExistingChestVisuals(chest.transform);
            GameObject replacement = null;
            if (prefab != null)
            {
                replacement = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (replacement == null) replacement = Object.Instantiate(prefab);
                replacement.name = "Exact Treasure Chest Visual";
                replacement.transform.SetParent(chest.transform, false);
                replacement.transform.localPosition = Vector3.zero;
                replacement.transform.localRotation = Quaternion.identity;
                replacement.transform.localScale = Vector3.one;
                NormalizeInstance(replacement.transform, 0.9f, Vector3.zero);
                DisableCollidersUnder(replacement.transform);
                if (!HasVisibleRenderers(replacement)) { Object.DestroyImmediate(replacement); replacement = null; }
            }
            bool importedVisible = replacement != null && HasVisibleRenderers(replacement);
            if (!importedVisible) replacement = CreateFallbackChestVisual(chest.transform);
            bool replacementVisible = replacement != null && HasVisibleRenderers(replacement);
            if (replacementVisible) HideOldChestRenderers(chest.transform, replacement.transform); else EnsureRootRendererVisible(chest);
            ElementbornPrototypeGeneratedAssetSlot slot = chest.GetComponent<ElementbornPrototypeGeneratedAssetSlot>();
            if (slot == null) slot = chest.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            slot.slotName = chest.name;
            slot.preferredPrefabName = importedVisible ? "TreasureChest_0623200731" : "FallbackChest";
            slot.role = "ChestVisual";
            slot.element = "None";
            slot.visualApplied = replacementVisible;
            return importedVisible;
        }

        public static void RestoreVisibleFallbackChests(bool save)
        {
            int restored = 0;
            ElementbornPrototypeInteractable[] interactables = Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < interactables.Length; i++)
            {
                var it = interactables[i];
                if (it == null || !IsChest(it.gameObject, it)) continue;
                if (!HasVisibleRenderers(it.gameObject)) { EnsureRootRendererVisible(it.gameObject); CreateFallbackChestVisual(it.transform); restored++; }
            }
            if (save) MarkSceneDirtyAndSave();
            Debug.Log("Restored visible fallback chests: " + restored);
        }

        public static void RestoreBlockyPlayer(bool save)
        {
            GameObject player = GameObject.Find("Prototype Player");
            if (player == null) return;
            RemoveExistingPlayerVisuals(player.transform);
            EnsureRootRendererVisible(player);
            if (save) MarkSceneDirtyAndSave();
        }

        private static void BuildPrefab(string folder, string hint, string prefabPath, string prefabName, float height, ElementbornPrototypeModelAnimationMode mode, string note)
        {
            EnsureFolder("Assets/Elementborn/Generated/Prefabs/ImportedModels/Approved");
            string fbx = FindFirstFbx(folder, hint);
            if (string.IsNullOrWhiteSpace(fbx)) { Debug.LogWarning("Could not find FBX for " + prefabName + " under " + folder); return; }
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            if (modelAsset == null) { Debug.LogWarning("Could not load FBX: " + fbx); return; }
            GameObject root = new GameObject(prefabName);
            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null) model = Object.Instantiate(modelAsset);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            NormalizeInstance(model.transform, height, Vector3.zero);
            DisableCollidersUnder(model.transform);
            var animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = mode;
            animator.bobAmplitude = mode == ElementbornPrototypeModelAnimationMode.Swim ? 0.045f : 0.015f;
            animator.boneWiggleDegrees = mode == ElementbornPrototypeModelAnimationMode.Swim ? 5.0f : 1.5f;
            var tag = root.AddComponent<ElementbornPrototypeImportedModelTag>();
            tag.sourceAssetPath = fbx;
            tag.modelRole = prefabName;
            tag.notes = note;
            if (!HasVisibleRenderers(root)) { Object.DestroyImmediate(root); Debug.LogWarning("Not saving prefab with no visible renderers: " + fbx); return; }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("Built prefab " + prefabPath + " from " + fbx);
        }

        private static string FindFirstFbx(string folder, string hint)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { folder });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) return path;
                }
            }
            string[] roots = { "Assets/Elementborn/Art/Models/MeshyImported/AutoImported", "Assets/Elementborn/Art/Models/MeshyImported" };
            for (int r = 0; r < roots.Length; r++)
            {
                if (!AssetDatabase.IsValidFolder(roots[r])) continue;
                string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { roots[r] });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) && NormalizeName(path).Contains(hint)) return path;
                }
            }
            return "";
        }

        private static bool IsChest(GameObject go, ElementbornPrototypeInteractable it)
        {
            string name = go != null ? go.name.ToLowerInvariant() : "";
            string display = it != null && it.displayName != null ? it.displayName.ToLowerInvariant() : "";
            return name.Contains("chest") || display.Contains("chest") || (it != null && it.kind == ElementbornPrototypeInteractableKind.LootChest);
        }

        private static void RemoveExistingPlayerVisuals(Transform player)
        {
            for (int i = player.childCount - 1; i >= 0; i--)
            {
                Transform child = player.GetChild(i);
                bool remove = child.name.Contains("Player Visual") || child.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null;
                bool keep = child.name.Contains("Prototype Quest Marker") || child.GetComponentInChildren<Camera>(true) != null;
                if (remove && !keep) Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void RemoveExistingChestVisuals(Transform chest)
        {
            for (int i = chest.childCount - 1; i >= 0; i--)
            {
                Transform child = chest.GetChild(i);
                bool remove = child.name.Contains("Treasure Chest Visual") || child.name.Contains("Fallback Treasure Chest Visual") || child.name.Contains("Exact Treasure Chest Visual") || child.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null;
                bool keep = child.name.Contains("Prototype Interaction Radius") || child.name.Contains("Prototype Quest Marker") || child.GetComponentInChildren<TextMesh>(true) != null;
                if (remove && !keep) Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void HideOldChestRenderers(Transform chest, Transform replacement)
        {
            Renderer rootRenderer = chest.GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;
            for (int i = 0; i < chest.childCount; i++)
            {
                Transform child = chest.GetChild(i);
                if (child == null || child == replacement) continue;
                bool keep = child.name.Contains("Prototype Interaction Radius") || child.name.Contains("Prototype Quest Marker") || child.GetComponentInChildren<TextMesh>(true) != null;
                if (keep) continue;
                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++) if (renderers[r] != null) renderers[r].enabled = false;
                DisableCollidersUnder(child);
            }
        }

        private static void RemoveDecorativeCharacterChildren(Transform player)
        {
            string[] names = { "Clean Character Details", "Art Direction Accessories" };
            for (int i = 0; i < names.Length; i++) { Transform child = player.Find(names[i]); if (child != null) Object.DestroyImmediate(child.gameObject); }
        }

        private static GameObject CreateFallbackChestVisual(Transform parent)
        {
            Transform existing = parent.Find("Fallback Treasure Chest Visual");
            if (existing != null) { SetAllRenderersEnabled(existing.gameObject, true); return existing.gameObject; }
            GameObject root = new GameObject("Fallback Treasure Chest Visual");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            Material wood = CreateMaterial("Fallback Chest Wood", new Color(0.44f, 0.23f, 0.08f));
            Material band = CreateMaterial("Fallback Chest Band", new Color(0.95f, 0.68f, 0.22f));
            CreateBox(root.transform, "Fallback Chest Base", new Vector3(0f, 0.18f, 0f), new Vector3(0.9f, 0.36f, 0.55f), wood);
            CreateBox(root.transform, "Fallback Chest Gold Band", new Vector3(0f, 0.28f, -0.29f), new Vector3(0.12f, 0.42f, 0.04f), band);
            CreateBox(root.transform, "Fallback Chest Lock", new Vector3(0f, 0.18f, -0.32f), new Vector3(0.22f, 0.16f, 0.04f), band);
            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lid.name = "Fallback Chest Rounded Lid";
            lid.transform.SetParent(root.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.43f, 0f);
            lid.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            lid.transform.localScale = new Vector3(0.28f, 0.48f, 0.28f);
            lid.GetComponent<Renderer>().sharedMaterial = wood;
            DestroyCollider(lid);
            return root;
        }

        private static void CreateBox(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            DestroyCollider(go);
        }

        private static void DestroyCollider(GameObject go) { Collider c = go.GetComponent<Collider>(); if (c != null) Object.DestroyImmediate(c); }
        private static bool HasVisibleRenderers(GameObject go) { if (go == null) return false; Renderer[] rs = go.GetComponentsInChildren<Renderer>(true); for (int i=0;i<rs.Length;i++) if (rs[i]!=null && rs[i].enabled) return true; return false; }
        private static void SetAllRenderersEnabled(GameObject go, bool enabled) { Renderer[] rs = go.GetComponentsInChildren<Renderer>(true); for (int i=0;i<rs.Length;i++) if (rs[i]!=null) rs[i].enabled = enabled; }
        private static void EnsureRootRendererVisible(GameObject go) { Renderer r = go != null ? go.GetComponent<Renderer>() : null; if (r != null) r.enabled = true; }
        private static int DisableCollidersUnder(Transform root) { int count=0; if (root==null) return 0; Collider[] cs = root.GetComponentsInChildren<Collider>(true); for (int i=0;i<cs.Length;i++) if (cs[i]!=null) { cs[i].enabled=false; count++; } return count; }

        private static void NormalizeInstance(Transform modelRoot, float targetHeight, Vector3 desiredLocalOffset)
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i=1;i<renderers.Length;i++) if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            float scale = Mathf.Clamp(targetHeight / Mathf.Max(0.001f, bounds.size.y), 0.00005f, 1.25f);
            modelRoot.localScale *= scale;
            renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            for (int i=1;i<renderers.Length;i++) if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            Vector3 worldDesired = modelRoot.parent != null ? modelRoot.parent.TransformPoint(desiredLocalOffset) : desiredLocalOffset;
            Vector3 correction = worldDesired - bounds.center;
            correction.y += bounds.extents.y;
            modelRoot.position += correction;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            Material m = shader != null ? new Material(shader) : new Material(Shader.Find("Hidden/InternalErrorShader"));
            m.name = name;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            m.color = color;
            return m;
        }

        private static string NormalizeName(string value) { if (string.IsNullOrWhiteSpace(value)) return ""; string lower=value.ToLowerInvariant(); StringBuilder b=new StringBuilder(); for(int i=0;i<lower.Length;i++){ char c=lower[i]; if(char.IsLetterOrDigit(c)) b.Append(c);} return b.ToString(); }
        private static void EnsureFolder(string path) { if (AssetDatabase.IsValidFolder(path)) return; string parent = Path.GetDirectoryName(path).Replace("\\", "/"); string name = Path.GetFileName(path); if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent); AssetDatabase.CreateFolder(parent, name); }
        private static void MarkSceneDirtyAndSave() { Scene scene = EditorSceneManager.GetActiveScene(); if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene); AssetDatabase.SaveAssets(); }
    }
}
#endif
