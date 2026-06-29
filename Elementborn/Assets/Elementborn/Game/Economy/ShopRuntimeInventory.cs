using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShopRuntimeInventory : MonoBehaviour
    {
        [SerializeField] private ShopDefinition shopDefinition;
        [SerializeField] private List<ShopItemListing> runtimeListings = new List<ShopItemListing>();
        [SerializeField] private float lastRestockedAtUnscaledTime = -1f;

        public ShopDefinition ShopDefinition => shopDefinition;
        public IReadOnlyList<ShopItemListing> Listings => runtimeListings;
        public float LastRestockedAtUnscaledTime => lastRestockedAtUnscaledTime;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (shopDefinition == null)
            {
                return;
            }

            if (shopDefinition.RestockMode == ShopRestockMode.Timed
                && lastRestockedAtUnscaledTime >= 0f
                && Time.unscaledTime >= lastRestockedAtUnscaledTime + shopDefinition.RestockSeconds)
            {
                Restock();
            }

            if (shopDefinition.RestockMode == ShopRestockMode.WorldEvent
                && !string.IsNullOrWhiteSpace(shopDefinition.RestockWorldEventId)
                && WorldEventTracker.IsActive(shopDefinition.RestockWorldEventId))
            {
                Restock();
            }
        }

        public void SetShopDefinition(ShopDefinition definition)
        {
            shopDefinition = definition;
            Initialize();
        }

        public void Initialize()
        {
            runtimeListings.Clear();

            if (shopDefinition == null)
            {
                return;
            }

            foreach (var listing in shopDefinition.Listings)
            {
                if (listing == null)
                {
                    continue;
                }

                runtimeListings.Add(CloneListing(listing));
            }

            if (lastRestockedAtUnscaledTime < 0f)
            {
                lastRestockedAtUnscaledTime = Time.unscaledTime;
            }
        }

        public ShopItemListing FindListing(string itemId)
        {
            return runtimeListings.Find(l => l != null && l.ItemId == itemId);
        }

        public void Restock()
        {
            foreach (var listing in runtimeListings)
            {
                if (listing != null)
                {
                    listing.RestockFull();
                }
            }

            lastRestockedAtUnscaledTime = Time.unscaledTime;
            NotificationFeed.Post($"{(shopDefinition != null ? shopDefinition.DisplayName : "Shop")} restocked.", NotificationType.Inventory);
        }

        public void ImportStock(string itemId, int stock)
        {
            var listing = FindListing(itemId);
            if (listing != null && !listing.InfiniteStock)
            {
                listing.Stock = Mathf.Max(0, stock);
            }
        }

        private static ShopItemListing CloneListing(ShopItemListing source)
        {
            return new ShopItemListing
            {
                Item = source.Item,
                FallbackItemId = source.FallbackItemId,
                PriceMode = source.PriceMode,
                FixedBuyPrice = source.FixedBuyPrice,
                FixedSellPrice = source.FixedSellPrice,
                BuyMultiplier = source.BuyMultiplier,
                SellMultiplier = source.SellMultiplier,
                Stock = source.Stock,
                MaxStock = source.MaxStock,
                InfiniteStock = source.InfiniteStock,
                CanBuy = source.CanBuy,
                CanSellToShop = source.CanSellToShop,
                RequiresWorldEvent = source.RequiresWorldEvent,
                RequiredWorldEventId = source.RequiredWorldEventId,
                RequiresFactionStanding = source.RequiresFactionStanding,
                RequiredFaction = source.RequiredFaction,
                RequiredReputation = source.RequiredReputation
            };
        }
    }
}
