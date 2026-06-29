#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterSpellContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string SpellDir = BaseDir + "/Spells";
        private const string AbilityDir = BaseDir + "/Abilities";
        private const string AttackDir = BaseDir + "/Combat/Attacks";
        private const string IconDir = "Assets/Elementborn/Art/UI/SpellIcons";

        [MenuItem("Elementborn/Generate Starter Content/Spells and Cooldown UI")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(SpellDir);

            CreateSpell("Fireball", "Fireball", "Launch a fire projectile that can burn enemies.", "FireArrow", "FireballProjectile", SpellTargetingMode.ForwardProjectile, SpellResourceType.Focus, 15f, 0.45f, 3f, 18f, 0f, "spell_fireball");
            CreateSpell("WaterSplash", "Water Splash", "A quick water spell that wets targets.", "WaterDash", "SplashShot", SpellTargetingMode.ForwardProjectile, SpellResourceType.Focus, 10f, 0.2f, 2f, 16f, 0f, "spell_water_splash");
            CreateSpell("EarthRoot", "Earth Root", "Root enemies in a small area.", "EarthShield", "CreaturePounce", SpellTargetingMode.GroundAoe, SpellResourceType.Focus, 18f, 0.75f, 6f, 14f, 3.5f, "spell_earth_root");
            CreateSpell("AirGust", "Air Gust", "A fast cone of wind that knocks enemies back.", "AirGlide", "BoatBroadside", SpellTargetingMode.Cone, SpellResourceType.Stamina, 20f, 0.15f, 3f, 10f, 4f, "spell_air_gust");
            CreateSpell("IceShard", "Ice Shard", "A precise ice projectile.", "IceStep", "SplashShot", SpellTargetingMode.ForwardProjectile, SpellResourceType.Focus, 14f, 0.35f, 3f, 20f, 0f, "spell_ice_shard");
            CreateSpell("StormBolt", "Storm Bolt", "A lightning strike that is stronger against wet enemies.", "Stormcall", "FireballProjectile", SpellTargetingMode.TargetedUnit, SpellResourceType.Focus, 30f, 1.0f, 9f, 22f, 0f, "spell_storm_bolt");
            CreateSpell("HealingBloom", "Healing Bloom", "A self-cast healing bloom placeholder.", "ChannelerFocus", "BasicSwordSlash", SpellTargetingMode.Self, SpellResourceType.Focus, 22f, 0.8f, 10f, 0f, 0f, "spell_healing_bloom");
            CreateSpell("BoatWindBurst", "Boat Wind Burst", "Empower a boat sail with a burst of wind.", "BoatWindBoost", "BoatBroadside", SpellTargetingMode.BoatAssist, SpellResourceType.Stamina, 25f, 0.4f, 12f, 0f, 0f, "spell_boat_wind_burst");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn spells.");
        }

        private static void CreateSpell(
            string id,
            string displayName,
            string description,
            string abilityId,
            string attackId,
            SpellTargetingMode targeting,
            SpellResourceType resource,
            float cost,
            float castTime,
            float cooldown,
            float range,
            float radius,
            string iconName)
        {
            string path = $"{SpellDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SpellCastDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SpellCastDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("spellId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;

            var ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>($"{AbilityDir}/{abilityId}.asset");
            var attack = AssetDatabase.LoadAssetAtPath<CombatAttackDefinition>($"{AttackDir}/{attackId}.asset");
            var icon = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{iconName}.png");

            so.FindProperty("ability").objectReferenceValue = ability;
            so.FindProperty("attack").objectReferenceValue = attack;
            so.FindProperty("targetingMode").enumValueIndex = (int)targeting;
            so.FindProperty("resourceType").enumValueIndex = (int)resource;
            so.FindProperty("resourceCost").floatValue = cost;
            so.FindProperty("castTimeSeconds").floatValue = castTime;
            so.FindProperty("cooldownSeconds").floatValue = cooldown;
            so.FindProperty("interruptible").boolValue = true;
            so.FindProperty("queueable").boolValue = true;
            so.FindProperty("range").floatValue = range;
            so.FindProperty("radius").floatValue = radius;
            so.FindProperty("icon").objectReferenceValue = icon;
            so.FindProperty("skillPointRewardOnFirstCast").intValue = 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
