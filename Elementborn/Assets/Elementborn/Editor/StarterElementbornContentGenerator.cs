#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    /// <summary>
    /// Generates starter item, recipe, and creature ScriptableObject assets for prototyping.
    /// </summary>
    public static class StarterElementbornContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string ItemDir = BaseDir + "/Items";
        private const string RecipeDir = BaseDir + "/Recipes";
        private const string CreatureDir = BaseDir + "/Creatures";

        [MenuItem("Elementborn/Generate Starter Content/Items, Recipes, Creatures")]
        public static void GenerateAll()
        {
            EnsureFolders();

            var healingFruit = CreateItem("HealingFruit", "Healing Fruit", "A sweet fruit that restores energy.", InventoryItemCategory.Food, InventoryItemRarity.Common, true, 99, 3, true, true, "Eat");
            var waterMint = CreateItem("WaterMint", "Water Mint", "A cooling herb common near clear springs.", InventoryItemCategory.Ingredient, InventoryItemRarity.Common, true, 99, 2, false, true, "Use");
            var coralShard = CreateItem("CoralShard", "Coral Shard", "A hard shard harvested from reef growth.", InventoryItemCategory.Material, InventoryItemRarity.Uncommon, true, 99, 6, false, true, "Use");
            var softSeaweed = CreateItem("SoftSeaweed", "Soft Seaweed", "Flexible seaweed useful for bandages and meals.", InventoryItemCategory.Ingredient, InventoryItemRarity.Common, true, 99, 2, false, true, "Use");
            var boatPlank = CreateItem("BoatPlank", "Boat Plank", "A treated plank for boat repairs.", InventoryItemCategory.BoatPart, InventoryItemRarity.Common, true, 20, 8, false, true, "Use");
            var sailcloth = CreateItem("Sailcloth", "Sailcloth", "Sturdy cloth for patching sails.", InventoryItemCategory.BoatPart, InventoryItemRarity.Common, true, 20, 8, false, true, "Use");
            var windThread = CreateItem("WindThread", "Wind Thread", "Thread that flutters even in still air.", InventoryItemCategory.Material, InventoryItemRarity.Rare, true, 20, 20, false, true, "Use");
            var stormScale = CreateItem("StormScale", "Storm Scale", "A scale left by storm-touched creatures.", InventoryItemCategory.CreatureItem, InventoryItemRarity.Rare, true, 20, 30, false, true, "Use");
            var creatureTreatBase = CreateItem("CreatureTreatBase", "Creature Treat Base", "A plain base for creature treats.", InventoryItemCategory.Ingredient, InventoryItemRarity.Common, true, 99, 3, false, true, "Use");
            var emberCrystal = CreateItem("EmberCrystal", "Ember Crystal", "A warm crystal used for fire-infused crafting.", InventoryItemCategory.Material, InventoryItemRarity.Uncommon, true, 50, 12, false, true, "Use");
            var seaSalt = CreateItem("SeaSalt", "Sea Salt", "Mineral-rich salt from coastal flats.", InventoryItemCategory.Ingredient, InventoryItemRarity.Common, true, 99, 1, false, true, "Use");

            var healingTea = CreateItem("HealingFruitTea", "Healing Fruit Tea", "A warm tea that restores health.", InventoryItemCategory.Consumable, InventoryItemRarity.Uncommon, true, 10, 15, true, true, "Drink");
            var coralBandage = CreateItem("CoralBandage", "Coral Bandage", "A reef-made bandage for emergency healing.", InventoryItemCategory.Consumable, InventoryItemRarity.Uncommon, true, 10, 18, true, true, "Use");
            var boatRepairKit = CreateItem("BoatRepairKit", "Boat Repair Kit", "Basic repair supplies for a damaged boat.", InventoryItemCategory.BoatPart, InventoryItemRarity.Uncommon, true, 5, 25, true, true, "Repair");
            var windSailPatch = CreateItem("WindSailPatch", "Wind-Sail Patch", "A patch that helps a sail catch favorable wind.", InventoryItemCategory.BoatPart, InventoryItemRarity.Rare, true, 5, 40, true, true, "Patch");
            var stormLure = CreateItem("StormLure", "Storm Lure", "A lure that attracts storm-loving creatures.", InventoryItemCategory.CreatureItem, InventoryItemRarity.Rare, true, 10, 35, true, true, "Use");
            var creatureTreat = CreateItem("CreatureTreat", "Creature Treat", "A simple treat used to tame and bond with creatures.", InventoryItemCategory.CreatureItem, InventoryItemRarity.Common, true, 20, 8, true, true, "Feed");
            var elementalArrows = CreateItem("ElementalArrowBundle", "Elemental Arrow Bundle", "A bundle of arrows suitable for elemental attacks.", InventoryItemCategory.Tool, InventoryItemRarity.Uncommon, true, 20, 20, true, true, "Equip");
            var reefStew = CreateItem("SimpleReefStew", "Simple Reef Stew", "A basic meal made from shore ingredients.", InventoryItemCategory.Food, InventoryItemRarity.Common, true, 10, 10, true, true, "Eat");
            var stableFeed = CreateItem("StableFeed", "Stable Feed", "A hearty feed for resting creatures.", InventoryItemCategory.CreatureItem, InventoryItemRarity.Common, true, 20, 6, true, true, "Feed");
            var clarityTonic = CreateItem("ClarityTonic", "Clarity Tonic", "A tonic that steadies the mind.", InventoryItemCategory.Consumable, InventoryItemRarity.Uncommon, true, 10, 18, true, true, "Drink");

            CreateItem("PearlCompass", "Pearl Compass", "A quest item said to point toward hidden reef gates.", InventoryItemCategory.Quest, InventoryItemRarity.Quest, false, 1, 0, false, false, "Use", true, true);
            CreateItem("Emberblade", "Emberblade", "A fire-touched blade suitable for early tests.", InventoryItemCategory.Weapon, InventoryItemRarity.Rare, false, 1, 80, false, true, "Equip", false, true);

            CreateRecipe("HealingFruitTea", "Healing Fruit Tea", "Brew a restorative tea.", CraftingRecipeCategory.Cooking, CraftingStationType.Campfire, healingTea, 1,
                new Ingredient(healingFruit, 1), new Ingredient(waterMint, 1));

            CreateRecipe("CoralBandage", "Coral Bandage", "Create a flexible healing bandage.", CraftingRecipeCategory.Alchemy, CraftingStationType.Workbench, coralBandage, 1,
                new Ingredient(coralShard, 1), new Ingredient(softSeaweed, 2));

            CreateRecipe("BoatRepairKit", "Boat Repair Kit", "Bundle basic boat repair supplies.", CraftingRecipeCategory.BoatRepair, CraftingStationType.Workbench, boatRepairKit, 1,
                new Ingredient(boatPlank, 2), new Ingredient(coralShard, 1));

            CreateRecipe("WindSailPatch", "Wind-Sail Patch", "Patch a sail with wind-catching thread.", CraftingRecipeCategory.BoatRepair, CraftingStationType.BoatDock, windSailPatch, 1,
                new Ingredient(sailcloth, 2), new Ingredient(windThread, 1));

            CreateRecipe("StormLure", "Storm Lure", "Craft a lure for storm-loving creatures.", CraftingRecipeCategory.CreatureCare, CraftingStationType.Stable, stormLure, 1,
                new Ingredient(stormScale, 1), new Ingredient(coralShard, 1), new Ingredient(waterMint, 1));

            CreateRecipe("CreatureTreat", "Creature Treat", "Make treats for taming and bonding.", CraftingRecipeCategory.CreatureCare, CraftingStationType.Campfire, creatureTreat, 2,
                new Ingredient(creatureTreatBase, 1), new Ingredient(seaSalt, 1), new Ingredient(healingFruit, 1));

            CreateRecipe("ElementalArrowBundle", "Elemental Arrow Bundle", "Prepare arrows that can carry elemental force.", CraftingRecipeCategory.Ammunition, CraftingStationType.Workbench, elementalArrows, 6,
                new Ingredient(emberCrystal, 1), new Ingredient(windThread, 1));

            CreateRecipe("SimpleReefStew", "Simple Reef Stew", "Cook a simple coastal meal.", CraftingRecipeCategory.Cooking, CraftingStationType.Campfire, reefStew, 1,
                new Ingredient(healingFruit, 1), new Ingredient(softSeaweed, 1), new Ingredient(seaSalt, 1));

            CreateRecipe("StableFeed", "Stable Feed", "Prepare feed for resting creatures.", CraftingRecipeCategory.CreatureCare, CraftingStationType.Stable, stableFeed, 3,
                new Ingredient(creatureTreatBase, 2), new Ingredient(seaSalt, 1));

            CreateRecipe("ClarityTonic", "Clarity Tonic", "Distill a tonic for focus and clarity.", CraftingRecipeCategory.Alchemy, CraftingStationType.AlchemyTable, clarityTonic, 1,
                new Ingredient(waterMint, 2), new Ingredient(coralShard, 1));

            CreateCreature("Skyotter", "Skyotter", "An amphibious storm-loving creature that can swim, glide, and ride turbulent winds.", CreatureTraversalType.Amphibious, CreatureTemperament.Playful, 35, "CreatureTreat", "StormLure");
            CreateCreature("MossWolf", "Moss Wolf", "A loyal forest predator that bonds well with patient handlers.", CreatureTraversalType.Land, CreatureTemperament.Gentle, 25, "CreatureTreat", "StableFeed");
            CreateCreature("Thunderbird", "Thunderbird", "A proud flying creature tied to storm currents.", CreatureTraversalType.Flying, CreatureTemperament.Proud, 60, "StormLure");
            CreateCreature("TealSerpent", "Teal Serpent", "A swimming serpent that favors reef channels and quiet offerings.", CreatureTraversalType.Swimming, CreatureTemperament.Wise, 50, "StormLure", "CreatureTreat");
            CreateCreature("EarthMole", "Earth Mole", "A burrowing creature that can sense movement through stone.", CreatureTraversalType.Burrowing, CreatureTemperament.Stubborn, 40, "StableFeed");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated Elementborn starter items, recipes, and creatures.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ItemDir);
            Directory.CreateDirectory(RecipeDir);
            Directory.CreateDirectory(CreatureDir);
        }

        private static InventoryItemDefinition CreateItem(
            string id,
            string displayName,
            string description,
            InventoryItemCategory category,
            InventoryItemRarity rarity,
            bool stackable,
            int maxStack,
            int baseValue,
            bool usable,
            bool sellable,
            string useVerb,
            bool questItem = false,
            bool important = false)
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
            so.FindProperty("questItem").boolValue = questItem;
            so.FindProperty("important").boolValue = important;
            so.FindProperty("stackable").boolValue = stackable;
            so.FindProperty("maxStack").intValue = maxStack;
            so.FindProperty("baseValue").intValue = baseValue;
            so.FindProperty("sellable").boolValue = sellable;
            so.FindProperty("usable").boolValue = usable;
            so.FindProperty("consumedOnUse").boolValue = usable;
            so.FindProperty("useVerb").stringValue = useVerb;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static CraftingRecipeDefinition CreateRecipe(
            string id,
            string displayName,
            string description,
            CraftingRecipeCategory category,
            CraftingStationType station,
            InventoryItemDefinition output,
            int outputQuantity,
            params Ingredient[] ingredients)
        {
            string path = $"{RecipeDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<CraftingRecipeDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CraftingRecipeDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("recipeId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("requiredStation").enumValueIndex = (int)station;
            so.FindProperty("knownByDefault").boolValue = true;
            so.FindProperty("importantRecipe").boolValue = category == CraftingRecipeCategory.BoatRepair || category == CraftingRecipeCategory.CreatureCare;
            so.FindProperty("currencyCost").intValue = 0;
            so.FindProperty("outputItem").objectReferenceValue = output;
            so.FindProperty("fallbackOutputItemId").stringValue = output != null ? output.ItemId : id;
            so.FindProperty("outputQuantity").intValue = outputQuantity;
            so.FindProperty("addJournalEntryOnCraft").boolValue = true;

            var ingredientProp = so.FindProperty("ingredients");
            ingredientProp.arraySize = ingredients.Length;
            for (int i = 0; i < ingredients.Length; i++)
            {
                var element = ingredientProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Item").objectReferenceValue = ingredients[i].Item;
                element.FindPropertyRelative("FallbackItemId").stringValue = ingredients[i].Item != null ? ingredients[i].Item.ItemId : ingredients[i].FallbackItemId;
                element.FindPropertyRelative("Quantity").intValue = Mathf.Max(1, ingredients[i].Quantity);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static CreatureDefinition CreateCreature(
            string id,
            string displayName,
            string description,
            CreatureTraversalType traversal,
            CreatureTemperament temperament,
            int difficulty,
            params string[] favoriteTreats)
        {
            string path = $"{CreatureDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<CreatureDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CreatureDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("creatureId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("traversalType").enumValueIndex = (int)traversal;
            so.FindProperty("temperament").enumValueIndex = (int)temperament;
            so.FindProperty("tameDifficulty").intValue = difficulty;
            so.FindProperty("baseBondGain").intValue = 8;

            var treats = so.FindProperty("favoriteTreatItemIds");
            treats.arraySize = favoriteTreats.Length;
            for (int i = 0; i < favoriteTreats.Length; i++)
            {
                treats.GetArrayElementAtIndex(i).stringValue = favoriteTreats[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private readonly struct Ingredient
        {
            public readonly InventoryItemDefinition Item;
            public readonly string FallbackItemId;
            public readonly int Quantity;

            public Ingredient(InventoryItemDefinition item, int quantity)
            {
                Item = item;
                FallbackItemId = item != null ? item.ItemId : "";
                Quantity = quantity;
            }

            public Ingredient(string fallbackItemId, int quantity)
            {
                Item = null;
                FallbackItemId = fallbackItemId;
                Quantity = quantity;
            }
        }
    }
}
#endif
