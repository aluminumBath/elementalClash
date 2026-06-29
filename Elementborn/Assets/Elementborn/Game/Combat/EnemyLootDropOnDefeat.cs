using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyLootDropOnDefeat : MonoBehaviour
    {
        [SerializeField] private LootDropTableDefinition dropTable;
        [SerializeField] private bool deliverToPlayerInventory = true;
        [SerializeField] private bool autoHookSimpleCombatHealth = true;

        private SimpleCombatHealth simpleHealth;
        private bool dropped;

        private void Awake()
        {
            simpleHealth = GetComponent<SimpleCombatHealth>();
        }

        private void OnEnable()
        {
            if (autoHookSimpleCombatHealth && simpleHealth != null) simpleHealth.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (autoHookSimpleCombatHealth && simpleHealth != null) simpleHealth.Died -= HandleDeath;
        }

        public void NotifyDefeated()
        {
            HandleDeath();
        }

        private void HandleDeath()
        {
            if (dropped || dropTable == null) return;
            dropped = true;

            foreach (var entry in dropTable.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || !entry.RollDrop()) continue;
                int qty = entry.RollQuantity();
                if (deliverToPlayerInventory)
                {
                    if (entry.Item != null) PlayerInventoryTracker.AddItem(entry.Item, qty);
                    else PlayerInventoryTracker.AddItemId(entry.ItemId, qty);
                }
                NotificationFeed.Post($"Loot: {entry.ItemId} x{qty}", NotificationType.Inventory);
            }
        }
    }
}
