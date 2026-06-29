using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class ShopItemListing
    {
        [Header("Item")]
        public InventoryItemDefinition Item;
        public string FallbackItemId = "";

        [Header("Price")]
        public ShopPriceMode PriceMode = ShopPriceMode.UseItemBaseValue;
        public int FixedBuyPrice = 1;
        public int FixedSellPrice = 1;
        [Range(0f, 5f)]
        public float BuyMultiplier = 1.25f;
        [Range(0f, 5f)]
        public float SellMultiplier = 0.5f;

        [Header("Stock")]
        public int Stock = 1;
        public int MaxStock = 10;
        public bool InfiniteStock = false;
        public bool CanBuy = true;
        public bool CanSellToShop = true;

        [Header("Restrictions")]
        public bool RequiresWorldEvent;
        public string RequiredWorldEventId = "";
        public bool RequiresFactionStanding;
        public ElementbornFactionId RequiredFaction = ElementbornFactionId.Unknown;
        public int RequiredReputation = 0;

        public string ItemId => Item != null ? Item.ItemId : FallbackItemId;
        public string DisplayName => Item != null ? Item.DisplayName : ItemId;
        public int BaseValue => Item != null ? Item.BaseValue : Mathf.Max(1, FixedBuyPrice);

        public bool HasStock(int quantity)
        {
            return InfiniteStock || Stock >= Mathf.Max(1, quantity);
        }

        public void RemoveStock(int quantity)
        {
            if (InfiniteStock)
            {
                return;
            }

            Stock = Mathf.Max(0, Stock - Mathf.Max(1, quantity));
        }

        public void AddStock(int quantity)
        {
            if (InfiniteStock)
            {
                return;
            }

            Stock = Mathf.Clamp(Stock + Mathf.Max(1, quantity), 0, Mathf.Max(0, MaxStock));
        }

        public void RestockFull()
        {
            if (!InfiniteStock)
            {
                Stock = Mathf.Max(0, MaxStock);
            }
        }

        public bool RequirementsMet(out string reason)
        {
            reason = "";

            if (string.IsNullOrWhiteSpace(ItemId))
            {
                reason = "Listing has no item id.";
                return false;
            }

            if (RequiresWorldEvent && !string.IsNullOrWhiteSpace(RequiredWorldEventId)
                && !WorldEventTracker.IsActive(RequiredWorldEventId))
            {
                reason = $"Requires active event: {RequiredWorldEventId}.";
                return false;
            }

            if (RequiresFactionStanding
                && RequiredFaction != ElementbornFactionId.Unknown
                && FactionReputationTracker.GetValue(RequiredFaction) < RequiredReputation)
            {
                reason = $"Requires {RequiredFaction} reputation {RequiredReputation}.";
                return false;
            }

            return true;
        }
    }
}
