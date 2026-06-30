#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public enum ElementbornGeneratedVisualPreset
    {
        PropsOnly,
        CharactersOnly,
        CreatureShowcaseSmall,
        GameplayReplacementsSmall,
        FullSafeVisualPass
    }

    public static class ElementbornGeneratedAssetSceneDecorator
    {
        public static bool requireApprovedGeneratedAssets = true;
        private const string SafeDecorRootName = "Generated Asset Safe Decoration";
        private const string ShowcaseRootName = "Generated Asset Showcase Gallery";


        [MenuItem("Elementborn/Assets/Toggle Presets Require Approved Assets")]
        public static void ToggleApprovalRequirement()
        {
            requireApprovedGeneratedAssets = !requireApprovedGeneratedAssets;
            Debug.Log("Generated visual presets require approved assets: " + requireApprovedGeneratedAssets);
        }

        [MenuItem("Elementborn/Assets/Clear Generated Asset Decorations In Open Scene")]
        public static void ClearGeneratedAssetDecorationsInOpenScene()
        {
            int destroyed = 0;
            int restored = 0;

            string[] rootNames = new string[]
            {
                SafeDecorRootName,
                ShowcaseRootName,
                "Generated Asset Scene Decoration"
            };

            for (int i = 0; i < rootNames.Length; i++)
            {
                GameObject root = GameObject.Find(rootNames[i]);
                if (root != null)
                {
                    Object.DestroyImmediate(root);
                    destroyed++;
                }
            }

            ElementbornPrototypeVisualPresetTag[] presetTags =
                Object.FindObjectsByType<ElementbornPrototypeVisualPresetTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < presetTags.Length; i++)
            {
                ElementbornPrototypeVisualPresetTag tag = presetTags[i];
                if (tag == null)
                {
                    continue;
                }

                if (tag.transform.parent != null)
                {
                    Renderer parentRenderer = tag.transform.parent.GetComponent<Renderer>();
                    if (parentRenderer != null)
                    {
                        parentRenderer.enabled = true;
                        restored++;
                    }
                }

                Object.DestroyImmediate(tag.gameObject);
                destroyed++;
            }

            ElementbornPrototypeGeneratedAssetSlot[] slots =
                Object.FindObjectsByType<ElementbornPrototypeGeneratedAssetSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < slots.Length; i++)
            {
                ElementbornPrototypeGeneratedAssetSlot slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                Renderer parentRenderer = slot.GetComponent<Renderer>();
                if (parentRenderer != null)
                {
                    parentRenderer.enabled = true;
                    restored++;
                }

                Transform transform = slot.transform;
                for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
                {
                    Transform child = transform.GetChild(childIndex);
                    if (child == null)
                    {
                        continue;
                    }

                    bool generatedChild =
                        child.name.StartsWith("Generated ") ||
                        child.name.StartsWith("Safe ") ||
                        child.name.StartsWith("Assigned ") ||
                        child.name.StartsWith("Preview ") ||
                        child.GetComponentInChildren<ElementbornPrototypeImportedModelTag>(true) != null ||
                        child.GetComponentInChildren<ElementbornPrototypeGeneratedAssetSlot>(true) != null ||
                        child.GetComponentInChildren<ElementbornPrototypeVisualPresetTag>(true) != null;

                    if (generatedChild)
                    {
                        Object.DestroyImmediate(child.gameObject);
                        destroyed++;
                    }
                }

                Object.DestroyImmediate(slot);
            }

            string[] oldShowcaseNameFragments = new string[]
            {
                "Showcase",
                "Gallery",
                "Generated Player Visual",
                "Generated PinkEyeAxolotl Visual",
                "Generated TreasureChest Visual",
                "Generated HealingTonic Visual",
                "Generated StaminaDraught Visual",
                "Generated AncientGlyphMap Visual",
                "Generated Emberblade Visual",
                "Generated StonebreakerHammer Visual",
                "Generated GildedArcBow Visual",
                "Safe Treasure Chest Visual",
                "Safe Side Chest Visual",
                "Safe Healing Tonic Visual",
                "Safe Stamina Draught Visual",
                "Safe Ancient Map Visual",
                "Safe Axolotl Hostile Visual"
            };

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null)
                {
                    continue;
                }

                bool shouldDestroy = false;
                for (int j = 0; j < oldShowcaseNameFragments.Length; j++)
                {
                    if (go.name.Contains(oldShowcaseNameFragments[j]))
                    {
                        shouldDestroy = true;
                        break;
                    }
                }

                if (shouldDestroy)
                {
                    Object.DestroyImmediate(go);
                    destroyed++;
                }
            }

            MarkSceneDirtyAndSave();
            Debug.Log("Generated asset decorations cleared. Destroyed=" + destroyed + " Restored renderers=" + restored);
        }

        [MenuItem("Elementborn/Assets/Decorate Open Prototype Scene With Generated Assets")]
        public static void DecorateOpenScene()
        {
            ApplyPreset(ElementbornGeneratedVisualPreset.FullSafeVisualPass, true);
        }

        [MenuItem("Elementborn/Assets/Decorate Open Prototype Scene With Safe Generated Assets")]
        public static void DecorateOpenSceneSafely()
        {
            ApplyPreset(ElementbornGeneratedVisualPreset.GameplayReplacementsSmall, true);
        }

        [MenuItem("Elementborn/Assets/Create Small Generated Asset Showcase Gallery")]
        public static void CreateSmallShowcaseGallery()
        {
            ApplyPreset(ElementbornGeneratedVisualPreset.CreatureShowcaseSmall, true);
        }

        [MenuItem("Elementborn/Assets/Apply Visual Preset - Props Only")]
        public static void ApplyPropsOnlyMenu()
        {
            ApplyPreset(ElementbornGeneratedVisualPreset.PropsOnly, true);
        }

        [MenuItem("Elementborn/Assets/Apply Visual Preset - Characters Only")]
        public static void ApplyCharactersOnlyMenu()
        {
            ApplyPreset(ElementbornGeneratedVisualPreset.CharactersOnly, true);
        }

        [MenuItem("Elementborn/Assets/Apply Visual Preset - Full Safe Visual Pass")]
        public static void ApplyFullSafeMenu()
        {
            ApplyPreset(ElementbornGeneratedVisualPreset.FullSafeVisualPass, true);
        }

        public static string BuildPresetReport(ElementbornGeneratedVisualPreset preset)
        {
            Placement[] placements = GetPlacementsForPreset(preset);
            int available = 0;
            int missing = 0;
            string report = "Visual preset report: " + preset + "\n";

            for (int i = 0; i < placements.Length; i++)
            {
                Placement placement = placements[i];
                bool hasPrefab = ElementbornGeneratedAssetLibraryBuilder.LoadAutoPrefab(placement.AssetSafeName) != null;
                bool isApproved = !requireApprovedGeneratedAssets || ElementbornGeneratedAssetApprovalDatabase.IsApproved(placement.AssetSafeName);
                bool hasTarget = placement.TargetObjectName == "__showcase__" || GameObject.Find(placement.TargetObjectName) != null;

                if (hasPrefab && hasTarget && isApproved)
                {
                    available++;
                }
                else
                {
                    missing++;
                }

                report += "\n" +
                    placement.AssetSafeName +
                    " -> " +
                    placement.TargetObjectName +
                    " | Prefab=" +
                    hasPrefab +
                    " | Approved=" +
                    isApproved +
                    " | Target=" +
                    hasTarget;
            }

            report += "\n\nAvailable=" + available + " Missing/Skipped=" + missing + " Total=" + placements.Length;
            return report;
        }

        public static void ApplyPreset(ElementbornGeneratedVisualPreset preset, bool saveScene)
        {
            Placement[] placements = GetPlacementsForPreset(preset);
            int applied = 0;
            int skipped = 0;

            EnsureRoot(SafeDecorRootName);

            for (int i = 0; i < placements.Length; i++)
            {
                Placement placement = placements[i];

                if (requireApprovedGeneratedAssets && !ElementbornGeneratedAssetApprovalDatabase.IsApproved(placement.AssetSafeName))
                {
                    skipped++;
                    continue;
                }

                if (placement.TargetObjectName == "__showcase__")
                {
                    bool created = CreateSafeShowcase(
                        placement.GeneratedObjectName,
                        placement.AssetSafeName,
                        placement.WorldPosition,
                        placement.TargetHeight,
                        placement.Label);

                    if (created)
                    {
                        applied++;
                    }
                    else
                    {
                        skipped++;
                    }

                    continue;
                }

                GameObject target = GameObject.Find(placement.TargetObjectName);
                if (target == null)
                {
                    skipped++;
                    continue;
                }

                GameObject attached = AttachVisual(
                    target.transform,
                    placement.AssetSafeName,
                    placement.GeneratedObjectName,
                    placement.LocalOffset,
                    placement.TargetHeight,
                    placement.HideExistingRenderer);

                if (attached != null)
                {
                    ElementbornPrototypeVisualPresetTag tag = attached.GetComponent<ElementbornPrototypeVisualPresetTag>();
                    if (tag == null)
                    {
                        tag = attached.AddComponent<ElementbornPrototypeVisualPresetTag>();
                    }

                    tag.presetName = preset.ToString();
                    tag.assetSafeName = placement.AssetSafeName;
                    tag.targetObjectName = placement.TargetObjectName;
                    tag.hidesOriginalRenderer = placement.HideExistingRenderer;
                    applied++;
                }
                else
                {
                    skipped++;
                }
            }

            if (saveScene)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("Applied generated visual preset " + preset + ". Applied=" + applied + " Skipped=" + skipped);
        }

        private static Placement[] GetPlacementsForPreset(ElementbornGeneratedVisualPreset preset)
        {
            switch (preset)
            {
                case ElementbornGeneratedVisualPreset.PropsOnly:
                    return new Placement[]
                    {
                        Target("Convergence Reward Chest", "TreasureChest", "Preset Treasure Chest Visual", new Vector3(0f, -0.35f, 0f), 0.95f, true, "Treasure Chest"),
                        Target("Side Supply Chest", "TreasureChest", "Preset Side Chest Visual", new Vector3(0f, -0.35f, 0f), 0.9f, true, "Treasure Chest"),
                        Target("Convergence Healing Shrine", "HealingTonic", "Preset Healing Tonic Visual", new Vector3(0f, 0.55f, 0f), 0.65f, false, "Healing Tonic"),
                        Target("Outer Rest Shrine", "StaminaDraught", "Preset Stamina Draught Visual", new Vector3(0f, 0.55f, 0f), 0.65f, false, "Stamina Draught"),
                        Target("Lore Stone of Unity", "AncientGlyphMap", "Preset Ancient Map Visual", new Vector3(0f, 0.1f, -0.35f), 0.8f, false, "Ancient Glyph Map"),
                    };

                case ElementbornGeneratedVisualPreset.CharactersOnly:
                    return new Placement[]
                    {
                        Target("Prototype Player", "ChannelerHeroNone", "Preset Player Visual", new Vector3(0f, -1f, 0f), 1.75f, true, "Player"),
                        Target("Fire Envoy", "ChannelerHeroFire", "Preset Fire Envoy Visual", new Vector3(0f, -1f, 0f), 1.65f, true, "Fire Envoy"),
                        Target("Water Envoy", "ChannelerHeroWater", "Preset Water Envoy Visual", new Vector3(0f, -1f, 0f), 1.65f, true, "Water Envoy"),
                        Target("Earth Envoy", "ChannelerHeroEarth", "Preset Earth Envoy Visual", new Vector3(0f, -1f, 0f), 1.65f, true, "Earth Envoy"),
                        Target("Air Envoy", "ChannelerHeroAir", "Preset Air Envoy Visual", new Vector3(0f, -1f, 0f), 1.65f, true, "Air Envoy"),
                    };

                case ElementbornGeneratedVisualPreset.CreatureShowcaseSmall:
                    return new Placement[]
                    {
                        Showcase("Skyotter Showcase", "Skyotter", new Vector3(13.5f, 0.9f, -8.5f), 0.95f, "Skyotter"),
                        Showcase("Fire Phoenix Showcase", "FirePhoenix", new Vector3(9.5f, 0.9f, 13.5f), 1.0f, "Phoenix"),
                        Showcase("Shadow Wolf Showcase", "ShadowWolf", new Vector3(-13.5f, 0.9f, -4.0f), 0.95f, "Shadow Wolf"),
                        Showcase("Steam Frog Showcase", "SteamFrogTwin", new Vector3(11.5f, 0.9f, -10.5f), 0.75f, "Steam Frog"),
                    };

                case ElementbornGeneratedVisualPreset.GameplayReplacementsSmall:
                    return new Placement[]
                    {
                        Target("Training Hostile", "PinkEyeAxolotl", "Preset Axolotl Hostile Visual", new Vector3(0f, -0.85f, 0f), 1.15f, true, "Axolotl Hostile"),
                        Target("Convergence Reward Chest", "TreasureChest", "Preset Treasure Chest Visual", new Vector3(0f, -0.35f, 0f), 0.95f, true, "Treasure Chest"),
                        Target("Convergence Healing Shrine", "HealingTonic", "Preset Healing Tonic Visual", new Vector3(0f, 0.55f, 0f), 0.65f, false, "Healing Tonic"),
                    };

                default:
                    return Combine(
                        GetPlacementsForPreset(ElementbornGeneratedVisualPreset.PropsOnly),
                        GetPlacementsForPreset(ElementbornGeneratedVisualPreset.CharactersOnly),
                        GetPlacementsForPreset(ElementbornGeneratedVisualPreset.CreatureShowcaseSmall),
                        GetPlacementsForPreset(ElementbornGeneratedVisualPreset.GameplayReplacementsSmall));
            }
        }

        private static Placement[] Combine(params Placement[][] groups)
        {
            int count = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                count += groups[i].Length;
            }

            Placement[] result = new Placement[count];
            int index = 0;
            for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                Placement[] group = groups[groupIndex];
                for (int i = 0; i < group.Length; i++)
                {
                    result[index++] = group[i];
                }
            }

            return result;
        }

        private static Placement Target(string targetName, string assetSafeName, string generatedObjectName, Vector3 localOffset, float targetHeight, bool hideRenderer, string label)
        {
            Placement placement = new Placement();
            placement.TargetObjectName = targetName;
            placement.AssetSafeName = assetSafeName;
            placement.GeneratedObjectName = generatedObjectName;
            placement.LocalOffset = localOffset;
            placement.TargetHeight = targetHeight;
            placement.HideExistingRenderer = hideRenderer;
            placement.Label = label;
            return placement;
        }

        private static Placement Showcase(string name, string assetSafeName, Vector3 worldPosition, float targetHeight, string label)
        {
            Placement placement = new Placement();
            placement.TargetObjectName = "__showcase__";
            placement.AssetSafeName = assetSafeName;
            placement.GeneratedObjectName = name;
            placement.WorldPosition = worldPosition;
            placement.TargetHeight = targetHeight;
            placement.Label = label;
            return placement;
        }

        private static bool CreateSafeShowcase(string name, string prefabSafeName, Vector3 position, float targetHeight, string label)
        {
            if (GameObject.Find(name) != null)
            {
                return false;
            }

            GameObject prefab = ElementbornGeneratedAssetLibraryBuilder.LoadAutoPrefab(prefabSafeName);
            if (prefab == null)
            {
                return false;
            }

            GameObject root = EnsureRoot(ShowcaseRootName);

            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = name;
            pedestal.transform.SetParent(root.transform, true);
            pedestal.transform.position = position;
            pedestal.transform.localScale = new Vector3(0.65f, 0.22f, 0.65f);
            SetMaterial(pedestal, name + " Pedestal", new Color(0.12f, 0.12f, 0.18f));

            GameObject attached = AttachVisual(pedestal.transform, prefabSafeName, "Preset " + prefabSafeName + " Visual", new Vector3(0f, 0.25f, 0f), targetHeight, false);
            if (attached != null)
            {
                ElementbornPrototypeVisualPresetTag tag = attached.GetComponent<ElementbornPrototypeVisualPresetTag>();
                if (tag == null)
                {
                    tag = attached.AddComponent<ElementbornPrototypeVisualPresetTag>();
                }

                tag.presetName = "Showcase";
                tag.assetSafeName = prefabSafeName;
                tag.targetObjectName = name;
            }

            AddLabel(pedestal.transform, label, new Vector3(0f, targetHeight + 0.65f, 0f));
            return attached != null;
        }

        public static GameObject AttachVisual(Transform parent, string prefabSafeName, string childName, Vector3 localOffset, float targetHeight, bool hideExistingRenderer)
        {
            if (parent == null)
            {
                return null;
            }

            Transform existing = parent.Find(childName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject prefab = ElementbornGeneratedAssetLibraryBuilder.LoadAutoPrefab(prefabSafeName);
            if (prefab == null)
            {
                return null;
            }

            if (hideExistingRenderer)
            {
                Renderer renderer = parent.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(prefab);
            }

            instance.name = childName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localOffset;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            NormalizeInstance(instance.transform, Mathf.Clamp(targetHeight, 0.25f, 1.85f), localOffset);

            ElementbornPrototypeGeneratedAssetSlot slot = parent.GetComponent<ElementbornPrototypeGeneratedAssetSlot>();
            if (slot == null)
            {
                slot = parent.gameObject.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            }

            slot.slotName = parent.name;
            slot.preferredPrefabName = prefabSafeName;
            slot.visualApplied = true;
            return instance;
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

        private static GameObject EnsureRoot(string name)
        {
            GameObject root = GameObject.Find(name);
            if (root == null)
            {
                root = new GameObject(name);
            }

            return root;
        }

        private static void AddLabel(Transform parent, string text, Vector3 localPosition)
        {
            GameObject labelGo = new GameObject("Generated Asset Label");
            labelGo.transform.SetParent(parent, false);
            labelGo.transform.localPosition = localPosition;
            labelGo.transform.localScale = Vector3.one * 0.13f;

            TextMesh label = labelGo.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 36;
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

        private struct Placement
        {
            public string TargetObjectName;
            public string AssetSafeName;
            public string GeneratedObjectName;
            public Vector3 LocalOffset;
            public Vector3 WorldPosition;
            public float TargetHeight;
            public bool HideExistingRenderer;
            public string Label;
        }
    }
}
#endif
