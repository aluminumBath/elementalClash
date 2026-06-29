using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class CraftingIngredientRequirement
    {
        public InventoryItemDefinition Item;
        public string FallbackItemId = "";
        public int Quantity = 1;

        public string ItemId => Item != null ? Item.ItemId : FallbackItemId;
        public string DisplayName => Item != null ? Item.DisplayName : ItemId;
        public int RequiredQuantity => Mathf.Max(1, Quantity);
    }
}
