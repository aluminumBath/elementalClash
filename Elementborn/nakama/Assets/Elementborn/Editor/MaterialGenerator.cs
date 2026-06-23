using UnityEditor;
using UnityEngine;

namespace Elementborn.EditorTools
{
    /// <summary>
    /// Builds cel-shaded <c>.mat</c> assets from the project's toon shaders so the generated rigs and scene have
    /// the intended look instead of Unity's default material. Run <b>Elementborn ▸ Materials ▸ Build Cel-Shaded
    /// Materials</b>, or let <see cref="BootstrapSceneGenerator"/> call <see cref="EnsureMaterials"/> for you.
    ///
    /// Sky / sun / ambient / fog are NOT made here — <c>SceneStyleController</c> sets those up at runtime (it
    /// builds its own ToonSky material). This generator covers mesh materials: a character base, a vertex-coloured
    /// ground (so the terrain's biome colours show through), stone, foliage, four element accents, and a water
    /// material from the ToonWater shader.
    /// </summary>
    public static class MaterialGenerator
    {
        public const string Dir = "Assets/Elementborn/Materials";
        public const string Character = Dir + "/Toon_Character.mat";
        public const string Ground = Dir + "/Toon_Ground.mat";
        public const string Stone = Dir + "/Toon_Stone.mat";
        public const string Foliage = Dir + "/Toon_Foliage.mat";
        public const string Fire = Dir + "/Toon_Fire.mat";
        public const string Water = Dir + "/Toon_Water.mat";
        public const string Earth = Dir + "/Toon_Earth.mat";
        public const string Air = Dir + "/Toon_Air.mat";
        public const string WaterSurface = Dir + "/Water_Toon.mat";

        [MenuItem("Elementborn/Materials/Build Cel-Shaded Materials")]
        public static void BuildMaterials()
        {
            EnsureDir(Dir);

            if (Shader.Find("Elementborn/ToonLit") == null)
                Debug.LogWarning("[Materials] 'Elementborn/ToonLit' not found — open the project so shaders import, then retry.");
            else
            {
                Lit(Character, new Color(0.82f, 0.78f, 0.72f), new Color(0.42f, 0.44f, 0.55f), 2, 0.12f, false);
                Lit(Ground, Color.white, new Color(0.40f, 0.45f, 0.40f), 3, 0f, true);   // terrain biome colours come from vertices
                Lit(Stone, new Color(0.55f, 0.56f, 0.58f), new Color(0.30f, 0.32f, 0.38f), 2, 0f, false);
                Lit(Foliage, new Color(0.36f, 0.62f, 0.32f), new Color(0.16f, 0.34f, 0.20f), 3, 0f, false);
                Lit(Fire, new Color(0.95f, 0.40f, 0.18f), new Color(0.55f, 0.15f, 0.10f), 2, 0.30f, false);
                Lit(Water, new Color(0.26f, 0.60f, 0.90f), new Color(0.10f, 0.26f, 0.50f), 2, 0.20f, false);
                Lit(Earth, new Color(0.60f, 0.45f, 0.30f), new Color(0.32f, 0.22f, 0.14f), 2, 0f, false);
                Lit(Air, new Color(0.80f, 0.92f, 0.95f), new Color(0.55f, 0.66f, 0.72f), 2, 0.25f, false);
            }

            var waterShader = Shader.Find("Elementborn/ToonWater");
            if (waterShader != null) Make(WaterSurface, waterShader, _ => { });
            else Debug.LogWarning("[Materials] 'Elementborn/ToonWater' not found — skipped Water_Toon.");

            AssetDatabase.SaveAssets();
            Debug.Log("[Materials] Cel-shaded materials built in " + Dir);
        }

        /// <summary>Builds the materials if they're missing (called by the bootstrap generator).</summary>
        public static void EnsureMaterials()
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(Character) == null) BuildMaterials();
        }

        public static Material Load(string path) => AssetDatabase.LoadAssetAtPath<Material>(path);

        private static Material Lit(string path, Color baseColor, Color shadowTint, int ramp, float rim, bool vertexColors)
        {
            var shader = Shader.Find("Elementborn/ToonLit");
            if (shader == null) return null;
            return Make(path, shader, m =>
            {
                m.SetColor("_BaseColor", baseColor);
                m.SetColor("_ShadowTint", shadowTint);
                m.SetFloat("_RampSteps", ramp);
                m.SetColor("_RimColor", Color.white);
                m.SetFloat("_RimAmount", rim);
                m.SetColor("_OutlineColor", new Color(0f, 0f, 0f, 1f));
                m.SetFloat("_OutlineWidth", 0.012f);
                m.SetFloat("_VertexColorOn", vertexColors ? 1f : 0f);
                if (vertexColors) m.EnableKeyword("_VERTEXCOLOR_ON");
                else m.DisableKeyword("_VERTEXCOLOR_ON");
            });
        }

        private static Material Make(string path, Shader shader, System.Action<Material> configure)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                configure(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            var mat = new Material(shader);
            configure(mat);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void EnsureDir(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir)) return;
            var parts = dir.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
