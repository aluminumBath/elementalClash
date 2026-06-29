#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class PlayableSceneProductionPolishBuilder
    {
        private const string MaterialDir = "Assets/Elementborn/Generated/Art/Materials";

        [MenuItem("Elementborn/Playable Setup/Apply Production Scene Polish")]
        public static void ApplyProductionPolish()
        {
            Directory.CreateDirectory(MaterialDir);
            Material road = EnsureMaterial("M_WarmStoneRoad", new Color(0.46f, 0.32f, 0.22f));
            Material sign = EnsureMaterial("M_WoodSign", new Color(0.46f, 0.25f, 0.12f));
            Material grass = EnsureMaterial("M_PlayableGrass", new Color(0.24f, 0.42f, 0.24f));
            Material lava = EnsureMaterial("M_LavaGlow", new Color(1f, 0.38f, 0.08f));
            Material pathGlow = EnsureMaterial("M_PathGlow", new Color(0.9f, 0.65f, 0.24f));

            GameObject root = GameObject.Find("Playable Scene Polish");
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
            root = new GameObject("Playable Scene Polish");

            CreatePlatform(root.transform, "Central Playtest Plaza", new Vector3(0f, -0.02f, 10f), new Vector3(28f, 0.2f, 28f), grass);
            CreateRoad(root.transform, "Road_To_FireCapital", new Vector3(-35f, 0.05f, -24f), new Vector3(6f, 0.12f, 78f), road, Quaternion.Euler(0f, 31f, 0f));
            CreateRoad(root.transform, "Road_To_WindCapital", new Vector3(-34f, 0.05f, 35f), new Vector3(5f, 0.12f, 70f), road, Quaternion.Euler(0f, -40f, 0f));
            CreateRoad(root.transform, "Road_To_EarthCapital", new Vector3(35f, 0.05f, -24f), new Vector3(5f, 0.12f, 75f), road, Quaternion.Euler(0f, -32f, 0f));
            CreateRoad(root.transform, "Road_To_MetalCapital", new Vector3(34f, 0.05f, 36f), new Vector3(5f, 0.12f, 75f), road, Quaternion.Euler(0f, 40f, 0f));

            CreateLavaChannel(root.transform, "FireCapital_Lava_Channel_A", new Vector3(-64f, 0.08f, -38f), new Vector3(3f, 0.15f, 18f), lava);
            CreateLavaChannel(root.transform, "FireCapital_Lava_Channel_B", new Vector3(-78f, 0.08f, -40f), new Vector3(2f, 0.15f, 15f), lava);
            CreateRoad(root.transform, "FireCapital_Glowing_Steps", new Vector3(-70f, 0.2f, -53f), new Vector3(7f, 0.15f, 12f), pathGlow, Quaternion.identity);

            CreateSign(root.transform, "Sign_FireCapital", "Fire Capital / Volcano Citadel", new Vector3(-45f, 0.2f, -28f), sign);
            CreateSign(root.transform, "Sign_Orphanage", "Crab-Sign Creature Orphanage", new Vector3(-5f, 0.2f, -10f), sign);
            CreateSign(root.transform, "Sign_WolfPack", "Warning: Romilus & Madrangea Pack", new Vector3(18f, 0.2f, -13f), sign);
            CreateSign(root.transform, "Sign_Dashboard", "Debug Dashboard: top-left panel + action buttons", new Vector3(5f, 0.2f, 1f), sign);

            CreateSceneGuideCanvas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Applied production-style scene polish.");
        }

        private static void CreateSceneGuideCanvas()
        {
            GameObject canvas = GameObject.Find("Elementborn Scene Guide");
            if (canvas != null)
            {
                Object.DestroyImmediate(canvas);
            }

            canvas = new GameObject("Elementborn Scene Guide");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Guide Panel");
            panel.transform.SetParent(canvas.transform, false);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.04f, 0.05f, 0.07f, 0.72f);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.64f, 0.68f);
            rect.anchorMax = new Vector2(0.98f, 0.96f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            GameObject textGo = new GameObject("Guide Text");
            textGo.transform.SetParent(panel.transform, false);
            Text text = textGo.AddComponent<Text>();
            Elementborn.Game.ElementbornBuiltinFontUtility.ApplyDefaultFont(text);
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text =
                "Elementborn Playtest Slice\\n" +
                "1. Visit the Fire Capital volcano citadel.\\n" +
                "2. Use dashboard buttons to evaluate/sync systems.\\n" +
                "3. Trigger social/orphanage/wolf-pack flows.\\n" +
                "4. Follow quest markers from the Fire royal court.";

            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.04f, 0.04f);
            textRect.anchorMax = new Vector2(0.96f, 0.96f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private static void CreatePlatform(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            Apply(go, mat);
        }

        private static void CreateRoad(Transform parent, string name, Vector3 position, Vector3 scale, Material mat, Quaternion rotation)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.transform.rotation = rotation;
            Apply(go, mat);
        }

        private static void CreateLavaChannel(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            CreateRoad(parent, name, position, scale, mat, Quaternion.identity);
        }

        private static void CreateSign(Transform parent, string name, string label, Vector3 position, Material mat)
        {
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = name;
            sign.transform.SetParent(parent, true);
            sign.transform.position = position + Vector3.up * 1.5f;
            sign.transform.localScale = new Vector3(5f, 1.2f, 0.25f);
            Apply(sign, mat);

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(sign.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.16f);
            textGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh mesh = textGo.AddComponent<TextMesh>();
            mesh.text = label;
            mesh.characterSize = 0.18f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
        }

        private static void Apply(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material EnsureMaterial(string name, Color color)
        {
            string path = $"{MaterialDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
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
    }
}
#endif
