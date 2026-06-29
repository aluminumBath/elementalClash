#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterCombatDefenseContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string CombatDir = BaseDir + "/Combat";
        private const string DefenseDir = CombatDir + "/Defense";

        [MenuItem("Elementborn/Generate Starter Content/Combat Defense and Stamina")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(DefenseDir);

            CreateTuning("StarterBalancedStamina", 100f, 18f, 0.75f, 5f, 8f, 55f, 0.18f, 100f, 10f, 25f, 0.45f, 0.28f, 4f, 0.35f);
            CreateTuning("HeavyShieldStamina", 120f, 12f, 1.0f, 8f, 10f, 72f, 0.14f, 100f, 8f, 32f, 0.38f, 0.22f, 3f, 0.45f);
            CreateTuning("AgileDodgeStamina", 90f, 24f, 0.55f, 4f, 6f, 42f, 0.2f, 100f, 12f, 18f, 0.38f, 0.26f, 4.6f, 0.2f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter combat defense/stamina tuning assets.");
        }

        private static void CreateTuning(
            string id,
            float maxStamina,
            float regen,
            float regenDelay,
            float blockStart,
            float blockDrain,
            float blockReduction,
            float perfectWindow,
            float perfectReduction,
            float perfectRefund,
            float dodgeCost,
            float dodgeDuration,
            float dodgeIFrames,
            float dodgeDistance,
            float dodgeCooldown)
        {
            string path = $"{DefenseDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<CombatStaminaTuning>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CombatStaminaTuning>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("maxStamina").floatValue = maxStamina;
            so.FindProperty("regenPerSecond").floatValue = regen;
            so.FindProperty("regenDelayAfterSpend").floatValue = regenDelay;
            so.FindProperty("blockStartCost").floatValue = blockStart;
            so.FindProperty("blockDrainPerSecond").floatValue = blockDrain;
            so.FindProperty("blockDamageReductionPercent").floatValue = blockReduction;
            so.FindProperty("perfectBlockWindowSeconds").floatValue = perfectWindow;
            so.FindProperty("perfectBlockReductionPercent").floatValue = perfectReduction;
            so.FindProperty("perfectBlockStaminaRefund").floatValue = perfectRefund;
            so.FindProperty("dodgeCost").floatValue = dodgeCost;
            so.FindProperty("dodgeDurationSeconds").floatValue = dodgeDuration;
            so.FindProperty("dodgeIFrameSeconds").floatValue = dodgeIFrames;
            so.FindProperty("dodgeDistance").floatValue = dodgeDistance;
            so.FindProperty("dodgeCooldownSeconds").floatValue = dodgeCooldown;
            so.FindProperty("exhaustedThreshold").floatValue = 1f;
            so.FindProperty("exhaustedRecoveryThreshold").floatValue = Mathf.Max(20f, maxStamina * 0.2f);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
