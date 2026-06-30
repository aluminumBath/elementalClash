#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeArtDirectionScenePolisher
    {
        private const string RootName = "Elementborn Art Direction Pass";

        [MenuItem("Elementborn/Visuals/Apply Prototype Art Direction Pass")]
        public static void ApplyArtDirectionMenu()
        {
            ApplyArtDirectionToOpenScene(true);
        }

        [MenuItem("Elementborn/Visuals/Clean Prototype Visual Clutter")]
        public static void CleanPrototypeVisualClutterMenu()
        {
            CleanPrototypeVisualClutter();
            MarkSceneDirtyAndSaveAssets();
        }

        public static void ApplyArtDirectionToOpenScene(bool saveAssets)
        {
            CleanPrototypeVisualClutter();
            NormalizeTextAndMarkers();

            GameObject root = GameObject.Find(RootName);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            root = new GameObject(RootName);

            CreateCentralConvergenceLook(root.transform);
            CreateElementalDistrictDressing(root.transform);
            CreatePathAndHubDressing(root.transform);
            CreateNPCSilhouetteHelpers();
            NormalizeGameplayObjectScale();

            if (saveAssets)
            {
                MarkSceneDirtyAndSaveAssets();
            }

            Debug.Log("Elementborn prototype art direction pass applied.");
        }

        public static void CleanPrototypeVisualClutter()
        {
            string[] exactNames =
            {
                "Prototype Instructions",
                "Controls Sign",
                "Imported Meshy Axolotl Showcase",
                "Generated Asset Scene Decoration",
                "Generated Asset Safe Decoration",
                "Generated Asset Showcase Gallery",
                RootName
            };

            for (int i = 0; i < exactNames.Length; i++)
            {
                DestroyIfFound(exactNames[i]);
            }

            string[] noisyNameFragments =
            {
                "Vista Board",
                "Roster Board",
                "Icon Board",
                "Reward Board",
                "Reference Board",
                "Design Board",
                "Style Board",
                "Boss Icon Board",
                "Asset Sheet",
                "Gallery",
                "Small Showcase",
                "Hub Market Stall",
                "Unifier Banner",
                "Dominion Banner",
                "Small Central Arch",
                "Fire Spire",
                "Water Marker",
                "Earth Monolith",
                "Air Obelisk"
            };

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null)
                {
                    continue;
                }

                for (int j = 0; j < noisyNameFragments.Length; j++)
                {
                    if (go.name.Contains(noisyNameFragments[j]))
                    {
                        Object.DestroyImmediate(go);
                        break;
                    }
                }
            }

            ElementbornGeneratedAssetSceneDecorator.ClearGeneratedAssetDecorationsInOpenScene();
        }

        private static void NormalizeTextAndMarkers()
        {
            TextMesh[] textMeshes = Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < textMeshes.Length; i++)
            {
                TextMesh textMesh = textMeshes[i];
                if (textMesh == null)
                {
                    continue;
                }

                Transform t = textMesh.transform;
                string text = textMesh.text ?? "";

                if (text == "ART")
                {
                    textMesh.text = "✦";
                }

                bool isMarker = t.name.Contains("Marker") || (t.parent != null && t.parent.name.Contains("Quest Marker"));
                if (isMarker)
                {
                    textMesh.fontSize = 42;
                    textMesh.characterSize = 0.18f;
                    t.localScale = Vector3.one * 0.1f;
                    continue;
                }

                bool isLongLabel = text.Length > 22 || text.Contains("\n");
                textMesh.fontSize = isLongLabel ? 32 : 38;
                textMesh.characterSize = isLongLabel ? 0.18f : 0.22f;

                if (t.localScale.magnitude > 0.7f)
                {
                    t.localScale = Vector3.one * 0.12f;
                }

                if (t.localScale.x < 0f || t.localScale.y < 0f || t.localScale.z < 0f)
                {
                    t.localScale = new Vector3(Mathf.Abs(t.localScale.x), Mathf.Abs(t.localScale.y), Mathf.Abs(t.localScale.z));
                }
            }

            ElementbornPrototypeQuestMarker[] markers = Object.FindObjectsByType<ElementbornPrototypeQuestMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < markers.Length; i++)
            {
                ElementbornPrototypeQuestMarker marker = markers[i];
                if (marker == null)
                {
                    continue;
                }

                marker.bobAmplitude = 0.06f;
                if (marker.markerText == "ART")
                {
                    marker.markerText = "✦";
                }

                EditorUtility.SetDirty(marker);
            }
        }

        private static void CreateCentralConvergenceLook(Transform root)
        {
            GameObject center = GameObject.Find("Central Convergence Platform");
            if (center != null)
            {
                Renderer renderer = center.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = CreateMaterial("Convergence Warm Violet", new Color(0.34f, 0.24f, 0.48f));
                }
            }

            GameObject group = CreateGroup(root, "Central City Silhouette");

            CreateCylinder(group.transform, "Convergence Outer Ring", new Vector3(0f, 0.18f, 0f), new Vector3(5.6f, 0.08f, 5.6f), new Color(0.18f, 0.15f, 0.24f));
            CreateCylinder(group.transform, "Convergence Inner Ring", new Vector3(0f, 0.25f, 0f), new Vector3(3.6f, 0.06f, 3.6f), new Color(0.42f, 0.34f, 0.6f));

            CreateColumn(group.transform, "North Lantern Pillar", new Vector3(0f, 1.1f, 4.0f), new Color(0.42f, 0.38f, 0.55f), new Color(1f, 0.85f, 0.35f));
            CreateColumn(group.transform, "South Lantern Pillar", new Vector3(0f, 1.1f, -4.0f), new Color(0.42f, 0.38f, 0.55f), new Color(0.55f, 0.85f, 1f));
            CreateColumn(group.transform, "East Lantern Pillar", new Vector3(4.0f, 1.1f, 0f), new Color(0.42f, 0.38f, 0.55f), new Color(1f, 0.35f, 0.25f));
            CreateColumn(group.transform, "West Lantern Pillar", new Vector3(-4.0f, 1.1f, 0f), new Color(0.42f, 0.38f, 0.55f), new Color(0.35f, 1f, 0.65f));

            CreateBanner(group.transform, "Unity Banner", new Vector3(-1.45f, 1.3f, 3.25f), new Color(0.16f, 0.75f, 0.45f));
            CreateBanner(group.transform, "Dominion Banner", new Vector3(1.45f, 1.3f, 3.25f), new Color(0.78f, 0.16f, 0.16f));
            CreateArch(group.transform, "Convergence Soft Arch", new Vector3(0f, 1.15f, 3.5f), 2.6f, 2.25f, new Color(0.28f, 0.24f, 0.34f));
        }

        private static void CreateElementalDistrictDressing(Transform root)
        {
            GameObject fire = CreateGroup(root, "Fire District Dressing");
            CreateLavaPool(fire.transform, new Vector3(10.8f, 0.12f, 10.2f));
            CreateBasaltCluster(fire.transform, new Vector3(12.5f, 0.25f, 7.8f));
            CreateCrystalCluster(fire.transform, "Fire Ember Crystals", new Vector3(7.5f, 0.35f, 10.8f), new Color(1f, 0.28f, 0.08f));

            GameObject water = CreateGroup(root, "Water District Dressing");
            CreatePond(water.transform, new Vector3(-10.2f, 0.1f, 9.2f));
            CreateReeds(water.transform, new Vector3(-12.4f, 0.25f, 7.6f));
            CreateCrystalCluster(water.transform, "Water Prism Crystals", new Vector3(-7.8f, 0.35f, 10.8f), new Color(0.18f, 0.7f, 1f));

            GameObject earth = CreateGroup(root, "Earth District Dressing");
            CreateTreeCluster(earth.transform, new Vector3(-11.2f, 0.0f, -9.2f));
            CreateRockCluster(earth.transform, new Vector3(-7.8f, 0.25f, -11.0f));
            CreateCrystalCluster(earth.transform, "Earth Moss Crystals", new Vector3(-12.8f, 0.35f, -6.6f), new Color(0.28f, 0.85f, 0.28f));

            GameObject air = CreateGroup(root, "Air District Dressing");
            CreateWindRings(air.transform, new Vector3(10.6f, 1.4f, -9.6f));
            CreateCloudPuffs(air.transform, new Vector3(12.4f, 1.4f, -6.6f));
            CreateCrystalCluster(air.transform, "Air Sky Crystals", new Vector3(7.8f, 0.35f, -11.0f), new Color(0.78f, 0.95f, 1f));
        }

        private static void CreatePathAndHubDressing(Transform root)
        {
            GameObject group = CreateGroup(root, "Readable Hub Dressing");
            CreateRoundedStall(group.transform, "Green Market Stall", new Vector3(-5.4f, 0.45f, 0.6f), new Color(0.12f, 0.6f, 0.36f));
            CreateRoundedStall(group.transform, "Blue Market Stall", new Vector3(5.4f, 0.45f, 0.6f), new Color(0.1f, 0.28f, 0.72f));

            CreateSmallShrineFrame(group.transform, "Healing Shrine Frame", new Vector3(-3.2f, 0.0f, 1.6f), new Color(0.35f, 1f, 0.75f));
            CreateSmallShrineFrame(group.transform, "Outer Rest Shrine Frame", new Vector3(10f, 0.0f, -2f), new Color(0.68f, 0.9f, 1f));

            CreateChestPedestal(group.transform, "Reward Chest Pedestal", new Vector3(3.2f, 0.1f, 1.6f), new Color(0.78f, 0.48f, 0.15f));
            CreateChestPedestal(group.transform, "Side Chest Pedestal", new Vector3(-9f, 0.1f, 2.5f), new Color(0.72f, 0.42f, 0.14f));
        }

        private static void CreateNPCSilhouetteHelpers()
        {
            AddNPCAccessories("Ember Guide", new Color(0.85f, 0.22f, 0.08f), true);
            AddNPCAccessories("Fire Envoy", new Color(1f, 0.28f, 0.12f), false);
            AddNPCAccessories("Water Envoy", new Color(0.2f, 0.55f, 1f), false);
            AddNPCAccessories("Earth Envoy", new Color(0.25f, 0.72f, 0.25f), false);
            AddNPCAccessories("Air Envoy", new Color(0.75f, 0.95f, 1f), false);
            AddHostileDetails();
        }

        private static void AddNPCAccessories(string npcName, Color accent, bool guide)
        {
            GameObject npc = GameObject.Find(npcName);
            if (npc == null || npc.transform.Find("Art Direction Accessories") != null)
            {
                return;
            }

            GameObject root = new GameObject("Art Direction Accessories");
            root.transform.SetParent(npc.transform, false);

            CreateCylinder(root.transform, "Shoulder Mantle", new Vector3(0f, 0.7f, 0f), new Vector3(0.82f, 0.05f, 0.82f), accent);
            CreateSphere(root.transform, "Attunement Gem", new Vector3(0f, 0.95f, -0.36f), new Vector3(0.16f, 0.16f, 0.16f), accent);
            CreateCylinder(root.transform, "Soft Hat Brim", new Vector3(0f, 1.52f, 0f), new Vector3(0.46f, 0.04f, 0.46f), new Color(0.12f, 0.1f, 0.12f));

            if (guide)
            {
                CreateCone(root.transform, "Guide Flame Hat", new Vector3(0f, 1.82f, 0f), 0.22f, 0.55f, new Color(1f, 0.35f, 0.08f));
            }
        }

        private static void AddHostileDetails()
        {
            GameObject hostile = GameObject.Find("Training Hostile");
            if (hostile == null || hostile.transform.Find("Art Direction Hostile Details") != null)
            {
                return;
            }

            GameObject root = new GameObject("Art Direction Hostile Details");
            root.transform.SetParent(hostile.transform, false);

            CreateSphere(root.transform, "Hostile Eye Left", new Vector3(-0.16f, 1.25f, -0.42f), new Vector3(0.11f, 0.11f, 0.11f), new Color(1f, 0.95f, 0.55f));
            CreateSphere(root.transform, "Hostile Eye Right", new Vector3(0.16f, 1.25f, -0.42f), new Vector3(0.11f, 0.11f, 0.11f), new Color(1f, 0.95f, 0.55f));
            CreateCone(root.transform, "Hostile Back Spike A", new Vector3(0f, 1.18f, 0.35f), 0.12f, 0.45f, new Color(0.08f, 0.05f, 0.08f));
            CreateCone(root.transform, "Hostile Back Spike B", new Vector3(0f, 0.82f, 0.42f), 0.12f, 0.38f, new Color(0.08f, 0.05f, 0.08f));
        }

        private static void NormalizeGameplayObjectScale()
        {
            // Keep large readable gates, but avoid huge debug-like signs/markers dominating the view.
            ElementbornPrototypeQuestMarker[] markers = Object.FindObjectsByType<ElementbornPrototypeQuestMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < markers.Length; i++)
            {
                if (markers[i] != null)
                {
                    markers[i].localOffset = new Vector3(0f, Mathf.Min(markers[i].localOffset.y, 3.1f), 0f);
                    markers[i].bobAmplitude = 0.06f;
                    EditorUtility.SetDirty(markers[i]);
                }
            }
        }

        private static void CreateLavaPool(Transform parent, Vector3 position)
        {
            CreateCylinder(parent, "Lava Pool Rim", position, new Vector3(2.0f, 0.08f, 1.4f), new Color(0.08f, 0.05f, 0.05f));
            CreateCylinder(parent, "Lava Glow", position + Vector3.up * 0.08f, new Vector3(1.65f, 0.04f, 1.05f), new Color(1f, 0.32f, 0.05f));
        }

        private static void CreatePond(Transform parent, Vector3 position)
        {
            CreateCylinder(parent, "Water Pool Rim", position, new Vector3(2.3f, 0.08f, 1.6f), new Color(0.08f, 0.12f, 0.18f));
            CreateCylinder(parent, "Water Pool Surface", position + Vector3.up * 0.08f, new Vector3(2.0f, 0.04f, 1.3f), new Color(0.1f, 0.55f, 0.95f));
        }

        private static void CreateBasaltCluster(Transform parent, Vector3 position)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.35f;
                float z = Mathf.Sin(i * 1.7f) * 0.35f;
                float h = 0.8f + i * 0.22f;
                CreateCylinder(parent, "Basalt Spire " + i, position + new Vector3(x, h * 0.5f, z), new Vector3(0.24f, h, 0.24f), new Color(0.08f, 0.07f, 0.08f));
            }
        }

        private static void CreateRockCluster(Transform parent, Vector3 position)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = new Vector3(Mathf.Cos(i) * 0.55f, 0.22f, Mathf.Sin(i * 1.6f) * 0.45f);
                CreateSphere(parent, "Rounded Boulder " + i, position + offset, new Vector3(0.55f, 0.38f, 0.5f), new Color(0.19f, 0.18f, 0.15f));
            }
        }

        private static void CreateReeds(Transform parent, Vector3 position)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3(Mathf.Cos(i * 0.9f) * 0.65f, 0.4f, Mathf.Sin(i * 1.2f) * 0.55f);
                CreateCylinder(parent, "Water Reed " + i, position + offset, new Vector3(0.035f, 0.8f, 0.035f), new Color(0.18f, 0.55f, 0.32f));
                CreateSphere(parent, "Water Reed Tip " + i, position + offset + Vector3.up * 0.45f, new Vector3(0.08f, 0.08f, 0.08f), new Color(0.65f, 0.45f, 0.22f));
            }
        }

        private static void CreateTreeCluster(Transform parent, Vector3 position)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = position + new Vector3((i - 1) * 0.8f, 0f, Mathf.Sin(i) * 0.5f);
                CreateCylinder(parent, "Soft Tree Trunk " + i, p + Vector3.up * 0.6f, new Vector3(0.18f, 1.2f, 0.18f), new Color(0.28f, 0.15f, 0.08f));
                CreateSphere(parent, "Soft Tree Crown " + i, p + Vector3.up * 1.45f, new Vector3(0.85f, 0.75f, 0.85f), new Color(0.13f, 0.55f, 0.18f));
            }
        }

        private static void CreateCloudPuffs(Transform parent, Vector3 position)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = new Vector3(Mathf.Cos(i) * 0.7f, Mathf.Sin(i * 0.8f) * 0.18f, Mathf.Sin(i) * 0.35f);
                CreateSphere(parent, "Cloud Puff " + i, position + offset, new Vector3(0.65f, 0.35f, 0.45f), new Color(0.86f, 0.96f, 1f));
            }
        }

        private static void CreateWindRings(Transform parent, Vector3 position)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject ring = CreateTorusLikeRing(parent, "Wind Ring " + i, position + Vector3.up * (i * 0.35f), 0.8f + i * 0.25f, 0.035f, new Color(0.74f, 0.95f, 1f));
                ring.transform.rotation = Quaternion.Euler(18f + i * 16f, i * 40f, 0f);
            }
        }

        private static void CreateCrystalCluster(Transform parent, string name, Vector3 position, Color color)
        {
            GameObject group = CreateGroup(parent, name);
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3((i - 2) * 0.28f, 0f, Mathf.Sin(i * 1.3f) * 0.22f);
                float height = 0.55f + i * 0.12f;
                CreateCone(group.transform, "Crystal " + i, position + offset + Vector3.up * (height * 0.5f), 0.18f, height, color);
            }
        }

        private static void CreateRoundedStall(Transform parent, string name, Vector3 position, Color canopy)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Rounded Stall Base", position + Vector3.up * 0.25f, new Vector3(0.9f, 0.5f, 0.55f), new Color(0.35f, 0.18f, 0.08f));
            CreateCylinder(group.transform, "Rounded Stall Canopy", position + Vector3.up * 0.85f, new Vector3(1.15f, 0.14f, 0.75f), canopy);
            CreateSphere(group.transform, "Canopy Left Cap", position + new Vector3(-0.55f, 0.85f, 0f), new Vector3(0.2f, 0.14f, 0.3f), canopy);
            CreateSphere(group.transform, "Canopy Right Cap", position + new Vector3(0.55f, 0.85f, 0f), new Vector3(0.2f, 0.14f, 0.3f), canopy);
        }

        private static void CreateSmallShrineFrame(Transform parent, string name, Vector3 position, Color accent)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Shrine Base Ring", position + Vector3.up * 0.12f, new Vector3(1.25f, 0.08f, 1.25f), new Color(0.12f, 0.12f, 0.16f));
            CreateColumn(group.transform, "Shrine Left Post", position + new Vector3(-0.55f, 0.65f, 0f), new Color(0.18f, 0.16f, 0.22f), accent);
            CreateColumn(group.transform, "Shrine Right Post", position + new Vector3(0.55f, 0.65f, 0f), new Color(0.18f, 0.16f, 0.22f), accent);
        }

        private static void CreateChestPedestal(Transform parent, string name, Vector3 position, Color accent)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Chest Base", position + Vector3.up * 0.08f, new Vector3(1.25f, 0.1f, 0.9f), new Color(0.1f, 0.08f, 0.06f));
            CreateSphere(group.transform, "Chest Glow", position + Vector3.up * 0.38f, new Vector3(0.28f, 0.16f, 0.28f), accent);
        }

        private static void CreateColumn(Transform parent, string name, Vector3 position, Color baseColor, Color lightColor)
        {
            CreateCylinder(parent, name + " Shaft", position, new Vector3(0.14f, 1.5f, 0.14f), baseColor);
            CreateSphere(parent, name + " Light", position + Vector3.up * 0.85f, new Vector3(0.22f, 0.22f, 0.22f), lightColor);
        }

        private static void CreateBanner(Transform parent, string name, Vector3 position, Color clothColor)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Banner Pole", position + new Vector3(-0.35f, 0.05f, 0f), new Vector3(0.05f, 1.8f, 0.05f), new Color(0.1f, 0.08f, 0.06f));
            CreatePlaneLikeQuad(group.transform, "Banner Cloth", position + new Vector3(0.1f, 0.4f, 0f), new Vector2(0.75f, 1.05f), clothColor);
        }

        private static void CreateArch(Transform parent, string name, Vector3 position, float width, float height, Color color)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Left Arch Post", position + new Vector3(-width * 0.5f, 0f, 0f), new Vector3(0.12f, height, 0.12f), color);
            CreateCylinder(group.transform, "Right Arch Post", position + new Vector3(width * 0.5f, 0f, 0f), new Vector3(0.12f, height, 0.12f), color);
            CreateTorusLikeRing(group.transform, "Arch Curve", position + Vector3.up * (height * 0.5f), width * 0.5f, 0.06f, color);
        }

        private static GameObject CreateGroup(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, true);
            group.transform.localScale = Vector3.one;
            return group;
        }

        private static void DestroyIfFound(string objectName)
        {
            GameObject go = GameObject.Find(objectName);
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void MarkSceneDirtyAndSaveAssets()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            SetMaterial(go, name + " Material", color);
            return go;
        }

        private static GameObject CreateSphere(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            SetMaterial(go, name + " Material", color);
            return go;
        }

        private static GameObject CreateCone(Transform parent, string name, Vector3 position, float radius, float height, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = position;

            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = BuildConeMesh(radius, height, 16);
            meshRenderer.sharedMaterial = CreateMaterial(name + " Material", color);
            return go;
        }

        private static GameObject CreatePlaneLikeQuad(Transform parent, string name, Vector3 position, Vector2 size, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            SetMaterial(go, name + " Material", color);
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
            return go;
        }

        private static GameObject CreateTorusLikeRing(Transform parent, string name, Vector3 position, float radius, float tubeRadius, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = position;

            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = BuildTorusMesh(radius, tubeRadius, 36, 8);
            meshRenderer.sharedMaterial = CreateMaterial(name + " Material", color);
            return go;
        }

        private static Mesh BuildConeMesh(float radius, float height, int segments)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 6];

            vertices[0] = Vector3.up * (height * 0.5f);
            vertices[1] = Vector3.down * (height * 0.5f);

            for (int i = 0; i < segments; i++)
            {
                float angle = (Mathf.PI * 2f * i) / segments;
                vertices[i + 2] = new Vector3(Mathf.Cos(angle) * radius, -height * 0.5f, Mathf.Sin(angle) * radius);
            }

            int tri = 0;
            for (int i = 0; i < segments; i++)
            {
                int current = i + 2;
                int next = ((i + 1) % segments) + 2;

                triangles[tri++] = 0;
                triangles[tri++] = current;
                triangles[tri++] = next;

                triangles[tri++] = 1;
                triangles[tri++] = next;
                triangles[tri++] = current;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildTorusMesh(float radius, float tubeRadius, int radialSegments, int tubeSegments)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[radialSegments * tubeSegments];
            int[] triangles = new int[radialSegments * tubeSegments * 6];

            for (int r = 0; r < radialSegments; r++)
            {
                float theta = 2f * Mathf.PI * r / radialSegments;
                Vector3 center = new Vector3(Mathf.Cos(theta) * radius, 0f, Mathf.Sin(theta) * radius);
                Vector3 radial = center.normalized;
                Vector3 binormal = Vector3.up;

                for (int t = 0; t < tubeSegments; t++)
                {
                    float phi = 2f * Mathf.PI * t / tubeSegments;
                    Vector3 offset = radial * (Mathf.Cos(phi) * tubeRadius) + binormal * (Mathf.Sin(phi) * tubeRadius);
                    vertices[r * tubeSegments + t] = center + offset;
                }
            }

            int tri = 0;
            for (int r = 0; r < radialSegments; r++)
            {
                int nextR = (r + 1) % radialSegments;
                for (int t = 0; t < tubeSegments; t++)
                {
                    int nextT = (t + 1) % tubeSegments;

                    int a = r * tubeSegments + t;
                    int b = nextR * tubeSegments + t;
                    int c = nextR * tubeSegments + nextT;
                    int d = r * tubeSegments + nextT;

                    triangles[tri++] = a;
                    triangles[tri++] = b;
                    triangles[tri++] = c;

                    triangles[tri++] = a;
                    triangles[tri++] = c;
                    triangles[tri++] = d;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void SetMaterial(GameObject go, string materialName, Color color)
        {
            Renderer renderer = go != null ? go.GetComponent<Renderer>() : null;
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = CreateMaterial(materialName, color);
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
    }
}
#endif
