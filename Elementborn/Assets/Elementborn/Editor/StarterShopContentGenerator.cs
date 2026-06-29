#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterShopContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string ShopDir = BaseDir + "/Shops";
        private const string ItemDir = BaseDir + "/Items";

        [MenuItem("Elementborn/Generate Starter Content/Shops")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(ShopDir);

            CreateShop(
                "KramSupplies",
                "Kram's Field Supplies",
                "A small practical kit of early exploration goods.",
                ElementbornFactionId.UnificationCircle,
                Listing("HealingFruitTea", 6, 12, 6, 4),
                Listing("CreatureTreat", 10, 8, 5, 3),
                Listing("ElementalArrowBundle", 5, 20, 12, 6));

            CreateShop(
                "NerithaReefMarket",
                "Neritha Reef Market",
                "Water-channeler market selling reef goods and supplies.",
                ElementbornFactionId.WaterChannelers,
                Listing("CoralBandage", 8, 18, 10, 5),
                Listing("SoftSeaweed", 20, 3, 1, 20),
                Listing("CoralShard", 20, 8, 4, 20),
                Listing("SimpleReefStew", 10, 10, 5, 10));

            CreateShop(
                "Dockwright",
                "Dockwright's Boatworks",
                "Boat repair supplies and sail patches.",
                ElementbornFactionId.ReefwoodVillagers,
                Listing("BoatRepairKit", 5, 25, 12, 5),
                Listing("WindSailPatch", 3, 40, 20, 3),
                Listing("BoatPlank", 20, 8, 3, 20),
                Listing("Sailcloth", 20, 8, 3, 20));

            CreateShop(
                "StableKeeper",
                "Stable Keeper",
                "Creature care items, feed, and lures.",
                ElementbornFactionId.ReefwoodVillagers,
                Listing("StableFeed", 15, 6, 3, 15),
                Listing("CreatureTreat", 15, 8, 4, 15),
                Listing("StormLure", 4, 35, 18, 4));

            CreateShop(
                "MerchantCaravan",
                "Traveling Merchant Caravan",
                "Temporary caravan inventory unlocked during merchant events.",
                ElementbornFactionId.Merchants,
                EventListing("ClarityTonic", 5, 18, 10, 5, "merchant_caravan_arrival"),
                EventListing("Emberblade", 1, 100, 50, 1, "merchant_caravan_arrival"),
                EventListing("PearlCompass", 1, 0, 0, 1, "merchant_caravan_arrival"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn shops.");
        }

        private static ShopItemListing Listing(string itemId, int stock, int buy, int sell, int maxStock)
        {
            var item = AssetDatabase.LoadAssetAtPath<InventoryItemDefinition>($"{ItemDir}/{itemId}.asset");
            return new ShopItemListing
            {
                Item = item,
                FallbackItemId = itemId,
                PriceMode = ShopPriceMode.FixedPrice,
                FixedBuyPrice = buy,
                FixedSellPrice = sell,
                Stock = stock,
                MaxStock = maxStock,
                InfiniteStock = false,
                CanBuy = true,
                CanSellToShop = true,
                BuyMultiplier = 1f,
                SellMultiplier = 0.5f
            };
        }

        private static ShopItemListing EventListing(string itemId, int stock, int buy, int sell, int maxStock, string eventId)
        {
            var listing = Listing(itemId, stock, buy, sell, maxStock);
            listing.RequiresWorldEvent = true;
            listing.RequiredWorldEventId = eventId;
            return listing;
        }

        private static void CreateShop(string id, string displayName, string description, ElementbornFactionId discountFaction, params ShopItemListing[] listings)
        {
            string path = $"{ShopDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ShopDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ShopDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("shopId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("restockMode").enumValueIndex = (int)ShopRestockMode.Timed;
            so.FindProperty("restockSeconds").floatValue = 600f;
            so.FindProperty("allowSelling").boolValue = true;
            so.FindProperty("allowBuying").boolValue = true;
            so.FindProperty("globalBuyMultiplier").floatValue = 1f;
            so.FindProperty("globalSellMultiplier").floatValue = 1f;

            var discounts = so.FindProperty("factionDiscounts");
            discounts.arraySize = discountFaction == ElementbornFactionId.Unknown ? 0 : 1;
            if (discounts.arraySize > 0)
            {
                var rule = discounts.GetArrayElementAtIndex(0);
                rule.FindPropertyRelative("Faction").enumValueIndex = (int)discountFaction;
                rule.FindPropertyRelative("RequiredReputation").intValue = 40;
                rule.FindPropertyRelative("DiscountPercent").floatValue = 10f;
                rule.FindPropertyRelative("SellBonusPercent").floatValue = 5f;
            }

            var listProp = so.FindProperty("listings");
            listProp.arraySize = listings.Length;
            for (int i = 0; i < listings.Length; i++)
            {
                var src = listings[i];
                var dst = listProp.GetArrayElementAtIndex(i);
                dst.FindPropertyRelative("Item").objectReferenceValue = src.Item;
                dst.FindPropertyRelative("FallbackItemId").stringValue = src.FallbackItemId;
                dst.FindPropertyRelative("PriceMode").enumValueIndex = (int)src.PriceMode;
                dst.FindPropertyRelative("FixedBuyPrice").intValue = src.FixedBuyPrice;
                dst.FindPropertyRelative("FixedSellPrice").intValue = src.FixedSellPrice;
                dst.FindPropertyRelative("BuyMultiplier").floatValue = src.BuyMultiplier;
                dst.FindPropertyRelative("SellMultiplier").floatValue = src.SellMultiplier;
                dst.FindPropertyRelative("Stock").intValue = src.Stock;
                dst.FindPropertyRelative("MaxStock").intValue = src.MaxStock;
                dst.FindPropertyRelative("InfiniteStock").boolValue = src.InfiniteStock;
                dst.FindPropertyRelative("CanBuy").boolValue = src.CanBuy;
                dst.FindPropertyRelative("CanSellToShop").boolValue = src.CanSellToShop;
                dst.FindPropertyRelative("RequiresWorldEvent").boolValue = src.RequiresWorldEvent;
                dst.FindPropertyRelative("RequiredWorldEventId").stringValue = src.RequiredWorldEventId;
                dst.FindPropertyRelative("RequiresFactionStanding").boolValue = src.RequiresFactionStanding;
                dst.FindPropertyRelative("RequiredFaction").enumValueIndex = (int)src.RequiredFaction;
                dst.FindPropertyRelative("RequiredReputation").intValue = src.RequiredReputation;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
