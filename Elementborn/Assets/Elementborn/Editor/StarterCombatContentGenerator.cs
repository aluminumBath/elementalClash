#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterCombatContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string ItemDir = BaseDir + "/Items";
        private const string CombatDir = BaseDir + "/Combat";
        private const string StatusDir = CombatDir + "/StatusEffects";
        private const string AttackDir = CombatDir + "/Attacks";
        private const string LootDir = CombatDir + "/LootTables";
        private const string IconDir = "Assets/Elementborn/Art/UI/CombatIcons";

        [MenuItem("Elementborn/Generate Starter Content/Combat and Status")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(StatusDir);
            Directory.CreateDirectory(AttackDir);
            Directory.CreateDirectory(LootDir);

            var burn = CreateStatus("Burn", "Burn", "Ongoing fire damage over time.", StatusEffectType.Burn, AbilityElementType.Fire, 4f, 1f, 3f, 0f, 0f, 0f, "status_burn");
            var wet = CreateStatus("Wet", "Wet", "Makes lightning and ice attacks more effective.", StatusEffectType.Wet, AbilityElementType.Water, 5f, 1f, 0f, -5f, 0f, 0f, "status_wet");
            var chilled = CreateStatus("Chilled", "Chilled", "Slows movement and slightly reduces offense.", StatusEffectType.Chilled, AbilityElementType.Ice, 5f, 1f, 0f, -20f, -5f, 0f, "status_chilled");
            var rooted = CreateStatus("Rooted", "Rooted", "Pins a target in place.", StatusEffectType.Rooted, AbilityElementType.Plant, 3f, 1f, 0f, -100f, 0f, 0f, "status_rooted");
            var shocked = CreateStatus("Shocked", "Shocked", "Arc damage that leaves the target vulnerable.", StatusEffectType.Shocked, AbilityElementType.Lightning, 3f, 1f, 2f, -10f, 0f, -5f, "status_shocked");

            CreateAttack("BasicSwordSlash", "Basic Sword Slash", "A simple close-range melee strike.", AbilityElementType.Neutral, 12f, 0.05f, 1.5f, 2f, AttackOriginType.OnFoot, null, "combat_damage_star");
            CreateAttack("EmberbladeSlash", "Emberblade Slash", "A fiery sword slash that can ignite enemies.", AbilityElementType.Fire, 16f, 0.08f, 1.6f, 2.5f, AttackOriginType.OnFoot, burn, "combat_damage_star");
            CreateAttack("FireballProjectile", "Fireball Projectile", "A ranged fireball attack.", AbilityElementType.Fire, 18f, 0.05f, 1.5f, 1.5f, AttackOriginType.OnFoot, burn, "combat_fireball");
            CreateAttack("SplashShot", "Splash Shot", "A water shot that wets the target.", AbilityElementType.Water, 12f, 0.04f, 1.4f, 1.0f, AttackOriginType.OnFoot, wet, "status_wet");
            CreateAttack("BoatBroadside", "Boat Broadside", "A boat-mounted volley attack.", AbilityElementType.Neutral, 22f, 0.06f, 1.5f, 3f, AttackOriginType.Boat, null, "combat_boat_barrage");
            CreateAttack("CreaturePounce", "Creature Pounce", "A mounted creature pounce attack.", AbilityElementType.Neutral, 20f, 0.08f, 1.6f, 4f, AttackOriginType.Creature, rooted, "combat_creature_pounce");

            CreateLootTable("ReefCrabLoot", "Reef Crab Loot", "Coastal creature drops.",
                Entry("CoralShard", 0.9f, 1, 2),
                Entry("SoftSeaweed", 0.45f, 1, 3));

            CreateLootTable("EmberWispLoot", "Ember Wisp Loot", "Fire creature drops.",
                Entry("EmberCrystal", 0.85f, 1, 2),
                Entry("HealingFruit", 0.15f, 1, 1));

            CreateLootTable("StormlingLoot", "Stormling Loot", "Storm-touched creature drops.",
                Entry("StormScale", 0.8f, 1, 2),
                Entry("CreatureTreat", 0.2f, 1, 1));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn combat/status assets.");
        }

        private static StatusEffectDefinition CreateStatus(string id, string displayName, string description, StatusEffectType type, AbilityElementType element, float duration, float tick, float tickDamage, float movePct, float atkPct, float defPct, string iconName)
        {
            string path = $"{StatusDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<StatusEffectDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StatusEffectDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            so.FindProperty("effectId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("effectType").enumValueIndex = (int)type;
            so.FindProperty("element").enumValueIndex = (int)element;
            so.FindProperty("durationSeconds").floatValue = duration;
            so.FindProperty("tickIntervalSeconds").floatValue = tick;
            so.FindProperty("tickDamage").floatValue = tickDamage;
            so.FindProperty("moveSpeedPercentDelta").floatValue = movePct;
            so.FindProperty("attackPowerPercentDelta").floatValue = atkPct;
            so.FindProperty("defensePercentDelta").floatValue = defPct;
            so.FindProperty("uniquePerTarget").boolValue = true;
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{iconName}.png");
            so.FindProperty("icon").objectReferenceValue = icon;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static CombatAttackDefinition CreateAttack(string id, string displayName, string description, AbilityElementType element, float damage, float critChance, float critMult, float knockback, AttackOriginType origin, StatusEffectDefinition status, string iconName)
        {
            string path = $"{AttackDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<CombatAttackDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CombatAttackDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            so.FindProperty("attackId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("element").enumValueIndex = (int)element;
            so.FindProperty("baseDamage").floatValue = damage;
            so.FindProperty("critChance").floatValue = critChance;
            so.FindProperty("critMultiplier").floatValue = critMult;
            so.FindProperty("knockbackForce").floatValue = knockback;
            so.FindProperty("useEquipmentBonuses").boolValue = true;
            so.FindProperty("originType").enumValueIndex = (int)origin;
            so.FindProperty("statusToApply").objectReferenceValue = status;
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{iconName}.png");
            so.FindProperty("icon").objectReferenceValue = icon;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static LootDropTableDefinition CreateLootTable(string id, string displayName, string description, params StarterLootEntry[] entries)
        {
            string path = $"{LootDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<LootDropTableDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LootDropTableDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            so.FindProperty("tableId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            var arr = so.FindProperty("entries");
            arr.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                var item = AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>($"{ItemDir}/{entries[i].ItemId}.asset");
                var dst = arr.GetArrayElementAtIndex(i);
                dst.FindPropertyRelative("Item").objectReferenceValue = item;
                dst.FindPropertyRelative("FallbackItemId").stringValue = entries[i].ItemId;
                dst.FindPropertyRelative("Chance").floatValue = entries[i].Chance;
                dst.FindPropertyRelative("MinQuantity").intValue = entries[i].Min;
                dst.FindPropertyRelative("MaxQuantity").intValue = entries[i].Max;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static StarterLootEntry Entry(string itemId, float chance, int min, int max)
        {
            return new StarterLootEntry { ItemId = itemId, Chance = chance, Min = min, Max = max };
        }

        private struct StarterLootEntry
        {
            public string ItemId;
            public float Chance;
            public int Min;
            public int Max;
        }
    }
}
#endif
