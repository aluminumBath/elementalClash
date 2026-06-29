#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornStarterContentRunner
    {
        [MenuItem("Elementborn/Starter Content/Run All Starter Generators Safely")]
        public static void RunAllStarterGeneratorsSafely()
        {
            var failures = new List<string>();
            Run("Items, Recipes, Creatures", StarterElementbornContentGenerator.GenerateAll, failures);
            Run("Abilities and Skill Trees", StarterAbilityContentGenerator.GenerateAll, failures);
            Run("Combat and Status", StarterCombatContentGenerator.GenerateAll, failures);
            Run("Combat Defense and Stamina", StarterCombatDefenseContentGenerator.GenerateAll, failures);
            Run("Enemy AI Profiles", StarterEnemyAiContentGenerator.GenerateAll, failures);
            Run("World Events", StarterWorldEventContentGenerator.GenerateAll, failures);
            Run("Shops", StarterShopContentGenerator.GenerateAll, failures);
            Run("Gathering Resources", StarterGatheringContentGenerator.GenerateAll, failures);
            Run("Equipment and Gear", StarterEquipmentContentGenerator.GenerateAll, failures);
            Run("Bosses and Arenas", StarterBossContentGenerator.GenerateAll, failures);
            Run("Spells and Cooldown UI", StarterSpellContentGenerator.GenerateAll, failures);
            Run("Quest UI Examples", StarterQuestUiContentGenerator.GenerateAll, failures);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (failures.Count == 0)
            {
                Debug.Log("All Elementborn starter generators completed.");
            }
            else
            {
                Debug.LogWarning("Some Elementborn starter generators failed:\n" + string.Join("\n", failures));
            }
        }

        private static void Run(string label, Action action, List<string> failures)
        {
            try
            {
                action?.Invoke();
                Debug.Log($"Elementborn generator complete: {label}");
            }
            catch (Exception ex)
            {
                failures.Add($"- {label}: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
#endif
