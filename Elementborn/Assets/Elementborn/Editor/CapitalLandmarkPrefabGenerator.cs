
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class CapitalLandmarkPrefabGenerator
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/Capitals";
        private const string MaterialDir = "Assets/Elementborn/Generated/Art/Materials";

        [MenuItem("Elementborn/Capitals/Generate Capital Landmark Prefabs")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(MaterialDir);

            Material basalt = EnsureMaterial("M_BasaltStone", new Color(0.18f, 0.18f, 0.2f));
            Material lava = EnsureMaterial("M_LavaGlow", new Color(1f, 0.38f, 0.08f));
            Material wind = EnsureMaterial("M_WindIvory", new Color(0.87f, 0.93f, 1f));
            Material earth = EnsureMaterial("M_EarthStone", new Color(0.42f, 0.34f, 0.22f));
            Material earthAccent = EnsureMaterial("M_EarthLeaf", new Color(0.25f, 0.58f, 0.25f));
            Material metal = EnsureMaterial("M_MetalSteel", new Color(0.62f, 0.66f, 0.72f));
            Material brass = EnsureMaterial("M_MetalBrass", new Color(0.82f, 0.67f, 0.24f));
            Material neutral = EnsureMaterial("M_NeutralCrystal", new Color(0.72f, 0.6f, 0.92f));
            Material water = EnsureMaterial("M_WaterTile", new Color(0.18f, 0.42f, 0.88f));

            SaveLandmark("FireCapital_VolcanoCitadel", CapitalId.FireCapital, "Volcano Citadel",
                "The Fire Capital crowns the lip of an active volcano. Black basalt bridges, lava falls, and a palace built into the caldera give it a strong visual identity for playtests.",
                root =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float scale = 18f - i * 4f;
                        CreatePrimitive(root, PrimitiveType.Cylinder, "VolcanoRing_" + i, new Vector3(0f, i * 2.4f, 0f), new Vector3(scale, 2.5f, scale), basalt);
                    }
                    CreatePrimitive(root, PrimitiveType.Cylinder, "CraterLava", new Vector3(0f, 10.2f, 0f), new Vector3(5.5f, 0.3f, 5.5f), lava);
                    CreatePrimitive(root, PrimitiveType.Cube, "Causeway", new Vector3(0f, 8.5f, -9f), new Vector3(5f, 0.5f, 10f), basalt);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "ThroneTower", new Vector3(0f, 12f, -3.5f), new Vector3(3f, 4.5f, 3f), basalt);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "SideTowerA", new Vector3(-5f, 10.8f, -1f), new Vector3(1.8f, 3.2f, 1.8f), basalt);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "SideTowerB", new Vector3(5f, 10.8f, -1f), new Vector3(1.8f, 3.2f, 1.8f), basalt);
                    CreatePrimitive(root, PrimitiveType.Cube, "PalaceHall", new Vector3(0f, 10.5f, 1.2f), new Vector3(8f, 2f, 5f), basalt);
                    CreatePrimitive(root, PrimitiveType.Cube, "LavaFallA", new Vector3(-8f, 4f, -6f), new Vector3(0.5f, 6f, 1.2f), lava);
                    CreatePrimitive(root, PrimitiveType.Cube, "LavaFallB", new Vector3(8f, 4f, -6f), new Vector3(0.5f, 6f, 1.2f), lava);
                });

            SaveLandmark("WindCapital_SkySpire", CapitalId.WindCapital, "Skyspire Theocracy",
                "A tall, airy skyline of white towers and floating-looking rings gives the Wind Capital a readable hub silhouette.",
                root =>
                {
                    CreatePrimitive(root, PrimitiveType.Cylinder, "CentralSpire", new Vector3(0f, 8f, 0f), new Vector3(3f, 8f, 3f), wind);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "RingBase", new Vector3(0f, 4f, 0f), new Vector3(10f, 0.4f, 10f), wind);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "RingHigh", new Vector3(0f, 10.5f, 0f), new Vector3(7f, 0.3f, 7f), wind);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "TowerA", new Vector3(-7f, 5f, -4f), new Vector3(1.8f, 5f, 1.8f), wind);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "TowerB", new Vector3(7f, 6f, -4f), new Vector3(1.8f, 6f, 1.8f), wind);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "TowerC", new Vector3(0f, 4.6f, 8f), new Vector3(2.4f, 4.6f, 2.4f), wind);
                });

            SaveLandmark("EarthCapital_TerracePalace", CapitalId.EarthCapital, "Terrace Palace",
                "The Earth Capital uses layered terraces, rooted buttresses, and a planted palace roof to read clearly in the playtest scene.",
                root =>
                {
                    CreatePrimitive(root, PrimitiveType.Cube, "TerraceA", new Vector3(0f, 1.2f, 0f), new Vector3(18f, 2.4f, 18f), earth);
                    CreatePrimitive(root, PrimitiveType.Cube, "TerraceB", new Vector3(0f, 3.1f, 0f), new Vector3(13f, 1.4f, 13f), earth);
                    CreatePrimitive(root, PrimitiveType.Cube, "TerraceC", new Vector3(0f, 4.5f, 0f), new Vector3(9f, 1.2f, 9f), earth);
                    CreatePrimitive(root, PrimitiveType.Cube, "PalaceHall", new Vector3(0f, 6.1f, -1f), new Vector3(8f, 2.5f, 5f), earth);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "GardenRoof", new Vector3(0f, 7.6f, -1f), new Vector3(4.6f, 0.35f, 4.6f), earthAccent);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "PillarA", new Vector3(-5f, 5.6f, 2f), new Vector3(1f, 4f, 1f), earth);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "PillarB", new Vector3(5f, 5.6f, 2f), new Vector3(1f, 4f, 1f), earth);
                });

            SaveLandmark("MetalCapital_FoundryKeep", CapitalId.MetalCapital, "Foundry Keep",
                "The Metal Capital is a blocky, industrial keep with furnaces, smokestacks, and heavy-metal bridges.",
                root =>
                {
                    CreatePrimitive(root, PrimitiveType.Cube, "KeepBase", new Vector3(0f, 2.5f, 0f), new Vector3(16f, 5f, 16f), metal);
                    CreatePrimitive(root, PrimitiveType.Cube, "ForgeHall", new Vector3(0f, 6f, -2f), new Vector3(10f, 2.5f, 6f), metal);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "StackA", new Vector3(-5f, 8f, 4f), new Vector3(1.3f, 5f, 1.3f), brass);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "StackB", new Vector3(5f, 8.5f, 4f), new Vector3(1.3f, 5.5f, 1.3f), brass);
                    CreatePrimitive(root, PrimitiveType.Cube, "ForgeGlow", new Vector3(0f, 4f, 6f), new Vector3(6f, 1f, 2f), lava);
                });

            SaveLandmark("NeutralCity_ConvergenceNexus", CapitalId.NeutralCentralCity, "Convergence Nexus",
                "The neutral central city reads as a shared hub: a plaza of crystal, water, and raised civic platforms.",
                root =>
                {
                    CreatePrimitive(root, PrimitiveType.Cylinder, "CivicPlaza", new Vector3(0f, 0.5f, 0f), new Vector3(15f, 0.5f, 15f), neutral);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "WaterRing", new Vector3(0f, 0.3f, 0f), new Vector3(11f, 0.15f, 11f), water);
                    CreatePrimitive(root, PrimitiveType.Cylinder, "CentralCrystal", new Vector3(0f, 5f, 0f), new Vector3(2f, 5f, 2f), neutral);
                    CreatePrimitive(root, PrimitiveType.Cube, "ForumA", new Vector3(-6f, 1.5f, -6f), new Vector3(4f, 2f, 4f), neutral);
                    CreatePrimitive(root, PrimitiveType.Cube, "ForumB", new Vector3(6f, 1.5f, -6f), new Vector3(4f, 2f, 4f), neutral);
                    CreatePrimitive(root, PrimitiveType.Cube, "ForumC", new Vector3(0f, 1.5f, 7f), new Vector3(6f, 2f, 4f), neutral);
                });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated capital landmark prefabs.");
        }

        [MenuItem("Elementborn/Capitals/Install Capital Landmarks In Open Scene")]
        public static void InstallInOpenScene()
        {
            GenerateAll();
            GameObject parent = GameObject.Find("Capital Landmarks");
            if (parent == null)
            {
                parent = new GameObject("Capital Landmarks");
            }

            Install("FireCapital_VolcanoCitadel", new Vector3(-70f, 0f, -45f), parent.transform);
            Install("WindCapital_SkySpire", new Vector3(-60f, 0f, 60f), parent.transform);
            Install("EarthCapital_TerracePalace", new Vector3(70f, 0f, -45f), parent.transform);
            Install("MetalCapital_FoundryKeep", new Vector3(70f, 0f, 60f), parent.transform);
            Install("NeutralCity_ConvergenceNexus", new Vector3(0f, 0f, 10f), parent.transform);
        }

        private static void Install(string prefabName, Vector3 position, Transform parent)
        {
            string path = $"{PrefabDir}/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"Capital landmark prefab missing: {path}");
                return;
            }

            GameObject existing = GameObject.Find(prefabName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                instance.name = prefabName;
                instance.transform.position = position;
                instance.transform.SetParent(parent, true);
                Undo.RegisterCreatedObjectUndo(instance, "Install capital landmark");
            }
        }

        private static void SaveLandmark(string fileName, CapitalId capitalId, string displayName, string summary, System.Action<Transform> build)
        {
            GameObject root = new GameObject(fileName);
            CapitalLandmarkDescriptor descriptor = root.AddComponent<CapitalLandmarkDescriptor>();
            descriptor.Configure(capitalId, displayName, summary);
            build?.Invoke(root.transform);

            GameObject spawn = new GameObject("PlayerSpawn");
            spawn.transform.SetParent(root.transform, false);
            spawn.transform.localPosition = new Vector3(0f, 1f, -12f);

            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/{fileName}.prefab");
            Object.DestroyImmediate(root);
        }

        private static Material EnsureMaterial(string name, Color color)
        {
            string path = $"{MaterialDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Lit");
                }
                mat = new Material(shader) { color = color };
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
            return go;
        }
    }
}
#endif
