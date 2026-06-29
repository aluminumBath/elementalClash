#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornUiSceneInstaller
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/UI";

        [MenuItem("Elementborn/UI Prefabs/Install Playable HUD In Open Scene")]
        public static void InstallPlayableHud()
        {
            ElementbornUiPrefabFactory.CreatePlayableHudPrefab();
            InstallPrefab($"{PrefabDir}/Elementborn_PlayableHudCanvas.prefab");
        }

        [MenuItem("Elementborn/UI Prefabs/Install Boss HUD In Open Scene")]
        public static void InstallBossHud()
        {
            ElementbornUiPrefabFactory.CreateBossHudPrefab();
            InstallPrefab($"{PrefabDir}/Elementborn_BossHudCanvas.prefab");
        }

        [MenuItem("Elementborn/UI Prefabs/Install Spell HUD In Open Scene")]
        public static void InstallSpellHud()
        {
            ElementbornUiPrefabFactory.CreateSpellHudPrefab();
            InstallPrefab($"{PrefabDir}/Elementborn_SpellHudCanvas.prefab");
        }

        private static void InstallPrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found at {path}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                Undo.RegisterCreatedObjectUndo(instance, $"Install {prefab.name}");
                EditorUtility.SetDirty(instance);
                Debug.Log($"Installed {prefab.name} in open scene.");
            }
        }
    }
}
#endif
