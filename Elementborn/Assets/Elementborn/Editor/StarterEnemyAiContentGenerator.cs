#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterEnemyAiContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string AiDir = BaseDir + "/EnemyAI";

        [MenuItem("Elementborn/Generate Starter Content/Enemy AI Profiles")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(AiDir);

            CreateProfile("BasicMeleeRaider", "Basic Melee Raider", "Patrols, chases, and performs melee attacks.", EnemyAiArchetype.BasicMelee, AbilityElementType.Neutral, 13f, 2.2f, 8f, 22f, 2f, 4f, 2f, 5f, 1.4f, 2.5f, 0.2f, false, false, false, false);
            CreateProfile("RangedFireCaster", "Ranged Fire Caster", "Keeps distance and uses ranged fire attacks.", EnemyAiArchetype.RangedCaster, AbilityElementType.Fire, 15f, 2f, 13f, 26f, 1.8f, 3.4f, 2.5f, 4f, 2f, 2.2f, 0.2f, false, true, false, false);
            CreateProfile("CowardScout", "Coward Scout", "Flees when low health.", EnemyAiArchetype.Coward, AbilityElementType.Air, 16f, 1.8f, 10f, 28f, 2.2f, 4.2f, 3f, 6f, 1.2f, 2.5f, 0.35f, true, true, false, false);
            CreateProfile("BoatRaider", "Boat Raider", "Attacks boats and keeps broadside range.", EnemyAiArchetype.BoatRaider, AbilityElementType.Neutral, 18f, 3f, 14f, 30f, 2f, 4.2f, 2.5f, 5f, 1.8f, 3f, 0.2f, false, true, true, false);
            CreateProfile("CreatureHunter", "Creature Hunter", "Uses creature pounce behavior and can attack mounts.", EnemyAiArchetype.CreatureHunter, AbilityElementType.Plant, 16f, 3.2f, 8f, 28f, 2.4f, 4.8f, 2f, 5.5f, 1.6f, 3f, 0.25f, false, true, false, true);
            CreateProfile("BossLiteGuardian", "Boss-lite Guardian", "A simple multi-phase enemy profile.", EnemyAiArchetype.BossLite, AbilityElementType.Earth, 20f, 3.0f, 14f, 34f, 1.8f, 4f, 2.5f, 5f, 1.5f, 2.5f, 0.15f, false, true, false, false, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter enemy AI profiles.");
        }

        private static void CreateProfile(
            string id,
            string displayName,
            string description,
            EnemyAiArchetype archetype,
            AbilityElementType element,
            float sight,
            float attackRange,
            float rangedRange,
            float loseRange,
            float patrolSpeed,
            float chaseSpeed,
            float strafeSpeed,
            float fleeSpeed,
            float meleeCooldown,
            float rangedCooldown,
            float fleePercent,
            bool fleeWhenLow,
            bool canStrafe,
            bool canAttackBoats,
            bool canAttackCreatures,
            bool usesBossPhases = false)
        {
            string path = $"{AiDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EnemyCombatProfile>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyCombatProfile>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("profileId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("archetype").enumValueIndex = (int)archetype;
            so.FindProperty("preferredElement").enumValueIndex = (int)element;
            so.FindProperty("sightRange").floatValue = sight;
            so.FindProperty("attackRange").floatValue = attackRange;
            so.FindProperty("rangedAttackRange").floatValue = rangedRange;
            so.FindProperty("loseTargetRange").floatValue = loseRange;
            so.FindProperty("hearingRange").floatValue = 8f;
            so.FindProperty("patrolSpeed").floatValue = patrolSpeed;
            so.FindProperty("chaseSpeed").floatValue = chaseSpeed;
            so.FindProperty("strafeSpeed").floatValue = strafeSpeed;
            so.FindProperty("fleeSpeed").floatValue = fleeSpeed;
            so.FindProperty("stoppingDistance").floatValue = 1.6f;
            so.FindProperty("canStrafe").boolValue = canStrafe;
            so.FindProperty("attackCooldownSeconds").floatValue = meleeCooldown;
            so.FindProperty("rangedCooldownSeconds").floatValue = rangedCooldown;
            so.FindProperty("lowHealthFleePercent").floatValue = fleePercent;
            so.FindProperty("fleeWhenLowHealth").boolValue = fleeWhenLow;
            so.FindProperty("useWeaknesses").boolValue = true;
            so.FindProperty("canAttackBoats").boolValue = canAttackBoats;
            so.FindProperty("canAttackCreatures").boolValue = canAttackCreatures;
            so.FindProperty("usesBossPhases").boolValue = usesBossPhases;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
