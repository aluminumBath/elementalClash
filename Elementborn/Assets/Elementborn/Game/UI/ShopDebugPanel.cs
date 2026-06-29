using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class ShopDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private VendorShopInteractable vendor;
        [SerializeField] private bool refreshEveryFrame = true;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            if (refreshEveryFrame)
            {
                Refresh();
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            if (text == null || vendor == null)
            {
                return;
            }

            var inventory = vendor.GetShopInventory();
            if (inventory == null || inventory.ShopDefinition == null)
            {
                text.text = "No shop selected.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(inventory.ShopDefinition.DisplayName);
            sb.AppendLine($"Coins: {PlayerInventoryTracker.Ensure().Currency}");
            sb.AppendLine($"Buy discount: {ShopPriceCalculator.GetBuyDiscountPercent(inventory.ShopDefinition):0}%");
            sb.AppendLine("Listings:");

            foreach (var listing in inventory.Listings)
            {
                if (listing == null)
                {
                    continue;
                }

                int buy = ShopPriceCalculator.GetBuyPrice(inventory.ShopDefinition, listing, 1);
                int sell = ShopPriceCalculator.GetSellPrice(inventory.ShopDefinition, listing, 1);
                string stock = listing.InfiniteStock ? "∞" : listing.Stock.ToString();
                sb.AppendLine($"- {listing.DisplayName} | stock {stock} | buy {buy} | sell {sell}");
            }

            text.text = sb.ToString();
        }
    }
}
