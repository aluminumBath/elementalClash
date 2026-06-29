#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Elementborn.Game.EditorTools
{
    /// <summary>
    /// Repairs generated Elementborn materials for the active render pipeline.
    /// Unity renders unsupported shaders as bright magenta/neon pink, which commonly happens
    /// when generated Built-in/Standard materials are used in URP/HDRP projects.
    /// </summary>
    public static class ElementbornRenderPipelineMaterialUtility
    {
        private const string GeneratedMaterialDir = "Assets/Elementborn/Generated/Art/Materials";

        [MenuItem("Elementborn/Visuals/Fix Pink Materials Everywhere")]
        public static void FixPinkMaterialsEverywhere()
        {
            int assetCount = FixGeneratedMaterialsAssets();
            int sceneCount = FixOpenSceneRenderers();

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"Elementborn visual material repair complete. Updated {assetCount} material asset(s) and {sceneCount} scene renderer material slot(s).");
        }

        [MenuItem("Elementborn/Visuals/Fix Generated Material Assets")]
        public static int FixGeneratedMaterialsAssets()
        {
            int changed = 0;
            string[] searchRoots = Directory.Exists("Assets/Elementborn")
                ? new[] { "Assets/Elementborn" }
                : new[] { "Assets" };

            string[] guids = AssetDatabase.FindAssets("t:Material", searchRoots);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("Assets/Elementborn"))
                {
                    continue;
                }

                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (RepairMaterial(material))
                {
                    changed++;
                    EditorUtility.SetDirty(material);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Elementborn generated/material asset repair updated {changed} material asset(s).");
            return changed;
        }

        [MenuItem("Elementborn/Visuals/Fix Open Scene Renderer Materials")]
        public static int FixOpenSceneRenderers()
        {
            int changed = 0;
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool rendererChanged = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];

                    if (material == null)
                    {
                        materials[i] = CreateRuntimeSafeMaterial(renderer.name + "_AutoMaterial", Color.white);
                        rendererChanged = true;
                        changed++;
                        continue;
                    }

                    string assetPath = AssetDatabase.GetAssetPath(material);
                    if (!string.IsNullOrWhiteSpace(assetPath))
                    {
                        if (RepairMaterial(material))
                        {
                            rendererChanged = true;
                            changed++;
                        }
                    }
                    else if (ShouldReplaceShader(material.shader))
                    {
                        Color color = ReadMaterialColor(material, Color.white);
                        Material replacement = CreateRuntimeSafeMaterial(material.name + "_PipelineSafe", color);
                        materials[i] = replacement;
                        rendererChanged = true;
                        changed++;
                    }
                }

                if (rendererChanged)
                {
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Elementborn open-scene renderer material repair updated {changed} material slot(s).");
            return changed;
        }

        public static Material EnsureMaterial(string path, Color color)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return CreateRuntimeSafeMaterial("Elementborn_Material", color);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path).Replace('\\', '/'));

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = CreateRuntimeSafeMaterial(Path.GetFileNameWithoutExtension(path), color);
                AssetDatabase.CreateAsset(material, path);
                EditorUtility.SetDirty(material);
                return material;
            }

            bool changed = RepairMaterial(material);
            if (ApplyColor(material, color))
            {
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        public static Material CreateRuntimeSafeMaterial(string name, Color color)
        {
            Shader shader = FindBestLitShader();
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            Material material = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            material.name = string.IsNullOrWhiteSpace(name) ? "Elementborn_Material" : name;
            ApplyColor(material, color);
            return material;
        }

        public static bool RepairMaterial(Material material)
        {
            if (material == null)
            {
                return false;
            }

            bool changed = false;
            Shader shader = FindBestLitShader();

            if (shader != null && ShouldReplaceShader(material.shader))
            {
                Color color = ReadMaterialColor(material, Color.white);
                material.shader = shader;
                ApplyColor(material, color);
                changed = true;
            }

            return changed;
        }

        private static bool ShouldReplaceShader(Shader shader)
        {
            if (shader == null)
            {
                return true;
            }

            string name = shader.name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            if (name == "Hidden/InternalErrorShader")
            {
                return true;
            }

            // Built-in Standard frequently renders magenta in URP/HDRP projects.
            if (name == "Standard" && GraphicsSettings.currentRenderPipeline != null)
            {
                return true;
            }

            return false;
        }

        private static Shader FindBestLitShader()
        {
            // Prefer the active pipeline, but keep fallbacks so this remains safe across installs.
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            string pipelineTypeName = pipeline != null ? pipeline.GetType().Name : string.Empty;

            if (pipelineTypeName.Contains("Universal"))
            {
                return Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Standard");
            }

            if (pipelineTypeName.Contains("HDRenderPipeline") || pipelineTypeName.Contains("HD"))
            {
                return Shader.Find("HDRP/Lit")
                    ?? Shader.Find("HDRP/Unlit")
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
            }

            return Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("HDRP/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
        }

        private static Color ReadMaterialColor(Material material, Color fallback)
        {
            if (material == null)
            {
                return fallback;
            }

            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            if (material.HasProperty("_Color"))
            {
                return material.GetColor("_Color");
            }

            return fallback;
        }

        private static bool ApplyColor(Material material, Color color)
        {
            if (material == null)
            {
                return false;
            }

            bool changed = false;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
                changed = true;
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
                changed = true;
            }

            return changed;
        }
    }
}
#endif
