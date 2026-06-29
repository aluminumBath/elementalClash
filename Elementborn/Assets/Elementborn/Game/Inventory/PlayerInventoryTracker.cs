using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Persistent runtime player inventory foundation.
    /// This stores item definitions while running; save bridges persist item IDs and quantities.
    /// </summary>
    public sealed class PlayerInventoryTracker : MonoBehaviour
    {
        public static PlayerInventoryTracker Instance { get; private set; }

        [Header("Inventory")]
        [SerializeField] private int maxSlots = 40;
        [SerializeField] private List<InventoryItemStack> stacks = new List<InventoryItemStack>();

        [Header("Wallet")]
        [SerializeField] private int currency;

        [Header("Events")]
        [SerializeField] private InventoryChangedEvent onInventoryChanged;
        [SerializeField] private InventoryItemStackEvent onItemAdded;
        [SerializeField] private InventoryItemStackEvent onItemRemoved;

        public IReadOnlyList<InventoryItemStack> Stacks => stacks;
        public int MaxSlots => Mathf.Max(1, maxSlots);
        public int Currency => currency;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Cleanup();
        }

        public static PlayerInventoryTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(PlayerInventoryTracker));
            return go.AddComponent<PlayerInventoryTracker>();
        }

        public static InventoryTransactionResult AddItem(InventoryItemDefinition definition, int quantity = 1)
        {
            return Ensure().Add(definition, quantity);
        }

        public static InventoryTransactionResult AddItemId(string itemId, int quantity = 1)
        {
            return Ensure().AddId(itemId, quantity);
        }

        public static InventoryTransactionResult RemoveItem(InventoryItemDefinition definition, int quantity = 1)
        {
            return definition == null
                ? InventoryTransactionResult.Fail(quantity, 0, "No item definition.")
                : Ensure().RemoveById(definition.ItemId, quantity);
        }

        public static InventoryTransactionResult RemoveItemId(string itemId, int quantity = 1)
        {
            return Ensure().RemoveById(itemId, quantity);
        }

        public static bool HasItem(InventoryItemDefinition definition, int quantity = 1)
        {
            return definition != null && Ensure().Count(definition.ItemId) >= quantity;
        }

        public static bool HasItemId(string itemId, int quantity = 1)
        {
            return Ensure().Count(itemId) >= quantity;
        }

        public static int CountItemId(string itemId)
        {
            return Ensure().Count(itemId);
        }

        public static void AddCurrency(int amount)
        {
            Ensure().ModifyCurrency(amount);
        }

        public static bool SpendCurrency(int amount)
        {
            return Ensure().Spend(amount);
        }

        public InventoryTransactionResult Add(InventoryItemDefinition definition, int quantity = 1)
        {
            if (definition == null)
            {
                return InventoryTransactionResult.Fail(quantity, 0, "No item definition.");
            }

            if (quantity <= 0)
            {
                return InventoryTransactionResult.Fail(quantity, 0, "Quantity must be positive.");
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

            while (quantity > 0 && stacks.Count < MaxSlots)
            {
                int stackQuantity = Mathf.Min(quantity, definition.MaxStack);
                var stack = new InventoryItemStack(definition, stackQuantity);
                stacks.Add(stack);
                moved += stackQuantity;
                quantity -= stackQuantity;
                onItemAdded?.Invoke(stack);
            }

            Cleanup();
            onInventoryChanged?.Invoke();

            string message = moved >= requested
                ? $"Picked up {definition.DisplayName} x{moved}."
                : $"Inventory full. Picked up {definition.DisplayName} x{moved}/{requested}.";

            if (moved > 0)
            {
                NotificationFeed.Post(message, NotificationType.Inventory);
                if (definition.Important)
                {
                    PlayerJournalTracker.AddOrUpdateEntry(
                        "item_" + PlayerJournalTracker.Safe(definition.ItemId),
                        JournalEntryType.Item,
                        definition.DisplayName,
                        definition.Description,
                        relatedId: definition.ItemId);
                }
            }

            return moved > 0
                ? InventoryTransactionResult.Ok(requested, moved, message)
                : InventoryTransactionResult.Fail(requested, 0, "Inventory is full.");
        }

        public InventoryTransactionResult AddId(string itemId, int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return InventoryTransactionResult.Fail(quantity, 0, "Item id is empty.");
            }

            if (quantity <= 0)
            {
                return InventoryTransactionResult.Fail(quantity, 0, "Quantity must be positive.");
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

            while (quantity > 0 && stacks.Count < MaxSlots)
            {
                int stackQuantity = Mathf.Min(quantity, 99);
                var stack = new InventoryItemStack(itemId, stackQuantity);
                stacks.Add(stack);
                moved += stackQuantity;
                quantity -= stackQuantity;
                onItemAdded?.Invoke(stack);
            }

            Cleanup();
            onInventoryChanged?.Invoke();

            return moved > 0
                ? InventoryTransactionResult.Ok(requested, moved, $"Picked up {itemId} x{moved}.")
                : InventoryTransactionResult.Fail(requested, 0, "Inventory is full.");
        }

        public InventoryTransactionResult RemoveById(string itemId, int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return InventoryTransactionResult.Fail(quantity, 0, "Item id is empty.");
            }

            if (quantity <= 0)
            {
                return InventoryTransactionResult.Fail(quantity, 0, "Quantity must be positive.");
            }

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
                    onItemRemoved?.Invoke(stack);
                    stacks.RemoveAt(i);
                }
            }

            Cleanup();
            onInventoryChanged?.Invoke();
            return InventoryTransactionResult.Ok(requested, moved, $"Removed {itemId} x{moved}.");
        }

        public bool UseItemById(string itemId)
        {
            var stack = FindStack(itemId);
            if (stack == null)
            {
                NotificationFeed.Post($"You do not have {itemId}.", NotificationType.Warning);
                return false;
            }

            var definition = stack.Definition;
            if (definition != null && !definition.Usable)
            {
                NotificationFeed.Post($"{definition.DisplayName} cannot be used right now.", NotificationType.Warning);
                return false;
            }

            string name = definition != null ? definition.DisplayName : itemId;
            NotificationFeed.Post($"Used {name}.", NotificationType.Inventory);

            if (definition == null || definition.ConsumedOnUse)
            {
                RemoveById(itemId, 1);
            }

            return true;
        }

        public int Count(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

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

        public InventoryItemStack FindStack(string itemId)
        {
            return stacks.Find(s => s != null && s.ResolvedItemId == itemId);
        }

        public void ModifyCurrency(int amount)
        {
            currency = Mathf.Max(0, currency + amount);
            NotificationFeed.Post(amount >= 0 ? $"Gained {amount} coins." : $"Spent {-amount} coins.", NotificationType.Inventory);
            onInventoryChanged?.Invoke();
        }

        public bool Spend(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (currency < amount)
            {
                NotificationFeed.Post("Not enough coins.", NotificationType.Warning);
                return false;
            }

            ModifyCurrency(-amount);
            return true;
        }

        public void Clear()
        {
            stacks.Clear();
            currency = 0;
            onInventoryChanged?.Invoke();
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
