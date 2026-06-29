using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Generic storage container. Chests/camps/homes can use this.
    /// </summary>
    public sealed class StorageContainerInventory : MonoBehaviour
    {
        [SerializeField] private string storageId = "";
        [SerializeField] private string displayName = "Storage";
        [SerializeField] private int maxSlots = 40;
        [SerializeField] private List<InventoryItemStack> stacks = new List<InventoryItemStack>();

        public string StorageId => string.IsNullOrWhiteSpace(storageId) ? name : storageId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? StorageId : displayName;
        public IReadOnlyList<InventoryItemStack> Stacks => stacks;

        public InventoryTransactionResult Add(InventoryItemDefinition definition, int quantity)
        {
            if (definition == null)
            {
                return InventoryTransactionResult.Fail(quantity, 0, "No item definition.");
            }

            int requested = quantity;
            int moved = 0;

            if (definition.Stackable)
            {
                foreach (var stack in stacks)
                {
                    if (stack != null && stack.CanMerge(definition))
                    {
                        int added = stack.AddQuantity(quantity);
                        moved += added;
                        quantity -= added;
                        if (quantity <= 0)
                        {
                            break;
                        }
                    }
                }
            }

            while (quantity > 0 && stacks.Count < Mathf.Max(1, maxSlots))
            {
                int stackQuantity = Mathf.Min(quantity, definition.MaxStack);
                stacks.Add(new InventoryItemStack(definition, stackQuantity));
                moved += stackQuantity;
                quantity -= stackQuantity;
            }

            Cleanup();
            return moved > 0
                ? InventoryTransactionResult.Ok(requested, moved)
                : InventoryTransactionResult.Fail(requested, 0, "Storage is full.");
        }

        public InventoryTransactionResult AddId(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return InventoryTransactionResult.Fail(quantity, 0, "Item id is empty.");
            }

            int requested = quantity;
            int moved = 0;

            foreach (var stack in stacks)
            {
                if (stack != null && stack.ResolvedItemId == itemId && stack.Quantity < stack.MaxStack)
                {
                    int added = stack.AddQuantity(quantity);
                    moved += added;
                    quantity -= added;
                    if (quantity <= 0)
                    {
                        break;
                    }
                }
            }

            while (quantity > 0 && stacks.Count < Mathf.Max(1, maxSlots))
            {
                int stackQuantity = Mathf.Min(quantity, 99);
                stacks.Add(new InventoryItemStack(itemId, stackQuantity));
                moved += stackQuantity;
                quantity -= stackQuantity;
            }

            Cleanup();
            return moved > 0
                ? InventoryTransactionResult.Ok(requested, moved)
                : InventoryTransactionResult.Fail(requested, 0, "Storage is full.");
        }

        public InventoryTransactionResult RemoveById(string itemId, int quantity)
        {
            int available = Count(itemId);
            if (available < quantity)
            {
                return InventoryTransactionResult.Fail(quantity, 0, $"Not enough {itemId}.");
            }

            int requested = quantity;
            int moved = 0;

            for (int i = stacks.Count - 1; i >= 0 && quantity > 0; i--)
            {
                var stack = stacks[i];
                if (stack == null || stack.ResolvedItemId != itemId)
                {
                    continue;
                }

                int removed = stack.RemoveQuantity(quantity);
                moved += removed;
                quantity -= removed;

                if (stack.Quantity <= 0)
                {
                    stacks.RemoveAt(i);
                }
            }

            Cleanup();
            return InventoryTransactionResult.Ok(requested, moved);
        }

        public bool TransferToPlayer(string itemId, int quantity)
        {
            if (Count(itemId) < quantity)
            {
                return false;
            }

            var remove = RemoveById(itemId, quantity);
            if (!remove.Success)
            {
                return false;
            }

            var add = PlayerInventoryTracker.AddItemId(itemId, remove.Moved);
            if (add.Moved < remove.Moved)
            {
                AddId(itemId, remove.Moved - add.Moved);
            }

            return add.Moved > 0;
        }

        public bool TransferFromPlayer(InventoryItemDefinition definition, int quantity)
        {
            if (definition == null || !PlayerInventoryTracker.HasItem(definition, quantity))
            {
                return false;
            }

            var add = Add(definition, quantity);
            if (add.Moved <= 0)
            {
                return false;
            }

            PlayerInventoryTracker.RemoveItem(definition, add.Moved);
            return true;
        }

        public int Count(string itemId)
        {
            int total = 0;
            foreach (var stack in stacks)
            {
                if (stack != null && stack.ResolvedItemId == itemId)
                {
                    total += stack.Quantity;
                }
            }

            return total;
        }

        public void Clear()
        {
            stacks.Clear();
        }

        public void ImportSavedStack(string itemId, int quantity)
        {
            AddId(itemId, quantity);
        }

        private void Cleanup()
        {
            stacks.RemoveAll(s => s == null || s.IsEmpty);
        }
    }
}
