#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornGeneratorFieldSafetyMenu
    {
        [MenuItem("Elementborn/Integration/Validate Starter Generator Serialized Fields")]
        public static void ValidateStarterGeneratorSerializedFields()
        {
            var lines = new List<string>
            {
                "# Elementborn Starter Generator Field Safety Report",
                "",
                "This report checks key serialized field names used by the starter content generators.",
                ""
            };

            int issues = 0;
            issues += ValidateScriptable<InventoryItemDefinition>(lines, "InventoryItemDefinition", "itemId", "displayName", "description", "category", "rarity", "questItem", "important", "stackable", "maxStack", "baseValue", "sellable", "usable", "consumedOnUse", "useVerb");
            issues += ValidateScriptable<CraftingRecipeDefinition>(lines, "CraftingRecipeDefinition", "recipeId", "displayName", "description", "category", "stationType", "knownByDefault", "currencyCost", "ingredients", "results");
            issues += ValidateScriptable<CreatureDefinition>(lines, "CreatureDefinition", "creatureId", "displayName", "description", "traversalType", "temperament", "favoriteTreatItemIds", "tameDifficulty", "bondXpForTame", "icon");
            issues += ValidateScriptable<AbilityDefinition>(lines, "AbilityDefinition", "abilityId", "displayName", "description", "element", "category", "defaultSlot", "passive", "ultimate", "defaultUnlockSource", "unlockRequirement", "effect");
            issues += ValidateScriptable<SkillTreeDefinition>(lines, "SkillTreeDefinition", "treeId", "displayName", "description", "element", "nodes");
            issues += ValidateScriptable<StatusEffectDefinition>(lines, "StatusEffectDefinition", "effectId", "displayName", "description", "effectType", "element", "durationSeconds", "tickIntervalSeconds", "tickDamage", "moveSpeedPercentDelta", "attackPowerPercentDelta", "defensePercentDelta", "uniquePerTarget", "icon");
            issues += ValidateScriptable<CombatAttackDefinition>(lines, "CombatAttackDefinition", "attackId", "displayName", "description", "element", "baseDamage", "critChance", "critMultiplier", "knockbackForce", "useEquipmentBonuses", "originType", "statusToApply", "icon");
            issues += ValidateScriptable<LootDropTableDefinition>(lines, "LootDropTableDefinition", "tableId", "displayName", "description", "entries");
            issues += ValidateScriptable<CombatStaminaTuning>(lines, "CombatStaminaTuning", "maxStamina", "regenPerSecond", "regenDelayAfterSpend", "blockStartCost", "blockDrainPerSecond", "blockDamageReductionPercent", "perfectBlockWindowSeconds", "perfectBlockReductionPercent", "perfectBlockStaminaRefund", "dodgeCost", "dodgeDurationSeconds", "dodgeIFrameSeconds", "dodgeDistance", "dodgeCooldownSeconds", "exhaustedThreshold", "exhaustedRecoveryThreshold");
            issues += ValidateScriptable<EnemyCombatProfile>(lines, "EnemyCombatProfile", "profileId", "displayName", "description", "archetype", "preferredElement", "sightRange", "attackRange", "rangedAttackRange", "loseTargetRange", "hearingRange", "patrolSpeed", "chaseSpeed", "strafeSpeed", "fleeSpeed", "stoppingDistance", "canStrafe", "attackCooldownSeconds", "rangedCooldownSeconds", "lowHealthFleePercent", "fleeWhenLowHealth", "useWeaknesses", "canAttackBoats", "canAttackCreatures", "usesBossPhases");
            issues += ValidateScriptable<WorldEventDefinition>(lines, "WorldEventDefinition", "eventId", "displayName", "description", "eventType", "triggerMode", "durationSeconds", "encounters");
            issues += ValidateScriptable<ShopDefinition>(lines, "ShopDefinition", "shopId", "displayName", "description", "stock", "discountRules");
            issues += ValidateScriptable<ResourceNodeDefinition>(lines, "ResourceNodeDefinition", "nodeId", "displayName", "description", "nodeType", "region", "requirement", "maxHarvestsBeforeDepleted", "respawnSeconds", "yields", "markerType");
            issues += ValidateScriptable<EquipmentItemDefinition>(lines, "EquipmentItemDefinition", "equipmentId", "displayName", "description", "sourceItem", "fallbackItemId", "slot", "category", "rarity", "element", "statModifiers", "icon");
            issues += ValidateScriptable<BossDefinition>(lines, "BossDefinition", "bossId", "displayName", "description", "region", "mapPosition", "addMapMarker", "mapMarkerType", "lootTable", "currencyReward", "skillPointReward", "phases", "icon", "introMessage", "defeatMessage");
            issues += ValidateScriptable<SpellCastDefinition>(lines, "SpellCastDefinition", "spellId", "displayName", "description", "ability", "attack", "targetingMode", "resourceType", "resourceCost", "castTimeSeconds", "cooldownSeconds", "interruptible", "queueable", "range", "radius", "targetMask", "icon");
            issues += ValidateScriptable<QuestUiDefinition>(lines, "QuestUiDefinition", "questId", "title", "description", "region", "giverName", "autoTrack", "icon", "objectives", "rewards");

            lines.Add(issues == 0 ? "No missing fields detected." : $"Detected {issues} missing serialized field reference(s). Update the corresponding generator before running it.");

            string dir = "Assets/Elementborn/Generated/Reports";
            if (!AssetDatabase.IsValidFolder("Assets/Elementborn/Generated")) AssetDatabase.CreateFolder("Assets/Elementborn", "Generated");
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/Elementborn/Generated", "Reports");
            string path = dir + "/GeneratorFieldSafetyReport.md";
            File.WriteAllLines(path, lines);
            AssetDatabase.ImportAsset(path);
            Debug.Log($"Wrote {path} with {issues} issue(s).");
        }

        private static int ValidateScriptable<T>(List<string> lines, string label, params string[] fields) where T : ScriptableObject
        {
            int issues = 0;
            T instance = ScriptableObject.CreateInstance<T>();
            var so = new SerializedObject(instance);
            lines.Add($"## {label}");
            foreach (string field in fields)
            {
                if (so.FindProperty(field) == null)
                {
                    lines.Add($"- Missing `{field}`");
                    issues++;
                }
            }

            if (issues == 0)
            {
                lines.Add("- OK");
            }
            lines.Add("");
            Object.DestroyImmediate(instance);
            return issues;
        }
    }
}
#endif
