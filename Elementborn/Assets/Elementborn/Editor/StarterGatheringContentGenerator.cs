#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterGatheringContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string NodeDir = BaseDir + "/Gathering";
        private const string ItemDir = BaseDir + "/Items";

        [MenuItem("Elementborn/Generate Starter Content/Gathering Resources")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(NodeDir);

            CreateToolItem("Pickaxe", "Pickaxe", "A simple tool for mining crystals and ore.", 20);
            CreateToolItem("Sickle", "Sickle", "A curved tool for harvesting plants and seaweed.", 15);
            CreateToolItem("FishingRod", "Fishing Rod", "A basic rod for fishing spots.", 18);
            CreateToolItem("HarvestKnife", "Harvest Knife", "A small knife for careful harvesting.", 12);
            CreateToolItem("GatheringNet", "Gathering Net", "A net for catching small aquatic resources.", 16);

            CreateNode(
                "CoralShardNode",
                "Coral Shard Outcrop",
                "A reef outcrop containing harvestable coral shards.",
                ResourceNodeType.Coral,
                "Neritha Reefwood",
                HarvestToolType.Chisel,
                "HarvestKnife",
                MapMarkerType.CoralReef,
                420f,
                Yield("CoralShard", 1f, 1, 3, false),
                Yield("PearlCompass", 0.03f, 1, 1, true));

            CreateNode(
                "SoftSeaweedPatch",
                "Soft Seaweed Patch",
                "Flexible seaweed useful for bandages and meals.",
                ResourceNodeType.Plant,
                "Neritha Reefwood",
                HarvestToolType.Sickle,
                "Sickle",
                MapMarkerType.ResourceNode,
                240f,
                Yield("SoftSeaweed", 1f, 2, 5, false),
                Yield("WaterMint", 0.15f, 1, 2, true));

            CreateNode(
                "EmberCrystalDeposit",
                "Ember Crystal Deposit",
                "A warm crystal deposit found near fire-aspected stone.",
                ResourceNodeType.Crystal,
                "Fire Region",
                HarvestToolType.Pickaxe,
                "Pickaxe",
                MapMarkerType.ResourceNode,
                600f,
                Yield("EmberCrystal", 1f, 1, 2, false),
                Yield("StormScale", 0.05f, 1, 1, true));

            CreateNode(
                "WindThreadReeds",
                "Wind-Thread Reeds",
                "Reeds that produce thread fluttering in still air.",
                ResourceNodeType.MagicalGrowth,
                "Air Region",
                HarvestToolType.Sickle,
                "Sickle",
                MapMarkerType.WindCurrent,
                480f,
                Yield("WindThread", 1f, 1, 3, false),
                Yield("StormLure", 0.08f, 1, 1, true));

            CreateNode(
                "StormScaleNest",
                "Storm Scale Nest",
                "A nest where storm-touched creatures sometimes shed scales.",
                ResourceNodeType.CreatureRemains,
                "Storm Cliffs",
                HarvestToolType.Hands,
                "",
                MapMarkerType.RareEnemySighting,
                900f,
                Yield("StormScale", 0.75f, 1, 2, false),
                Yield("CreatureTreat", 0.2f, 1, 1, true));

            CreateNode(
                "HealingFruitBush",
                "Healing Fruit Bush",
                "A bush bearing sweet fruit useful for healing recipes.",
                ResourceNodeType.Plant,
                "Green Region",
                HarvestToolType.Hands,
                "",
                MapMarkerType.ResourceNode,
                180f,
                Yield("HealingFruit", 1f, 2, 4, false));

            CreateNode(
                "WaterMintPatch",
                "Water Mint Patch",
                "A cooling herb patch near clean springs.",
                ResourceNodeType.Plant,
                "Water Region",
                HarvestToolType.Hands,
                "",
                MapMarkerType.ResourceNode,
                180f,
                Yield("WaterMint", 1f, 2, 5, false));

            CreateNode(
                "DriftwoodPile",
                "Driftwood Pile",
                "Sea-smoothed wood suitable for basic repairs.",
                ResourceNodeType.Driftwood,
                "Coastal Waters",
                HarvestToolType.Hands,
                "",
                MapMarkerType.ResourceNode,
                300f,
                Yield("BoatPlank", 1f, 1, 3, false));

            CreateNode(
                "SailclothWreckage",
                "Sailcloth Wreckage",
                "A torn wreckage pile with usable sailcloth.",
                ResourceNodeType.Wreckage,
                "Coastal Waters",
                HarvestToolType.Knife,
                "HarvestKnife",
                MapMarkerType.Dock,
                600f,
                Yield("Sailcloth", 1f, 1, 2, false),
                Yield("BoatRepairKit", 0.1f, 1, 1, true));

            CreateNode(
                "SeaSaltFlat",
                "Sea Salt Flat",
                "A coastal salt flat where sea salt can be gathered.",
                ResourceNodeType.SaltFlat,
                "Coastal Waters",
                HarvestToolType.Hands,
                "",
                MapMarkerType.ResourceNode,
                240f,
                Yield("SeaSalt", 1f, 2, 6, false));

            CreateNode(
                "CreatureTreatHerbs",
                "Creature Treat Herbs",
                "A mixed patch of herbs used for creature treats.",
                ResourceNodeType.Plant,
                "Neutral City Outskirts",
                HarvestToolType.Sickle,
                "Sickle",
                MapMarkerType.ResourceNode,
                240f,
                Yield("CreatureTreatBase", 1f, 1, 4, false),
                Yield("StableFeed", 0.1f, 1, 1, true));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn gathering resources.");
        }

        private static InventoryItemDefinition CreateToolItem(string id, string displayName, string description, int value)
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
            so.FindProperty("category").enumValueIndex = (int)InventoryItemCategory.Tool;
            so.FindProperty("rarity").enumValueIndex = (int)InventoryItemRarity.Common;
            so.FindProperty("questItem").boolValue = false;
            so.FindProperty("important").boolValue = false;
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

        private static void CreateNode(
            string id,
            string displayName,
            string description,
            ResourceNodeType nodeType,
            string region,
            HarvestToolType toolType,
            string requiredToolItemId,
            MapMarkerType markerType,
            float respawnSeconds,
            params YieldDef[] yields)
        {
            string path = $"{NodeDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ResourceNodeDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("nodeId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("nodeType").enumValueIndex = (int)nodeType;
            so.FindProperty("region").stringValue = region;
            so.FindProperty("important").boolValue = false;

            var req = so.FindProperty("requirement");
            req.FindPropertyRelative("ToolType").enumValueIndex = (int)toolType;
            req.FindPropertyRelative("RequiredToolItemId").stringValue = requiredToolItemId;
            req.FindPropertyRelative("RequiredElement").enumValueIndex = (int)AbilityElementType.Neutral;
            req.FindPropertyRelative("RequiredAbilityId").stringValue = "";

            so.FindProperty("maxHarvestsBeforeDepleted").intValue = 1;
            so.FindProperty("respawnSeconds").floatValue = respawnSeconds;
            so.FindProperty("rareYieldBonus").floatValue = 0f;
            so.FindProperty("addMapMarker").boolValue = true;
            so.FindProperty("markerType").enumValueIndex = (int)markerType;
            so.FindProperty("addJournalOnFirstHarvest").boolValue = true;

            var yieldsProp = so.FindProperty("yields");
            yieldsProp.arraySize = yields.Length;
            for (int i = 0; i < yields.Length; i++)
            {
                var item = AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>($"{ItemDir}/{yields[i].ItemId}.asset");
                var y = yieldsProp.GetArrayElementAtIndex(i);
                y.FindPropertyRelative("Item").objectReferenceValue = item;
                y.FindPropertyRelative("FallbackItemId").stringValue = yields[i].ItemId;
                y.FindPropertyRelative("Chance").floatValue = yields[i].Chance;
                y.FindPropertyRelative("MinQuantity").intValue = yields[i].Min;
                y.FindPropertyRelative("MaxQuantity").intValue = yields[i].Max;
                y.FindPropertyRelative("RareYield").boolValue = yields[i].Rare;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static YieldDef Yield(string itemId, float chance, int min, int max, bool rare)
        {
            return new YieldDef
            {
                ItemId = itemId,
                Chance = chance,
                Min = min,
                Max = max,
                Rare = rare
            };
        }

        private struct YieldDef
        {
            public string ItemId;
            public float Chance;
            public int Min;
            public int Max;
            public bool Rare;
        }
    }
}
#endif
