using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShopService : MonoBehaviour
    {
        public static ShopService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static ShopService Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(ShopService));
            return go.AddComponent<ShopService>();
        }

        public static ShopTransactionResult Buy(ShopRuntimeInventory shopInventory, string itemId, int quantity = 1)
        {
            if (shopInventory == null || shopInventory.ShopDefinition == null)
            {
                return Fail(ShopTransactionType.Buy, itemId, quantity, "No shop selected.");
            }

            var shop = shopInventory.ShopDefinition;
            if (!shop.AllowBuying)
            {
                return Fail(ShopTransactionType.Buy, itemId, quantity, "This shop is not selling.");
            }

            var listing = shopInventory.FindListing(itemId);
            if (listing == null || !listing.CanBuy)
            {
                return Fail(ShopTransactionType.Buy, itemId, quantity, "Item is not for sale.");
            }

            if (!listing.RequirementsMet(out string reason))
            {
                return Fail(ShopTransactionType.Buy, itemId, quantity, reason);
            }

            quantity = Mathf.Max(1, quantity);
            if (!listing.HasStock(quantity))
            {
                return Fail(ShopTransactionType.Buy, itemId, quantity, "Not enough stock.");
            }

            int price = ShopPriceCalculator.GetBuyPrice(shop, listing, quantity);
            if (!PlayerInventoryTracker.SpendCurrency(price))
            {
                return Fail(ShopTransactionType.Buy, itemId, quantity, "Not enough coins.");
            }

            InventoryTransactionResult add = listing.Item != null
                ? PlayerInventoryTracker.AddItem(listing.Item, quantity)
                : PlayerInventoryTracker.AddItemId(listing.ItemId, quantity);

            if (add.Moved <= 0)
            {
                PlayerInventoryTracker.AddCurrency(price);
                return Fail(ShopTransactionType.Buy, itemId, quantity, "Inventory is full.");
            }

            listing.RemoveStock(add.Moved);

            string message = $"Bought {listing.DisplayName} x{add.Moved} for {price}.";
            NotificationFeed.Post(message, NotificationType.Inventory);
            return ShopTransactionResult.Ok(ShopTransactionType.Buy, listing.ItemId, add.Moved, price, message);
        }

        public static ShopTransactionResult Sell(ShopRuntimeInventory shopInventory, string itemId, int quantity = 1)
        {
            if (shopInventory == null || shopInventory.ShopDefinition == null)
            {
                return Fail(ShopTransactionType.Sell, itemId, quantity, "No shop selected.");
            }

            var shop = shopInventory.ShopDefinition;
            if (!shop.AllowSelling)
            {
                return Fail(ShopTransactionType.Sell, itemId, quantity, "This shop is not buying.");
            }

            quantity = Mathf.Max(1, quantity);
            if (!PlayerInventoryTracker.HasItemId(itemId, quantity))
            {
                return Fail(ShopTransactionType.Sell, itemId, quantity, "You do not have enough of that item.");
            }

            var listing = shopInventory.FindListing(itemId);
            int price;
            if (listing != null)
            {
                if (!listing.CanSellToShop)
                {
                    return Fail(ShopTransactionType.Sell, itemId, quantity, "This shop will not buy that.");
                }

                price = ShopPriceCalculator.GetSellPrice(shop, listing, quantity);
            }
            else
            {
                var stack = PlayerInventoryTracker.Ensure().FindStack(itemId);
                price = ShopPriceCalculator.GetFallbackSellPrice(stack, quantity);
            }

            InventoryTransactionResult remove = PlayerInventoryTracker.RemoveItemId(itemId, quantity);
            if (!remove.Success)
            {
                return Fail(ShopTransactionType.Sell, itemId, quantity, remove.Message);
            }

            PlayerInventoryTracker.AddCurrency(price);

            if (listing != null)
            {
                listing.AddStock(remove.Moved);
            }

            string message = $"Sold {itemId} x{remove.Moved} for {price}.";
            NotificationFeed.Post(message, NotificationType.Inventory);
            return ShopTransactionResult.Ok(ShopTransactionType.Sell, itemId, remove.Moved, price, message);
        }

        public static void Restock(ShopRuntimeInventory shopInventory)
        {
            if (shopInventory == null)
            {
                return;
            }

            shopInventory.Restock();
        }

        private static ShopTransactionResult Fail(ShopTransactionType type, string itemId, int quantity, string message)
        {
            NotificationFeed.Post(message, NotificationType.Warning);
            return ShopTransactionResult.Fail(type, itemId, quantity, message);
        }
    }
}
