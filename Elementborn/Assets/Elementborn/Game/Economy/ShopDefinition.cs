using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Economy/Shop Definition", fileName = "ShopDefinition")]
    public sealed class ShopDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string shopId = "";
        [SerializeField] private string displayName = "Shop";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Behavior")]
        [SerializeField] private ShopRestockMode restockMode = ShopRestockMode.Never;
        [SerializeField] private float restockSeconds = 600f;
        [SerializeField] private string restockWorldEventId = "";
        [SerializeField] private bool allowSelling = true;
        [SerializeField] private bool allowBuying = true;

        [Header("Pricing")]
        [SerializeField] private float globalBuyMultiplier = 1f;
        [SerializeField] private float globalSellMultiplier = 1f;
        [SerializeField] private List<ShopFactionDiscountRule> factionDiscounts = new List<ShopFactionDiscountRule>();

        [Header("Listings")]
        [SerializeField] private List<ShopItemListing> listings = new List<ShopItemListing>();

        public string ShopId => string.IsNullOrWhiteSpace(shopId) ? name : shopId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ShopId : displayName;
        public string Description => description;
        public ShopRestockMode RestockMode => restockMode;
        public float RestockSeconds => Mathf.Max(1f, restockSeconds);
        public string RestockWorldEventId => restockWorldEventId;
        public bool AllowSelling => allowSelling;
        public bool AllowBuying => allowBuying;
        public float GlobalBuyMultiplier => Mathf.Max(0f, globalBuyMultiplier);
        public float GlobalSellMultiplier => Mathf.Max(0f, globalSellMultiplier);
        public IReadOnlyList<ShopFactionDiscountRule> FactionDiscounts => factionDiscounts;
        public IReadOnlyList<ShopItemListing> Listings => listings;

        public ShopItemListing FindListing(string itemId)
        {
            return listings.Find(l => l != null && l.ItemId == itemId);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(shopId))
            {
                shopId = name;
            }

            restockSeconds = Mathf.Max(1f, restockSeconds);
            globalBuyMultiplier = Mathf.Max(0f, globalBuyMultiplier);
            globalSellMultiplier = Mathf.Max(0f, globalSellMultiplier);
        }
    }
}
