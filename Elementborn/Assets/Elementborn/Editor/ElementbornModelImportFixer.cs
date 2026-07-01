#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    /// <summary>
    /// Companion to <see cref="ModelAssetAuditMenu"/> (which only reports). This one FIXES the two things that make
    /// freshly-imported Meshy models look wrong in-game:
    ///   1. <b>Lying flat / wrong up-axis</b> — turns on the model importer's Bake Axis Conversion.
    ///   2. <b>Rendering white/untextured</b> — builds a URP/Lit material per source material that plugs in the
    ///      sibling <c>*_texture.png</c> (albedo), optional <c>*_normal.png</c>, and <c>*_metallic.png</c>, then
    ///      remaps the fbx's embedded materials onto those via the importer's external-object map and reimports.
    ///
    /// It only touches models under the Meshy folders (and Resources/Models), is fully re-runnable, and every change
    /// is an import setting you can revert (Reimport / toggle the setting back). Generated materials land in
    /// <c>Assets/Elementborn/Art/Models/_GeneratedURPMaterials/</c>.
    ///
    /// Menu: Elementborn/Model Fix/*    Run the audit first (Elementborn/Model Audit) to see what's flagged.
    /// </summary>
    public static class ElementbornModelImportFixer
    {
        private static readonly string[] SearchFolders =
        {
            "Assets/Elementborn/Art/Models/MeshyImported",
            "Assets/Elementborn/Resources/Models",
        };

        private const string GeneratedMatDir = "Assets/Elementborn/Art/Models/_GeneratedURPMaterials";

        [MenuItem("Elementborn/Model Fix/Bake Axis Conversion (orientation only)")]
        public static void BakeAxisOnly()
        {
            if (!EditorUtility.DisplayDialog("Bake Axis Conversion",
                "Turn on Bake Axis Conversion for every Meshy model and reimport?\n\n" +
                "This fixes models that import lying on their side/stomach. Re-runnable and revertible.",
                "Fix orientation", "Cancel")) return;

            int changed = 0;
            foreach (string path in ModelPaths())
            {
                var mi = AssetImporter.GetAtPath(path) as ModelImporter;
                if (mi == null) continue;
                if (!mi.bakeAxisConversion)
                {
                    mi.bakeAxisConversion = true;
                    mi.SaveAndReimport();
                    changed++;
                }
            }
            Debug.Log($"[Elementborn] Bake Axis Conversion enabled on {changed} model(s).");
        }

        [MenuItem("Elementborn/Model Fix/Rebuild URP Materials + Bake Axis (all Meshy models)")]
        public static void RebuildMaterialsAndAxis()
        {
            if (!EditorUtility.DisplayDialog("Rebuild model materials",
                "For every Meshy model: enable Bake Axis Conversion, build a URP/Lit material from the sibling " +
                "*_texture.png (+ normal/metallic if present), and remap the model onto it.\n\n" +
                "Generated materials go in Art/Models/_GeneratedURPMaterials/. Re-runnable and revertible.",
                "Fix models", "Cancel")) return;

            if (!AssetDatabase.IsValidFolder(GeneratedMatDir))
            {
                Directory.CreateDirectory(GeneratedMatDir);
                AssetDatabase.Refresh();
            }

            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            if (urp == null)
            {
                Debug.LogError("[Elementborn] URP/Lit shader not found — is the Universal RP package active? Aborting.");
                return;
            }

            var sb = new StringBuilder("=== Elementborn Model Fix ===\n");
            int models = 0, mats = 0, noTex = 0;

            try
            {
                foreach (string path in ModelPaths())
                {
                    var mi = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (mi == null) continue;
                    models++;

                    mi.bakeAxisConversion = true;

                    string dir = Path.GetDirectoryName(path);
                    Texture2D albedo = FindTexture(dir, "_texture", "_albedo", "_basecolor", "_diffuse");
                    Texture2D normal = FindTexture(dir, "_normal");
                    Texture2D metallic = FindTexture(dir, "_metallic");
                    if (albedo == null) { noTex++; mi.SaveAndReimport(); continue; }

                    string modelName = Path.GetFileNameWithoutExtension(path);
                    foreach (Object rep in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                    {
                        var srcMat = rep as Material;
                        if (srcMat == null) continue;

                        string matPath = $"{GeneratedMatDir}/{Sanitize(modelName)}__{Sanitize(srcMat.name)}.mat";
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                        if (mat == null) { mat = new Material(urp); AssetDatabase.CreateAsset(mat, matPath); }
                        mat.shader = urp;
                        mat.SetColor("_BaseColor", Color.white);
                        mat.SetTexture("_BaseMap", albedo);
                        if (normal != null) { mat.SetTexture("_BumpMap", normal); mat.EnableKeyword("_NORMALMAP"); }
                        if (metallic != null) { mat.SetTexture("_MetallicGlossMap", metallic); mat.EnableKeyword("_METALLICSPECGLOSSMAP"); }
                        EditorUtility.SetDirty(mat);

                        mi.AddRemap(new AssetImporter.SourceAssetIdentifier(srcMat), mat);
                        mats++;
                    }
                    mi.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            sb.AppendLine($"Models processed: {models}");
            sb.AppendLine($"URP materials built/remapped: {mats}");
            sb.AppendLine($"Models with no albedo texture found (axis fixed, material left as-is): {noTex}");
            sb.AppendLine("Re-run the Model Audit to confirm nothing is still flagged.");
            Debug.Log(sb.ToString());
        }

        private static IEnumerable<string> ModelPaths()
        {
            var seen = new HashSet<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", SearchFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (seen.Add(path)) yield return path;
            }
        }

        // Find a texture in the model's folder (recursively) whose name ends with one of the given suffixes.
        private static Texture2D FindTexture(string dir, params string[] suffixes)
        {
            if (string.IsNullOrEmpty(dir)) return null;
            foreach (string file in Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories))
            {
                string lower = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                foreach (string s in suffixes)
                    if (lower.EndsWith(s))
                    {
                        string assetPath = file.Replace('\\', '/');
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                        if (tex != null) return tex;
                    }
            }
            return null;
        }

        private static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Replace(' ', '_');
        }
    }
}
#endif
