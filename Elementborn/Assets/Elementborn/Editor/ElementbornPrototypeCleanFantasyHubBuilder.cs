#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeCleanFantasyHubBuilder
    {
        private const string RootName = "Elementborn Clean Fantasy Hub";

        [MenuItem("Elementborn/Visuals/Rebuild Clean Fantasy Hub Look")]
        public static void RebuildCleanFantasyHubLookMenu()
        {
            RebuildCleanFantasyHubLook(true);
        }

        [MenuItem("Elementborn/Visuals/Remove Debug Text And Preview Objects")]
        public static void RemoveDebugTextAndPreviewObjectsMenu()
        {
            RemoveDebugTextAndPreviewObjects();
            MarkSceneDirtyAndSave();
        }

        public static void RebuildCleanFantasyHubLook(bool save)
        {
            RemoveDebugTextAndPreviewObjects();
            RemoveOldVisualRoots();
            RestyleBaseGeometry();
            RestyleGameplayActors();

            GameObject root = GameObject.Find(RootName);
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }

            root = new GameObject(RootName);

            BuildTerrainAndPlaza(root.transform);
            BuildElementalLandmarks(root.transform);
            BuildVillageDressing(root.transform);
            BuildAtmosphere(root.transform);

            ElementbornPrototypeReachabilityAndChestVisualFixer.RepairReachability(false);
            ElementbornPrototypeSpecificModelInstaller.RestoreVisibleFallbackChests(false);

            if (save)
            {
                MarkSceneDirtyAndSave();
            }

            Debug.Log("Rebuilt clean fantasy hub look. Generated FBX models are hidden/opt-in; this pass focuses on a readable non-debug scene.");
        }

        public static void RemoveDebugTextAndPreviewObjects()
        {
            // Remove generated / preview clutter first.
            SafeCallClearGeneratedDecorations();

            string[] exactNames =
            {
                "Generated Asset Review Gallery",
                "Generated Asset Showcase Gallery",
                "Generated Asset Safe Decoration",
                "Generated Asset Scene Decoration",
                "Prototype Instructions",
                "Controls Sign",
                "Elementborn Art Direction Pass",
                "Elementborn Clean Fantasy Hub"
            };

            for (int i = 0; i < exactNames.Length; i++)
            {
                DestroyIfFound(exactNames[i]);
            }

            string[] fragments =
            {
                "Preview ",
                "Review ",
                "Showcase",
                "Gallery",
                "Vista Board",
                "Roster Board",
                "Icon Board",
                "Reward Board",
                "Reference Board",
                "Design Board",
                "Style Board",
                "Boss Icon Board",
                "Asset Sheet",
                "Generated ",
                "Safe ",
                "Assigned "
            };

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject go = allObjects[i];
                if (go == null)
                {
                    continue;
                }

                for (int f = 0; f < fragments.Length; f++)
                {
                    if (go.name.Contains(fragments[f]))
                    {
                        Object.DestroyImmediate(go);
                        break;
                    }
                }
            }

            // Remove/disable almost all world TextMesh debug labels. Keep small quest markers only.
            TextMesh[] labels = Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < labels.Length; i++)
            {
                TextMesh label = labels[i];
                if (label == null)
                {
                    continue;
                }

                bool isMarker =
                    label.transform.name.Contains("Marker") ||
                    (label.transform.parent != null && label.transform.parent.name.Contains("Quest Marker"));

                if (isMarker)
                {
                    label.fontSize = 28;
                    label.characterSize = 0.12f;
                    label.transform.localScale = Vector3.one * 0.08f;
                    continue;
                }

                Object.DestroyImmediate(label.gameObject);
            }
        }

        private static void SafeCallClearGeneratedDecorations()
        {
            try
            {
                ElementbornGeneratedAssetSceneDecorator.ClearGeneratedAssetDecorationsInOpenScene();
            }
            catch
            {
                // Keep this cleanup safe even if generated-asset scripts are temporarily broken.
            }
        }

        private static void RemoveOldVisualRoots()
        {
            string[] oldRoots =
            {
                "Elementborn Art Direction Pass",
                "Fire District Dressing",
                "Water District Dressing",
                "Earth District Dressing",
                "Air District Dressing",
                "Readable Hub Dressing",
                "Central City Silhouette"
            };

            for (int i = 0; i < oldRoots.Length; i++)
            {
                DestroyIfFound(oldRoots[i]);
            }
        }

        private static void RestyleBaseGeometry()
        {
            // Hide/remove giant road text by recoloring roads and shrinking their dominance.
            RestyleNamedPrimitive("North Road", new Color(0.26f, 0.22f, 0.18f), new Vector3(2.6f, 0.08f, 6.0f));
            RestyleNamedPrimitive("South Road", new Color(0.26f, 0.22f, 0.18f), new Vector3(2.6f, 0.08f, 6.0f));
            RestyleNamedPrimitive("East Road", new Color(0.26f, 0.22f, 0.18f), new Vector3(6.0f, 0.08f, 2.6f));
            RestyleNamedPrimitive("West Road", new Color(0.26f, 0.22f, 0.18f), new Vector3(6.0f, 0.08f, 2.6f));

            RestyleNamedPrimitive("Elementborn Test Arena", new Color(0.21f, 0.35f, 0.28f), new Vector3(18f, 0.08f, 18f));

            // Original colored quadrant floor pieces are useful for orientation, but they should not look like flat debug mats.
            SoftenColoredFloor("Fire Quarter", new Color(0.38f, 0.16f, 0.12f));
            SoftenColoredFloor("Water Quarter", new Color(0.14f, 0.31f, 0.42f));
            SoftenColoredFloor("Earth Quarter", new Color(0.16f, 0.34f, 0.18f));
            SoftenColoredFloor("Air Quarter", new Color(0.34f, 0.38f, 0.40f));
            SoftenColoredFloor("Fire Ground", new Color(0.38f, 0.16f, 0.12f));
            SoftenColoredFloor("Water Ground", new Color(0.14f, 0.31f, 0.42f));
            SoftenColoredFloor("Earth Ground", new Color(0.16f, 0.34f, 0.18f));
            SoftenColoredFloor("Air Ground", new Color(0.34f, 0.38f, 0.40f));

            GameObject center = GameObject.Find("Central Convergence Platform");
            if (center != null)
            {
                center.transform.localScale = new Vector3(3.4f, 0.25f, 3.4f);
                SetMaterial(center, "Clean Convergence Material", new Color(0.35f, 0.28f, 0.52f));
            }

            // Make element gates less harsh/blocky.
            RestyleGate("Fire Gate", new Color(0.95f, 0.25f, 0.08f));
            RestyleGate("Water Gate", new Color(0.1f, 0.55f, 0.95f));
            RestyleGate("Earth Gate", new Color(0.35f, 0.72f, 0.28f));
            RestyleGate("Air Gate", new Color(0.76f, 0.94f, 1f));
        }

        private static void RestyleGameplayActors()
        {
            RestyleCharacter("Prototype Player", new Color(0.08f, 0.16f, 0.42f), new Color(0.95f, 0.72f, 0.50f));
            RestyleCharacter("Ember Guide", new Color(0.78f, 0.22f, 0.08f), new Color(1.0f, 0.65f, 0.30f));
            RestyleCharacter("Fire Envoy", new Color(0.85f, 0.18f, 0.08f), new Color(1.0f, 0.75f, 0.45f));
            RestyleCharacter("Water Envoy", new Color(0.1f, 0.38f, 0.82f), new Color(0.70f, 0.90f, 1.0f));
            RestyleCharacter("Earth Envoy", new Color(0.16f, 0.52f, 0.22f), new Color(0.62f, 0.88f, 0.52f));
            RestyleCharacter("Air Envoy", new Color(0.50f, 0.82f, 0.96f), new Color(0.92f, 0.98f, 1.0f));

            GameObject hostile = GameObject.Find("Training Hostile");
            if (hostile != null)
            {
                SetMaterial(hostile, "Clean Hostile Body", new Color(0.22f, 0.08f, 0.12f));
                hostile.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
                AddHostileSilhouette(hostile.transform);
            }

            GameObject dummy = GameObject.Find("Training Dummy");
            if (dummy != null)
            {
                SetMaterial(dummy, "Clean Dummy Material", new Color(0.48f, 0.28f, 0.14f));
            }
        }

        private static void BuildTerrainAndPlaza(Transform root)
        {
            GameObject terrain = CreateGroup(root, "Painterly Terrain Shapes");

            CreateEllipse(terrain.transform, "Earth Grass Shape", new Vector3(-8f, 0.02f, -7f), new Vector3(7.5f, 0.04f, 5.8f), new Color(0.12f, 0.42f, 0.17f));
            CreateEllipse(terrain.transform, "Water Soft Shore", new Vector3(-8f, 0.025f, 7f), new Vector3(7.8f, 0.04f, 5.8f), new Color(0.12f, 0.40f, 0.56f));
            CreateEllipse(terrain.transform, "Fire Ash Ground", new Vector3(8f, 0.025f, 7f), new Vector3(7.4f, 0.04f, 5.8f), new Color(0.45f, 0.18f, 0.12f));
            CreateEllipse(terrain.transform, "Air Pale Stone", new Vector3(8f, 0.025f, -7f), new Vector3(7.4f, 0.04f, 5.8f), new Color(0.50f, 0.58f, 0.60f));

            CreateEllipse(terrain.transform, "Central Plaza Stone", new Vector3(0f, 0.065f, 0f), new Vector3(5.8f, 0.05f, 5.8f), new Color(0.27f, 0.25f, 0.30f));
            CreateRing(terrain.transform, "Central Gold Inlay", new Vector3(0f, 0.12f, 0f), 2.4f, 0.035f, new Color(0.95f, 0.64f, 0.22f));

            CreatePebblePath(terrain.transform, new Vector3(0f, 0.12f, -5.2f), Vector3.forward, 7);
            CreatePebblePath(terrain.transform, new Vector3(0f, 0.12f, 5.2f), Vector3.back, 7);
            CreatePebblePath(terrain.transform, new Vector3(-5.2f, 0.12f, 0f), Vector3.right, 7);
            CreatePebblePath(terrain.transform, new Vector3(5.2f, 0.12f, 0f), Vector3.left, 7);
        }

        private static void BuildElementalLandmarks(Transform root)
        {
            GameObject fire = CreateGroup(root, "Fire Landmark");
            CreateLavaBrazier(fire.transform, new Vector3(8.5f, 0.1f, 8.0f));
            CreateCrystalCluster(fire.transform, new Vector3(11.0f, 0.05f, 6.5f), new Color(1.0f, 0.28f, 0.08f), "Fire Crystal");

            GameObject water = CreateGroup(root, "Water Landmark");
            CreatePool(water.transform, new Vector3(-8.5f, 0.1f, 8.0f));
            CreateReedCluster(water.transform, new Vector3(-11.0f, 0.1f, 6.5f));

            GameObject earth = CreateGroup(root, "Earth Landmark");
            CreateTree(earth.transform, new Vector3(-9.5f, 0.05f, -8.0f), 1.25f);
            CreateTree(earth.transform, new Vector3(-11.2f, 0.05f, -6.3f), 0.95f);
            CreateBoulderCluster(earth.transform, new Vector3(-7.2f, 0.12f, -10.0f));

            GameObject air = CreateGroup(root, "Air Landmark");
            CreateWindRibbon(air.transform, new Vector3(8.7f, 1.25f, -8.2f));
            CreateCloudCluster(air.transform, new Vector3(11.0f, 1.25f, -6.5f));
        }

        private static void BuildVillageDressing(Transform root)
        {
            GameObject village = CreateGroup(root, "Village Dressing");

            CreateStall(village.transform, "Potion Stall", new Vector3(-4.8f, 0.1f, 2.7f), new Color(0.10f, 0.55f, 0.45f));
            CreateStall(village.transform, "Gear Stall", new Vector3(4.8f, 0.1f, 2.7f), new Color(0.55f, 0.28f, 0.10f));

            CreateBanner(village.transform, "Unifier Soft Banner", new Vector3(-2.0f, 0.1f, 3.7f), new Color(0.16f, 0.72f, 0.42f));
            CreateBanner(village.transform, "Dominion Soft Banner", new Vector3(2.0f, 0.1f, 3.7f), new Color(0.72f, 0.12f, 0.12f));

            CreateArch(village.transform, "Soft Entrance Arch", new Vector3(0f, 0.1f, 4.2f), new Color(0.26f, 0.22f, 0.17f));
            CreateLantern(village.transform, new Vector3(-3.2f, 0.1f, -1.8f), new Color(0.55f, 0.9f, 1f));
            CreateLantern(village.transform, new Vector3(3.2f, 0.1f, -1.8f), new Color(1f, 0.55f, 0.22f));
        }

        private static void BuildAtmosphere(Transform root)
        {
            GameObject atmosphere = CreateGroup(root, "Atmosphere Dressing");
            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                float radius = 11f + Mathf.Sin(i * 1.7f) * 1.5f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0.25f, Mathf.Sin(angle) * radius);
                CreateGrassTuft(atmosphere.transform, "Grass Tuft " + i, pos);
            }
        }

        private static void RestyleNamedPrimitive(string name, Color color, Vector3 localScale)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                return;
            }

            go.transform.localScale = localScale;
            SetMaterial(go, name + " Clean Material", color);
        }

        private static void SoftenColoredFloor(string name, Color color)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                return;
            }

            SetMaterial(go, name + " Soft Material", color);
            Vector3 scale = go.transform.localScale;
            go.transform.localScale = new Vector3(Mathf.Min(scale.x, 8f), Mathf.Min(scale.y, 0.08f), Mathf.Min(scale.z, 8f));
        }

        private static void RestyleGate(string name, Color accent)
        {
            GameObject gate = GameObject.Find(name);
            if (gate == null)
            {
                return;
            }

            Renderer[] renderers = gate.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                bool beamLike = renderer.gameObject.name.ToLowerInvariant().Contains("beam") || renderer.gameObject.name.ToLowerInvariant().Contains("glow");
                renderer.sharedMaterial = CreateMaterial(renderer.gameObject.name + " Clean Gate Material", beamLike ? accent : new Color(0.15f, 0.13f, 0.18f));
            }

            CreateGateAccent(gate.transform, accent);
        }

        private static void CreateGateAccent(Transform gate, Color accent)
        {
            if (gate.Find("Clean Gate Accent") != null)
            {
                return;
            }

            GameObject root = new GameObject("Clean Gate Accent");
            root.transform.SetParent(gate, false);

            CreateRing(root.transform, "Gate Halo", new Vector3(0f, 2.1f, 0f), 1.15f, 0.035f, accent);
            CreateCone(root.transform, "Gate Top Crystal", new Vector3(0f, 3.15f, 0f), 0.18f, 0.55f, accent);
        }

        private static void RestyleCharacter(string name, Color bodyColor, Color accentColor)
        {
            GameObject character = GameObject.Find(name);
            if (character == null)
            {
                return;
            }

            Renderer renderer = character.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial(name + " Body Material", bodyColor);
            }

            character.transform.localScale = new Vector3(0.75f, 1.0f, 0.75f);

            if (character.transform.Find("Clean Character Details") != null)
            {
                return;
            }

            GameObject details = new GameObject("Clean Character Details");
            details.transform.SetParent(character.transform, false);

            CreateSphere(details.transform, "Head Soft Shape", new Vector3(0f, 1.25f, 0f), new Vector3(0.32f, 0.32f, 0.32f), accentColor);
            CreateCylinder(details.transform, "Shoulder Mantle", new Vector3(0f, 0.78f, 0f), new Vector3(0.55f, 0.04f, 0.55f), accentColor);
            CreateSphere(details.transform, "Chest Gem", new Vector3(0f, 0.72f, -0.38f), new Vector3(0.11f, 0.11f, 0.11f), accentColor);
        }

        private static void AddHostileSilhouette(Transform hostile)
        {
            if (hostile.Find("Clean Hostile Details") != null)
            {
                return;
            }

            GameObject details = new GameObject("Clean Hostile Details");
            details.transform.SetParent(hostile, false);

            CreateSphere(details.transform, "Eye Left", new Vector3(-0.16f, 1.22f, -0.42f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1f, 0.9f, 0.25f));
            CreateSphere(details.transform, "Eye Right", new Vector3(0.16f, 1.22f, -0.42f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1f, 0.9f, 0.25f));
            CreateCone(details.transform, "Back Spike A", new Vector3(0f, 1.15f, 0.38f), 0.1f, 0.45f, new Color(0.06f, 0.04f, 0.06f));
            CreateCone(details.transform, "Back Spike B", new Vector3(0f, 0.82f, 0.42f), 0.1f, 0.35f, new Color(0.06f, 0.04f, 0.06f));
        }

        private static void CreatePebblePath(Transform parent, Vector3 start, Vector3 direction, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = start + direction.normalized * (i * 0.85f);
                float side = Mathf.Sin(i * 1.7f) * 0.22f;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
                CreateEllipse(parent, "Path Stone " + start + " " + i, pos + right * side, new Vector3(0.45f, 0.035f, 0.32f), new Color(0.36f, 0.33f, 0.29f));
            }
        }

        private static void CreateLavaBrazier(Transform parent, Vector3 pos)
        {
            CreateCylinder(parent, "Brazier Base", pos + Vector3.up * 0.18f, new Vector3(0.65f, 0.18f, 0.65f), new Color(0.08f, 0.06f, 0.05f));
            CreateSphere(parent, "Brazier Fire Glow", pos + Vector3.up * 0.55f, new Vector3(0.42f, 0.55f, 0.42f), new Color(1.0f, 0.32f, 0.05f));
            CreateCone(parent, "Brazier Flame", pos + Vector3.up * 0.95f, 0.22f, 0.8f, new Color(1.0f, 0.65f, 0.05f));
        }

        private static void CreatePool(Transform parent, Vector3 pos)
        {
            CreateEllipse(parent, "Pool Rim", pos, new Vector3(1.8f, 0.08f, 1.25f), new Color(0.08f, 0.12f, 0.16f));
            CreateEllipse(parent, "Pool Water", pos + Vector3.up * 0.08f, new Vector3(1.55f, 0.04f, 1.0f), new Color(0.12f, 0.62f, 0.95f));
        }

        private static void CreateReedCluster(Transform parent, Vector3 pos)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector3 offset = new Vector3(Mathf.Cos(i) * 0.45f, 0.35f, Mathf.Sin(i * 1.3f) * 0.35f);
                CreateCylinder(parent, "Reed " + i, pos + offset, new Vector3(0.025f, 0.7f, 0.025f), new Color(0.15f, 0.55f, 0.28f));
            }
        }

        private static void CreateTree(Transform parent, Vector3 pos, float scale)
        {
            CreateCylinder(parent, "Tree Trunk", pos + Vector3.up * 0.5f * scale, new Vector3(0.14f * scale, 1.0f * scale, 0.14f * scale), new Color(0.28f, 0.15f, 0.07f));
            CreateSphere(parent, "Tree Crown", pos + Vector3.up * 1.25f * scale, new Vector3(0.7f * scale, 0.58f * scale, 0.7f * scale), new Color(0.12f, 0.48f, 0.18f));
        }

        private static void CreateBoulderCluster(Transform parent, Vector3 pos)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(Mathf.Cos(i) * 0.45f, 0.22f, Mathf.Sin(i * 1.3f) * 0.35f);
                CreateSphere(parent, "Boulder " + i, pos + offset, new Vector3(0.45f, 0.32f, 0.38f), new Color(0.22f, 0.20f, 0.16f));
            }
        }

        private static void CreateWindRibbon(Transform parent, Vector3 pos)
        {
            CreateRing(parent, "Wind Ring A", pos, 0.75f, 0.03f, new Color(0.75f, 0.95f, 1.0f));
            CreateRing(parent, "Wind Ring B", pos + Vector3.up * 0.35f, 1.0f, 0.03f, new Color(0.75f, 0.95f, 1.0f));
            CreateRing(parent, "Wind Ring C", pos + Vector3.up * 0.7f, 1.25f, 0.03f, new Color(0.75f, 0.95f, 1.0f));
        }

        private static void CreateCloudCluster(Transform parent, Vector3 pos)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(Mathf.Cos(i) * 0.5f, Mathf.Sin(i * 0.7f) * 0.12f, Mathf.Sin(i) * 0.25f);
                CreateSphere(parent, "Cloud " + i, pos + offset, new Vector3(0.5f, 0.25f, 0.35f), new Color(0.86f, 0.96f, 1.0f));
            }
        }

        private static void CreateCrystalCluster(Transform parent, Vector3 pos, Color color, string name)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3((i - 2) * 0.22f, 0.25f, Mathf.Sin(i) * 0.16f);
                CreateCone(parent, name + " " + i, pos + offset, 0.12f, 0.55f + i * 0.08f, color);
            }
        }

        private static void CreateStall(Transform parent, string name, Vector3 pos, Color canopy)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Stall Base", pos + Vector3.up * 0.28f, new Vector3(0.7f, 0.35f, 0.48f), new Color(0.35f, 0.18f, 0.08f));
            CreateCylinder(group.transform, "Stall Canopy", pos + Vector3.up * 0.82f, new Vector3(0.92f, 0.12f, 0.62f), canopy);
            CreateSphere(group.transform, "Canopy Cap Left", pos + new Vector3(-0.43f, 0.82f, 0f), new Vector3(0.18f, 0.13f, 0.26f), canopy);
            CreateSphere(group.transform, "Canopy Cap Right", pos + new Vector3(0.43f, 0.82f, 0f), new Vector3(0.18f, 0.13f, 0.26f), canopy);
        }

        private static void CreateBanner(Transform parent, string name, Vector3 pos, Color color)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Banner Pole", pos + Vector3.up * 0.8f, new Vector3(0.035f, 1.6f, 0.035f), new Color(0.10f, 0.08f, 0.06f));
            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cloth.name = "Banner Cloth";
            cloth.transform.SetParent(group.transform, true);
            cloth.transform.position = pos + new Vector3(0.25f, 1.05f, 0f);
            cloth.transform.localScale = new Vector3(0.55f, 0.8f, 1f);
            cloth.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            SetMaterial(cloth, name + " Cloth Material", color);
            Collider collider = cloth.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
        }

        private static void CreateArch(Transform parent, string name, Vector3 pos, Color color)
        {
            GameObject group = CreateGroup(parent, name);
            CreateCylinder(group.transform, "Arch Left", pos + new Vector3(-1.0f, 0.9f, 0f), new Vector3(0.08f, 1.8f, 0.08f), color);
            CreateCylinder(group.transform, "Arch Right", pos + new Vector3(1.0f, 0.9f, 0f), new Vector3(0.08f, 1.8f, 0.08f), color);
            CreateRing(group.transform, "Arch Top", pos + new Vector3(0f, 1.8f, 0f), 1.0f, 0.05f, color);
        }

        private static void CreateLantern(Transform parent, Vector3 pos, Color glow)
        {
            CreateCylinder(parent, "Lantern Pole", pos + Vector3.up * 0.65f, new Vector3(0.035f, 1.3f, 0.035f), new Color(0.08f, 0.07f, 0.06f));
            CreateSphere(parent, "Lantern Glow", pos + Vector3.up * 1.25f, new Vector3(0.18f, 0.18f, 0.18f), glow);
        }

        private static void CreateGrassTuft(Transform parent, string name, Vector3 pos)
        {
            GameObject group = CreateGroup(parent, name);
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3((i - 1) * 0.06f, 0.18f, Mathf.Sin(i) * 0.04f);
                CreateCone(group.transform, "Blade " + i, pos + offset, 0.025f, 0.36f, new Color(0.12f, 0.5f, 0.18f));
            }
        }

        private static GameObject CreateGroup(Transform parent, string name)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, true);
            return group;
        }

        private static void DestroyIfFound(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
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

        private static GameObject CreateEllipse(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
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
            meshFilter.sharedMesh = BuildConeMesh(radius, height, 14);
            meshRenderer.sharedMaterial = CreateMaterial(name + " Material", color);
            return go;
        }

        private static GameObject CreateRing(Transform parent, string name, Vector3 position, float radius, float tubeRadius, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = position;

            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = BuildTorusMesh(radius, tubeRadius, 32, 8);
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
                float angle = Mathf.PI * 2f * i / segments;
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

                for (int t = 0; t < tubeSegments; t++)
                {
                    float phi = 2f * Mathf.PI * t / tubeSegments;
                    Vector3 offset = radial * (Mathf.Cos(phi) * tubeRadius) + Vector3.up * (Mathf.Sin(phi) * tubeRadius);
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

        private static void SetMaterial(GameObject go, string name, Color color)
        {
            Renderer renderer = go != null ? go.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial(name, color);
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
    }
}
#endif
