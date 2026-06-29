using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class InventoryItemStack
    {
        public InventoryItemDefinition Definition;
        public string ItemId = "";
        public int Quantity = 1;

        public string ResolvedItemId => Definition != null ? Definition.ItemId : ItemId;
        public string DisplayName => Definition != null ? Definition.DisplayName : ResolvedItemId;
        public int MaxStack => Definition != null ? Definition.MaxStack : 99;
        public bool IsEmpty => Quantity <= 0 || string.IsNullOrWhiteSpace(ResolvedItemId);

        public InventoryItemStack()
        {
        }

        public InventoryItemStack(InventoryItemDefinition definition, int quantity)
        {
            Definition = definition;
            ItemId = definition != null ? definition.ItemId : "";
            Quantity = Mathf.Max(0, quantity);
        }

        public InventoryItemStack(string itemId, int quantity)
        {
            ItemId = itemId ?? "";
            Quantity = Mathf.Max(0, quantity);
        }

        public bool CanMerge(InventoryItemDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            return ResolvedItemId == definition.ItemId && Quantity < definition.MaxStack;
        }

        public int AddQuantity(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int room = Mathf.Max(0, MaxStack - Quantity);
            int added = Mathf.Min(room, amount);
            Quantity += added;
            return added;
        }

        public int RemoveQuantity(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int removed = Mathf.Min(Quantity, amount);
            Quantity -= removed;
            return removed;
        }
    }
}
