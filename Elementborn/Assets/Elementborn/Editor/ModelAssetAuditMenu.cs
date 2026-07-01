#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game.EditorTools
{
    /// <summary>
    /// Read-only audit of imported 3D model assets. It answers the two questions that make a freshly-imported
    /// creature look wrong in-game:
    ///   1. "Which models are missing materials?" (renders white/untextured, or magenta when a shader is broken
    ///      or a built-in/Standard material is used under URP), and
    ///   2. "Which models look mis-rotated?" (bounds are far deeper than they are tall — the classic lying-flat /
    ///      up-axis import problem, e.g. the rabbit on its side while the bat stands upright).
    /// It also lists every creature the game expects a model for but cannot find in Resources, so you can see at a
    /// glance which creatures are still showing the primitive placeholder (cyan sphere / tan cube).
    ///
    /// Everything is reported to the Console. Nothing is modified — the fixes are per-asset import settings
    /// (Materials > Extract Materials/Textures, Model > Bake Axis Conversion, or Convert to URP), which this tool
    /// only points you to.
    /// Menu: Elementborn/Model Audit/*
    /// </summary>
    public static class ModelAssetAuditMenu
    {
        [MenuItem("Elementborn/Model Audit/Scan Models For Issues")]
        public static void ScanModels()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Elementborn Model Audit ===");

            string[] guids = AssetDatabase.FindAssets("t:Model");
            sb.AppendLine("Imported model assets found: " + guids.Length);

            int withMaterialIssues = 0, withRotationWarning = 0, clean = 0;
            var flagged = new List<Object>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                List<string> materialProblems = MaterialProblems(go);
                string rotationNote = RotationWarning(go, path);

                if (materialProblems.Count > 0)
                {
                    withMaterialIssues++;
                    sb.AppendLine("  [MATERIAL] " + path);
                    foreach (var p in materialProblems) sb.AppendLine("        - " + p);
                }
                if (!string.IsNullOrEmpty(rotationNote))
                {
                    withRotationWarning++;
                    sb.AppendLine("  [ROTATION] " + path + "   " + rotationNote);
                }
                if (materialProblems.Count == 0 && string.IsNullOrEmpty(rotationNote)) clean++;
                else flagged.Add(go);
            }

            sb.AppendLine(string.Format(
                "Summary: {0} clean, {1} with material issues, {2} with rotation warnings.",
                clean, withMaterialIssues, withRotationWarning));

            AppendMissingCreatureModels(sb);

            // Leave the flagged assets selected so you can arrow through them in the Project window.
            if (flagged.Count > 0) Selection.objects = flagged.ToArray();

            Debug.Log(sb.ToString());
        }

        /// <summary>Per-renderer material issues that make a model render white or magenta.</summary>
        private static List<string> MaterialProblems(GameObject go)
        {
            var problems = new List<string>();
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    problems.Add(r.name + ": no material slots");
                    continue;
                }
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null)
                    {
                        problems.Add(r.name + ": empty material slot " + i + " (renders white/pink)");
                        continue;
                    }
                    var shader = m.shader;
                    if (shader == null)
                    {
                        problems.Add(r.name + ": material '" + m.name + "' has no shader");
                        continue;
                    }
                    string sn = shader.name;
                    if (sn == "Hidden/InternalErrorShader")
                        problems.Add(r.name + ": material '" + m.name + "' shader is broken (magenta) — reassign or Convert to URP");
                    else if (sn == "Standard" || sn.StartsWith("Legacy Shaders/"))
                        problems.Add(r.name + ": material '" + m.name + "' uses built-in '" + sn + "' (magenta under URP) — Assets > Convert > Built-in Materials to URP");
                    else if (m.name == "Default-Material" || m.name.StartsWith("Default-"))
                        problems.Add(r.name + ": using Unity default material (untextured white) — Materials tab > Extract Materials, or assign one");
                }
            }
            return problems;
        }

        /// <summary>
        /// Heuristic up-axis check. A creature/character standing correctly is taller (Y) than it is deep (Z).
        /// If the combined mesh bounds are markedly deeper than tall, the model is probably lying flat — a
        /// per-asset bake-axis / import-orientation issue. Reported, never auto-fixed.
        /// </summary>
        private static string RotationWarning(GameObject go, string path)
        {
            Bounds bounds = default;
            bool hasBounds = false;

            foreach (var f in go.GetComponentsInChildren<MeshFilter>(true))
            {
                if (f.sharedMesh == null) continue;
                if (!hasBounds) { bounds = f.sharedMesh.bounds; hasBounds = true; }
                else bounds.Encapsulate(f.sharedMesh.bounds);
            }
            foreach (var s in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (s.sharedMesh == null) continue;
                if (!hasBounds) { bounds = s.sharedMesh.bounds; hasBounds = true; }
                else bounds.Encapsulate(s.sharedMesh.bounds);
            }
            if (!hasBounds) return null;

            Vector3 size = bounds.size;
            if (size.z > size.y * 1.6f && size.z > size.x * 1.1f)
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                string bakeState = importer != null ? ("  [bakeAxisConversion=" + importer.bakeAxisConversion + "]") : "";
                return string.Format("bounds {0:0.00}x{1:0.00}x{2:0.00} (deep > tall; may be lying flat){3}",
                    size.x, size.y, size.z, bakeState);
            }
            return null;
        }

        /// <summary>
        /// Lists creature kinds the game will look for but cannot load from Resources, mirroring exactly how
        /// <see cref="CreatureModelLibrary"/> resolves them. A missing entry means that creature currently spawns
        /// with its primitive placeholder instead of a real model.
        /// </summary>
        private static void AppendMissingCreatureModels(StringBuilder sb)
        {
            sb.AppendLine("");
            sb.AppendLine("--- Creature models the game expects (missing = still shows primitive placeholder) ---");
            int present = 0, missing = 0;
            foreach (CreatureKind kind in System.Enum.GetValues(typeof(CreatureKind)))
            {
                bool found = false;
                string firstPath = null;
                foreach (var p in CreatureModelNames.CandidatePaths(kind))
                {
                    if (firstPath == null) firstPath = p;
                    if (Resources.Load<GameObject>(p) != null) { found = true; break; }
                }
                if (found) present++;
                else { missing++; sb.AppendLine("  [MISSING] " + kind + "   ->   Resources/" + firstPath); }
            }
            sb.AppendLine(string.Format("Creature models: {0} present, {1} missing.", present, missing));
        }
    }
}
#endif
