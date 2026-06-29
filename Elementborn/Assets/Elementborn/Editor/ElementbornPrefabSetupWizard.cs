#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrefabSetupWizard
    {
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs";

        [MenuItem("Elementborn/Unity Setup/Create Recommended Generated Prefabs")]
        public static void CreateRecommendedGeneratedPrefabs()
        {
            EnsurePrefabDir();
            CreateRuntimeSystemsPrefab();
            CreateMinimalPlayerPrefab();
            CreateUiCanvasPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Elementborn generated setup prefabs.");
        }

        [MenuItem("Elementborn/Unity Setup/Create Runtime Systems Prefab")]
        public static void CreateRuntimeSystemsPrefab()
        {
            EnsurePrefabDir();
            var go = new GameObject("Elementborn_RuntimeSystems_Prefab");
            go.AddComponent<ElementbornRuntimeBootstrap>();
            go.AddComponent<ElementbornIntegrationDiagnostics>();
            SaveAndDestroy(go, PrefabDir + "/Elementborn_RuntimeSystems.prefab");
        }

        [MenuItem("Elementborn/Unity Setup/Create Minimal Player Prefab")]
        public static void CreateMinimalPlayerPrefab()
        {
            EnsurePrefabDir();
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Elementborn_MinimalPlayer_Prefab";
            go.tag = "Player";
            go.AddComponent<CharacterController>();
            go.AddComponent<SimpleCombatHealth>();
            go.AddComponent<StaminaResource>();
            go.AddComponent<CombatDefenseController>();
            go.AddComponent<PlayerCombatInputController>();
            go.AddComponent<SpellResourcePool>();
            go.AddComponent<SpellCastController>();
            go.AddComponent<SpellLoadoutController>();
            go.AddComponent<PlayerInteractor>();
            go.AddComponent<PlayerTestRigSetup>();
            SaveAndDestroy(go, PrefabDir + "/Elementborn_MinimalPlayer.prefab");
        }

        [MenuItem("Elementborn/Unity Setup/Create Basic UI Canvas Prefab")]
        public static void CreateUiCanvasPrefab()
        {
            EnsurePrefabDir();
            var canvasGo = new GameObject("Elementborn_BasicUiCanvas_Prefab", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<QuestTrackerHudView>();
            canvasGo.AddComponent<QuestObjectivePopupView>();
            canvasGo.AddComponent<SpellResourceBarView>();
            canvasGo.AddComponent<SpellCastBarView>();
            canvasGo.AddComponent<BossHealthBarView>();
            SaveAndDestroy(canvasGo, PrefabDir + "/Elementborn_BasicUiCanvas.prefab");
        }

        private static void EnsurePrefabDir()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Elementborn/Generated")) AssetDatabase.CreateFolder("Assets/Elementborn", "Generated");
            if (!AssetDatabase.IsValidFolder(PrefabDir)) AssetDatabase.CreateFolder("Assets/Elementborn/Generated", "Prefabs");
        }

        private static void SaveAndDestroy(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
    }
}
#endif
