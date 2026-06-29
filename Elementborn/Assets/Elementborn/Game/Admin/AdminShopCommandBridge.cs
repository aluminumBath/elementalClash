using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for shops/economy.
    ///
    /// Commands:
    /// shop.buy itemId|quantity
    /// shop.sell itemId|quantity
    /// shop.restock
    /// money.add amount
    /// money.spend amount
    /// </summary>
    public sealed class AdminShopCommandBridge : MonoBehaviour
    {
        [SerializeField] private VendorShopInteractable currentVendor;

        public void SetCurrentVendor(VendorShopInteractable vendor)
        {
            currentVendor = vendor;
        }

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed.StartsWith("shop.buy "))
            {
                if (currentVendor == null)
                {
                    NotificationFeed.Post("No current vendor assigned.", NotificationType.Warning);
                    return true;
                }

                ParseItemQuantity(trimmed.Substring("shop.buy ".Length), out string itemId, out int quantity);
                ShopService.Buy(currentVendor.GetShopInventory(), itemId, quantity);
                return true;
            }

            if (trimmed.StartsWith("shop.sell "))
            {
                if (currentVendor == null)
                {
                    NotificationFeed.Post("No current vendor assigned.", NotificationType.Warning);
                    return true;
                }

                ParseItemQuantity(trimmed.Substring("shop.sell ".Length), out string itemId, out int quantity);
                ShopService.Sell(currentVendor.GetShopInventory(), itemId, quantity);
                return true;
            }

            if (trimmed == "shop.restock")
            {
                if (currentVendor != null)
                {
                    ShopService.Restock(currentVendor.GetShopInventory());
                }

                return true;
            }

            if (trimmed.StartsWith("money.add "))
            {
                string value = trimmed.Substring("money.add ".Length).Trim();
                if (int.TryParse(value, out int amount))
                {
                    PlayerInventoryTracker.AddCurrency(amount);
                }

                return true;
            }

            if (trimmed.StartsWith("money.spend "))
            {
                string value = trimmed.Substring("money.spend ".Length).Trim();
                if (int.TryParse(value, out int amount))
                {
                    PlayerInventoryTracker.SpendCurrency(amount);
                }

                return true;
            }

            return false;
        }

        private static void ParseItemQuantity(string payload, out string itemId, out int quantity)
        {
            string[] parts = payload.Split('|');
            itemId = parts.Length > 0 ? parts[0].Trim() : "";
            quantity = 1;
            if (parts.Length > 1)
            {
                int.TryParse(parts[1], out quantity);
                quantity = Mathf.Max(1, quantity);
            }
        }
    }
}
