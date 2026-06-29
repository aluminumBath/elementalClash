#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterEquipmentContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string ItemDir = BaseDir + "/Items";
        private const string EquipmentDir = BaseDir + "/Equipment";
        private const string SetDir = BaseDir + "/EquipmentSets";
        private const string IconDir = "Assets/Elementborn/Art/UI/EquipmentIcons";

        [MenuItem("Elementborn/Generate Starter Content/Equipment and Gear")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(ItemDir);
            Directory.CreateDirectory(EquipmentDir);
            Directory.CreateDirectory(SetDir);

            var basicSwordItem = CreateItem("BasicSword", "Basic Sword", "A reliable starter sword.", InventoryItemCategory.Weapon, InventoryItemRarity.Common, 20);
            var emberbladeItem = CreateExistingOrItem("Emberblade", "Emberblade", "A fire-touched blade suitable for early tests.", InventoryItemCategory.Weapon, InventoryItemRarity.Rare, 80);
            var coralShieldItem = CreateItem("CoralShield", "Coral Shield", "A shield carved from hardened reef coral.", InventoryItemCategory.Armor, InventoryItemRarity.Uncommon, 35);
            var gliderCloakItem = CreateItem("GliderCloak", "Glider Cloak", "A light cloak that catches wind currents.", InventoryItemCategory.Armor, InventoryItemRarity.Rare, 60);
            var channelerFocusItem = CreateItem("ChannelerFocus", "Channeler Focus", "A focus crystal for controlled elemental channeling.", InventoryItemCategory.Tool, InventoryItemRarity.Uncommon, 45);
            var reefAmuletItem = CreateItem("ReefAmulet", "Reef Amulet", "An amulet favored by Neritha water channelers.", InventoryItemCategory.Trinket, InventoryItemRarity.Uncommon, 30);
            var windRingItem = CreateItem("WindRing", "Wind Ring", "A ring etched with air-channeler spiral marks.", InventoryItemCategory.Trinket, InventoryItemRarity.Rare, 50);
            var basicArmorItem = CreateItem("BasicArmor", "Basic Armor", "Simple protective armor for early encounters.", InventoryItemCategory.Armor, InventoryItemRarity.Common, 35);
            var travelBootsItem = CreateItem("TravelBoots", "Travel Boots", "Comfortable boots for long roads.", InventoryItemCategory.Armor, InventoryItemRarity.Common, 20);
            var repairHammerItem = CreateItem("RepairHammer", "Repair Hammer", "A sturdy hammer useful around docks and camps.", InventoryItemCategory.Tool, InventoryItemRarity.Common, 18);

            var pickaxeItem = CreateExistingOrItem("Pickaxe", "Pickaxe", "A simple tool for mining crystals and ore.", InventoryItemCategory.Tool, InventoryItemRarity.Common, 20);
            var sickleItem = CreateExistingOrItem("Sickle", "Sickle", "A curved tool for harvesting plants and seaweed.", InventoryItemCategory.Tool, InventoryItemRarity.Common, 15);
            var harvestKnifeItem = CreateExistingOrItem("HarvestKnife", "Harvest Knife", "A small knife for careful harvesting.", InventoryItemCategory.Tool, InventoryItemRarity.Common, 12);
            var fishingRodItem = CreateExistingOrItem("FishingRod", "Fishing Rod", "A basic rod for fishing spots.", InventoryItemCategory.Tool, InventoryItemRarity.Common, 18);
            var gatheringNetItem = CreateExistingOrItem("GatheringNet", "Gathering Net", "A net for aquatic harvesting.", InventoryItemCategory.Tool, InventoryItemRarity.Common, 16);

            var boatSailPatchItem = CreateItem("BoatSailPatchUpgrade", "Boat Sail Patch Upgrade", "A reusable sail upgrade inspired by wind-thread patches.", InventoryItemCategory.BoatPart, InventoryItemRarity.Uncommon, 45);
            var boatKeelPlatesItem = CreateItem("BoatKeelPlates", "Boat Keel Plates", "Protective plates that steady a boat in rough water.", InventoryItemCategory.BoatPart, InventoryItemRarity.Uncommon, 45);
            var creatureSaddleItem = CreateItem("CreatureSaddle", "Creature Saddle", "A comfortable saddle for rideable creatures.", InventoryItemCategory.CreatureItem, InventoryItemRarity.Common, 35);
            var stormTackItem = CreateItem("StormTack", "Storm Tack", "Harness gear for storm-loving creatures.", InventoryItemCategory.CreatureItem, InventoryItemRarity.Rare, 75);

            var basicSword = CreateEquipment("BasicSword", "Basic Sword", "A reliable starter sword.", basicSwordItem, EquipmentSlotType.MainHand, EquipmentCategory.Weapon, InventoryItemRarity.Common, AbilityElementType.Neutral, "eq_basic_sword",
                Mod(GearStatType.AttackPower, 5f, 0f));

            var emberblade = CreateEquipment("Emberblade", "Emberblade", "A fire-touched blade that boosts fire channeling.", emberbladeItem, EquipmentSlotType.MainHand, EquipmentCategory.Weapon, InventoryItemRarity.Rare, AbilityElementType.Fire, "eq_emberblade",
                Mod(GearStatType.AttackPower, 9f, 0f),
                Mod(GearStatType.FirePower, 4f, 8f));

            var coralShield = CreateEquipment("CoralShield", "Coral Shield", "A reef shield that favors water defense.", coralShieldItem, EquipmentSlotType.OffHand, EquipmentCategory.Shield, InventoryItemRarity.Uncommon, AbilityElementType.Water, "eq_coral_shield",
                Mod(GearStatType.Defense, 8f, 0f),
                Mod(GearStatType.WaterPower, 2f, 0f));

            var gliderCloak = CreateEquipment("GliderCloak", "Glider Cloak", "A cloak that improves air control and gliding.", gliderCloakItem, EquipmentSlotType.Cloak, EquipmentCategory.Cloak, InventoryItemRarity.Rare, AbilityElementType.Air, "eq_glider_cloak",
                Mod(GearStatType.GlideControl, 8f, 10f),
                Mod(GearStatType.AirPower, 2f, 0f));

            var channelerFocus = CreateEquipment("ChannelerFocus", "Channeler Focus", "A focus crystal for elemental power and cooldown control.", channelerFocusItem, EquipmentSlotType.Focus, EquipmentCategory.ChannelerFocus, InventoryItemRarity.Uncommon, AbilityElementType.Mixed, "eq_channeler_focus",
                Mod(GearStatType.ElementPower, 5f, 5f),
                Mod(GearStatType.CooldownReduction, 0f, 4f));

            CreateEquipment("Pickaxe", "Pickaxe", "A mining tool that slightly improves harvesting speed.", pickaxeItem, EquipmentSlotType.Tool, EquipmentCategory.GatheringTool, InventoryItemRarity.Common, AbilityElementType.Earth, "eq_pickaxe",
                Mod(GearStatType.HarvestSpeed, 2f, 0f));

            CreateEquipment("Sickle", "Sickle", "A harvesting tool that improves plant gathering.", sickleItem, EquipmentSlotType.Tool, EquipmentCategory.GatheringTool, InventoryItemRarity.Common, AbilityElementType.Plant, "eq_sickle",
                Mod(GearStatType.HarvestSpeed, 2f, 0f));

            CreateEquipment("HarvestKnife", "Harvest Knife", "A careful harvesting knife for wreckage and coral work.", harvestKnifeItem, EquipmentSlotType.Tool, EquipmentCategory.GatheringTool, InventoryItemRarity.Common, AbilityElementType.Neutral, "eq_harvest_knife",
                Mod(GearStatType.RareHarvestChance, 0f, 2f));

            CreateEquipment("FishingRod", "Fishing Rod", "A rod for fishing spots.", fishingRodItem, EquipmentSlotType.Tool, EquipmentCategory.GatheringTool, InventoryItemRarity.Common, AbilityElementType.Water, "eq_fishing_rod",
                Mod(GearStatType.RareHarvestChance, 0f, 2f));

            CreateEquipment("GatheringNet", "Gathering Net", "A net for aquatic harvesting.", gatheringNetItem, EquipmentSlotType.Tool, EquipmentCategory.GatheringTool, InventoryItemRarity.Common, AbilityElementType.Water, "eq_gathering_net",
                Mod(GearStatType.HarvestSpeed, 1f, 0f),
                Mod(GearStatType.RareHarvestChance, 0f, 1f));

            CreateEquipment("BoatSailPatchUpgrade", "Boat Sail Patch Upgrade", "A boat gear upgrade that improves wind speed.", boatSailPatchItem, EquipmentSlotType.BoatUpgrade, EquipmentCategory.BoatGear, InventoryItemRarity.Uncommon, AbilityElementType.Air, "eq_boat_sail_patch",
                Mod(GearStatType.BoatSpeed, 4f, 8f));

            CreateEquipment("BoatKeelPlates", "Boat Keel Plates", "Boat gear that improves handling and defense.", boatKeelPlatesItem, EquipmentSlotType.BoatUpgrade, EquipmentCategory.BoatGear, InventoryItemRarity.Uncommon, AbilityElementType.Metal, "eq_boat_keel_plates",
                Mod(GearStatType.BoatHandling, 4f, 8f),
                Mod(GearStatType.Defense, 2f, 0f));

            CreateEquipment("CreatureSaddle", "Creature Saddle", "Basic tack for rideable creatures.", creatureSaddleItem, EquipmentSlotType.CreatureGear, EquipmentCategory.CreatureTack, InventoryItemRarity.Common, AbilityElementType.Neutral, "eq_creature_saddle",
                Mod(GearStatType.CreatureBondGain, 0f, 5f));

            CreateEquipment("StormTack", "Storm Tack", "Creature tack that improves bonding with storm-loving creatures.", stormTackItem, EquipmentSlotType.CreatureGear, EquipmentCategory.CreatureTack, InventoryItemRarity.Rare, AbilityElementType.Lightning, "eq_storm_tack",
                Mod(GearStatType.CreatureBondGain, 0f, 12f),
                Mod(GearStatType.AirPower, 2f, 0f));

            CreateEquipment("ReefAmulet", "Reef Amulet", "A water-aspected amulet with defensive properties.", reefAmuletItem, EquipmentSlotType.Amulet, EquipmentCategory.Jewelry, InventoryItemRarity.Uncommon, AbilityElementType.Water, "eq_reef_amulet",
                Mod(GearStatType.WaterPower, 3f, 0f),
                Mod(GearStatType.MaxHealth, 10f, 0f));

            CreateEquipment("WindRing", "Wind Ring", "A ring that helps channel air movement.", windRingItem, EquipmentSlotType.Ring, EquipmentCategory.Jewelry, InventoryItemRarity.Rare, AbilityElementType.Air, "eq_wind_ring",
                Mod(GearStatType.MoveSpeed, 0f, 4f),
                Mod(GearStatType.AirPower, 3f, 0f));

            CreateEquipment("BasicArmor", "Basic Armor", "Simple protective armor.", basicArmorItem, EquipmentSlotType.Chest, EquipmentCategory.Armor, InventoryItemRarity.Common, AbilityElementType.Neutral, "eq_basic_armor",
                Mod(GearStatType.Defense, 6f, 0f),
                Mod(GearStatType.MaxHealth, 8f, 0f));

            CreateEquipment("TravelBoots", "Travel Boots", "Boots that improve travel speed.", travelBootsItem, EquipmentSlotType.Feet, EquipmentCategory.Armor, InventoryItemRarity.Common, AbilityElementType.Neutral, "eq_travel_boots",
                Mod(GearStatType.MoveSpeed, 0f, 3f));

            CreateEquipment("RepairHammer", "Repair Hammer", "A dock tool that helps with boat repairs.", repairHammerItem, EquipmentSlotType.Tool, EquipmentCategory.GatheringTool, InventoryItemRarity.Common, AbilityElementType.Metal, "eq_repair_hammer",
                Mod(GearStatType.BoatHandling, 1f, 0f));

            CreateSetBonus("ReefGuardianSet", "Reef Guardian Set", "A defensive set for water-channeler regions.", new[] { "CoralShield", "ReefAmulet" },
                Mod(GearStatType.Defense, 3f, 5f),
                Mod(GearStatType.WaterPower, 2f, 0f));

            CreateSetBonus("WindrunnerSet", "Windrunner Set", "A mobility set for air traversal.", new[] { "GliderCloak", "WindRing" },
                Mod(GearStatType.MoveSpeed, 0f, 4f),
                Mod(GearStatType.GlideControl, 3f, 5f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn equipment, gear items, and set bonuses.");
        }

        private static InventoryItemDefinition CreateExistingOrItem(string id, string displayName, string description, InventoryItemCategory category, InventoryItemRarity rarity, int value)
        {
            string path = $"{ItemDir}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>(path);
            return existing != null ? existing : CreateItem(id, displayName, description, category, rarity, value);
        }

        private static InventoryItemDefinition CreateItem(string id, string displayName, string description, InventoryItemCategory category, InventoryItemRarity rarity, int value)
        {
            string path = $"{ItemDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<InventoryItemDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("itemId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("rarity").enumValueIndex = (int)rarity;
            so.FindProperty("questItem").boolValue = false;
            so.FindProperty("important").boolValue = rarity >= InventoryItemRarity.Rare;
            so.FindProperty("stackable").boolValue = false;
            so.FindProperty("maxStack").intValue = 1;
            so.FindProperty("baseValue").intValue = value;
            so.FindProperty("sellable").boolValue = true;
            so.FindProperty("usable").boolValue = false;
            so.FindProperty("consumedOnUse").boolValue = false;
            so.FindProperty("useVerb").stringValue = "Equip";
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EquipmentItemDefinition CreateEquipment(
            string id,
            string displayName,
            string description,
            InventoryItemDefinition item,
            EquipmentSlotType slot,
            EquipmentCategory category,
            InventoryItemRarity rarity,
            AbilityElementType element,
            string iconName,
            params GearStatModifier[] modifiers)
        {
            string path = $"{EquipmentDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EquipmentItemDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EquipmentItemDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("equipmentId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("sourceItem").objectReferenceValue = item;
            so.FindProperty("fallbackItemId").stringValue = item != null ? item.ItemId : id;
            so.FindProperty("slot").enumValueIndex = (int)slot;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("rarity").enumValueIndex = (int)rarity;
            so.FindProperty("element").enumValueIndex = (int)element;
            so.FindProperty("requiredLevel").intValue = 1;
            so.FindProperty("requiredAbilityId").stringValue = "";
            so.FindProperty("requiredItemId").stringValue = "";
            so.FindProperty("removesFromInventoryWhenEquipped").boolValue = false;
            so.FindProperty("returnsToInventoryWhenUnequipped").boolValue = false;

            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{iconName}.png");
            so.FindProperty("icon").objectReferenceValue = icon;

            var mods = so.FindProperty("statModifiers");
            mods.arraySize = modifiers.Length;
            for (int i = 0; i < modifiers.Length; i++)
            {
                var dst = mods.GetArrayElementAtIndex(i);
                dst.FindPropertyRelative("Stat").enumValueIndex = (int)modifiers[i].Stat;
                dst.FindPropertyRelative("FlatValue").floatValue = modifiers[i].FlatValue;
                dst.FindPropertyRelative("PercentValue").floatValue = modifiers[i].PercentValue;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EquipmentSetBonusDefinition CreateSetBonus(string id, string displayName, string description, string[] equipmentIds, params GearStatModifier[] modifiers)
        {
            string path = $"{SetDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EquipmentSetBonusDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EquipmentSetBonusDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("setId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;

            var ids = so.FindProperty("requiredEquipmentIds");
            ids.arraySize = equipmentIds.Length;
            for (int i = 0; i < equipmentIds.Length; i++)
            {
                ids.GetArrayElementAtIndex(i).stringValue = equipmentIds[i];
            }

            var mods = so.FindProperty("bonusModifiers");
            mods.arraySize = modifiers.Length;
            for (int i = 0; i < modifiers.Length; i++)
            {
                var dst = mods.GetArrayElementAtIndex(i);
                dst.FindPropertyRelative("Stat").enumValueIndex = (int)modifiers[i].Stat;
                dst.FindPropertyRelative("FlatValue").floatValue = modifiers[i].FlatValue;
                dst.FindPropertyRelative("PercentValue").floatValue = modifiers[i].PercentValue;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static GearStatModifier Mod(GearStatType stat, float flat, float percent)
        {
            return new GearStatModifier
            {
                Stat = stat,
                FlatValue = flat,
                PercentValue = percent
            };
        }
    }
}
#endif
