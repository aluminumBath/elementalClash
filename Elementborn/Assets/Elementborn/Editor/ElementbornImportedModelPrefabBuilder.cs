#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornImportedModelPrefabBuilder
    {
        private const string PinkEyeAxolotlFbxPath = "Assets/Elementborn/Art/Models/MeshyImported/PinkEyeAxolotl/Meshy_AI_Pink_Eye_Axolotl_3D_M_0629191922_image-to-3d-texture.fbx";
        private const string PrefabFolder = "Assets/Elementborn/Generated/Prefabs/ImportedModels";

        [MenuItem("Elementborn/Assets/Build Imported Meshy Example Prefabs")]
        public static void BuildImportedMeshyExamplePrefabs()
        {
            EnsureFolder("Assets/Elementborn/Generated");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs");
            EnsureFolder(PrefabFolder);

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PinkEyeAxolotlFbxPath);
            if (modelAsset == null)
            {
                Debug.LogWarning("Could not find imported Meshy FBX at: " + PinkEyeAxolotlFbxPath);
                return;
            }

            BuildPrefab(modelAsset, "PinkEyeAxolotl_Showcase.prefab", ElementbornPrototypeModelAnimationMode.Swim, "Imported Meshy axolotl showcase prefab.");
            BuildPrefab(modelAsset, "PinkEyeAxolotl_Hostile.prefab", ElementbornPrototypeModelAnimationMode.Combat, "Imported Meshy axolotl hostile visual prefab.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Built imported Meshy example prefabs in " + PrefabFolder);
        }

        private static void BuildPrefab(GameObject modelAsset, string prefabName, ElementbornPrototypeModelAnimationMode mode, string notes)
        {
            GameObject root = new GameObject(prefabName.Replace(".prefab", ""));
            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null)
            {
                model = Object.Instantiate(modelAsset);
            }

            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            ElementbornPrototypeImportedModelAnimator animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = mode;
            animator.bobAmplitude = mode == ElementbornPrototypeModelAnimationMode.Combat ? 0.05f : 0.08f;
            animator.boneWiggleDegrees = mode == ElementbornPrototypeModelAnimationMode.Combat ? 7.5f : 5.5f;

            ElementbornPrototypeImportedModelTag tag = root.AddComponent<ElementbornPrototypeImportedModelTag>();
            tag.sourceAssetPath = PinkEyeAxolotlFbxPath;
            tag.modelRole = prefabName;
            tag.notes = notes;

            string path = PrefabFolder + "/" + prefabName;
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
