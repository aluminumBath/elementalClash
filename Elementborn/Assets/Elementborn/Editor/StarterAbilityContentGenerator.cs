#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterAbilityContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string AbilityDir = BaseDir + "/Abilities";
        private const string SkillTreeDir = BaseDir + "/SkillTrees";

        [MenuItem("Elementborn/Generate Starter Content/Abilities and Skill Trees")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(AbilityDir);
            Directory.CreateDirectory(SkillTreeDir);

            var fireArrow = CreateAbility("FireArrow", "Fire Arrow", "Fire a burning projectile.", AbilityElementType.Fire, AbilityCategory.ActiveCombat, AbilitySlotType.Primary, 1, 1, 18f, 0f, 1.2f, true);
            var waterDash = CreateAbility("WaterDash", "Water Dash", "Dash forward on a surge of water.", AbilityElementType.Water, AbilityCategory.Traversal, AbilitySlotType.Traversal, 1, 1, 0f, 0f, 3.0f, false, isMovement: true);
            var earthShield = CreateAbility("EarthShield", "Earth Shield", "Raise a temporary shield of stone.", AbilityElementType.Earth, AbilityCategory.ActiveCombat, AbilitySlotType.Utility, 2, 1, 8f, 3f, 6.0f, false, isBuff: true);
            var airGlide = CreateAbility("AirGlide", "Air Glide", "Catch the air to slow falls and cross gaps.", AbilityElementType.Air, AbilityCategory.Traversal, AbilitySlotType.Traversal, 2, 1, 0f, 4f, 4.0f, FalseBool(), isMovement: true);
            var boatWindBoost = CreateAbility("BoatWindBoost", "Boat Wind Boost", "Briefly empower your sail with elemental wind.", AbilityElementType.Air, AbilityCategory.Boat, AbilitySlotType.Boat, 2, 1, 0f, 5f, 10.0f, false, isBoat: true);
            var creatureEmpathy = CreateAbility("CreatureEmpathy", "Creature Empathy", "Bond with creatures more quickly.", AbilityElementType.Plant, AbilityCategory.CreatureBond, AbilitySlotType.Passive, 1, 1, 0f, 0f, 0f, false, isPassive: true, isCreature: true);
            var channelerFocus = CreateAbility("ChannelerFocus", "Channeler Focus", "Passive focus that improves elemental control.", AbilityElementType.Neutral, AbilityCategory.Passive, AbilitySlotType.Passive, 1, 1, 0f, 0f, 0f, false, isPassive: true);
            var iceStep = CreateAbility("IceStep", "Ice Step", "Create a brief slick path over water or unstable ground.", AbilityElementType.Ice, AbilityCategory.Traversal, AbilitySlotType.Traversal, 3, 2, 0f, 4f, 7.5f, false, isMovement: true);
            var bloodMend = CreateAbility("BloodMend", "Blood Mend", "Convert stamina into emergency healing.", AbilityElementType.Blood, AbilityCategory.ActiveCombat, AbilitySlotType.Utility, 3, 2, 12f, 0f, 12.0f, false, isHealing: true);
            var steamVeil = CreateAbility("SteamVeil", "Steam Veil", "Create a defensive cloud of steam.", AbilityElementType.Steam, AbilityCategory.ActiveCombat, AbilitySlotType.Secondary, 3, 2, 6f, 3f, 8.0f, false, isArea: true);
            var metalGrapple = CreateAbility("MetalGrapple", "Metal Grapple", "Launch a metal hook toward traversal anchors.", AbilityElementType.Metal, AbilityCategory.Traversal, AbilitySlotType.Traversal, 4, 2, 0f, 0f, 6.0f, true, isMovement: true);
            var stormcall = CreateAbility("Stormcall", "Stormcall", "Ultimate storm burst that calls wind and lightning.", AbilityElementType.Lightning, AbilityCategory.Ultimate, AbilitySlotType.Ultimate, 5, 3, 30f, 6f, 30.0f, false, isArea: true);

            CreateSkillTree("FireTree", "Fire Channeling", AbilityElementType.Fire, fireArrow, steamVeil, stormcall);
            CreateSkillTree("WaterTree", "Water Channeling", AbilityElementType.Water, waterDash, iceStep, bloodMend);
            CreateSkillTree("EarthTree", "Earth Channeling", AbilityElementType.Earth, earthShield, metalGrapple);
            CreateSkillTree("AirTree", "Air Channeling", AbilityElementType.Air, airGlide, boatWindBoost, stormcall);
            CreateSkillTree("BondTree", "Creature Bonding", AbilityElementType.Plant, creatureEmpathy, channelerFocus);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn abilities and skill trees.");
        }

        private static bool FalseBool()
        {
            return false;
        }

        private static AbilityDefinition CreateAbility(
            string id,
            string displayName,
            string description,
            AbilityElementType element,
            AbilityCategory category,
            AbilitySlotType slot,
            int requiredLevel,
            int skillPointCost,
            float power,
            float duration,
            float cooldown,
            bool projectile,
            bool isPassive = false,
            bool isMovement = false,
            bool isArea = false,
            bool isBuff = false,
            bool isHealing = false,
            bool isBoat = false,
            bool isCreature = false)
        {
            string path = $"{AbilityDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AbilityDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("abilityId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("element").enumValueIndex = (int)element;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("defaultSlot").enumValueIndex = (int)slot;
            so.FindProperty("passive").boolValue = isPassive || category == AbilityCategory.Passive;
            so.FindProperty("ultimate").boolValue = category == AbilityCategory.Ultimate;
            so.FindProperty("defaultUnlockSource").enumValueIndex = (int)AbilityUnlockSource.SkillPoints;

            var req = so.FindProperty("unlockRequirement");
            req.FindPropertyRelative("RequiredPlayerLevel").intValue = requiredLevel;
            req.FindPropertyRelative("SkillPointCost").intValue = skillPointCost;

            var effect = so.FindProperty("effect");
            effect.FindPropertyRelative("Power").floatValue = power;
            effect.FindPropertyRelative("Radius").floatValue = isArea ? 4f : 0f;
            effect.FindPropertyRelative("DurationSeconds").floatValue = duration;
            effect.FindPropertyRelative("CooldownSeconds").floatValue = cooldown;
            effect.FindPropertyRelative("ResourceCost").floatValue = 0f;
            effect.FindPropertyRelative("IsProjectile").boolValue = projectile;
            effect.FindPropertyRelative("IsAreaEffect").boolValue = isArea;
            effect.FindPropertyRelative("IsMovement").boolValue = isMovement;
            effect.FindPropertyRelative("IsBuff").boolValue = isBuff;
            effect.FindPropertyRelative("IsHealing").boolValue = isHealing;
            effect.FindPropertyRelative("IsBoatEffect").boolValue = isBoat;
            effect.FindPropertyRelative("IsCreatureEffect").boolValue = isCreature;
            effect.FindPropertyRelative("DebugEffectName").stringValue = id;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void CreateSkillTree(string id, string displayName, AbilityElementType element, params AbilityDefinition[] abilities)
        {
            string path = $"{SkillTreeDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SkillTreeDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("treeId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = $"Starter {displayName} skill tree.";
            so.FindProperty("element").enumValueIndex = (int)element;

            var nodes = so.FindProperty("nodes");
            nodes.arraySize = abilities.Length;
            for (int i = 0; i < abilities.Length; i++)
            {
                var node = nodes.GetArrayElementAtIndex(i);
                node.FindPropertyRelative("NodeId").stringValue = abilities[i] != null ? abilities[i].AbilityId : $"node_{i}";
                node.FindPropertyRelative("Ability").objectReferenceValue = abilities[i];
                node.FindPropertyRelative("UiPosition").vector2Value = new Vector2(i * 180f, 0f);

                var prereqs = node.FindPropertyRelative("PrerequisiteNodeIds");
                prereqs.arraySize = i == 0 ? 0 : 1;
                if (i > 0 && abilities[i - 1] != null)
                {
                    prereqs.GetArrayElementAtIndex(0).stringValue = abilities[i - 1].AbilityId;
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
