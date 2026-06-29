using UnityEngine;

namespace Elementborn.Game
{
    public sealed class VendorShopInteractable : BaseInteractable
    {
        [SerializeField] private string vendorName = "Vendor";
        [SerializeField] private ShopRuntimeInventory shopInventory;
        [SerializeField] private bool markOnMap = true;
        [SerializeField] private bool buyFirstAvailableOnInteract = false;

        private void Awake()
        {
            if (shopInventory == null)
            {
                shopInventory = GetComponent<ShopRuntimeInventory>();
            }

            if (shopInventory == null)
            {
                shopInventory = gameObject.AddComponent<ShopRuntimeInventory>();
            }
        }

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportVendorNpc(transform.position, vendorName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(vendorName, "Shop");
        }

        public override void Interact(GameObject interactor)
        {
            if (buyFirstAvailableOnInteract)
            {
                BuyFirstAvailable();
            }
            else
            {
                Debug.Log($"Open shop UI for {vendorName}. Listings: {shopInventory.Listings.Count}");
            }

            base.Interact(interactor);
        }

        public bool BuyFirstAvailable()
        {
            foreach (var listing in shopInventory.Listings)
            {
                if (listing == null || !listing.CanBuy || !listing.HasStock(1))
                {
                    continue;
                }

                var result = ShopService.Buy(shopInventory, listing.ItemId, 1);
                return result.Success;
            }

            NotificationFeed.Post("Nothing available to buy.", NotificationType.Warning);
            return false;
        }

        public ShopRuntimeInventory GetShopInventory()
        {
            return shopInventory;
        }
    }
}
