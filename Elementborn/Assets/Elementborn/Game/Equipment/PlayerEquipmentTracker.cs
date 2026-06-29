using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class PlayerEquipmentTracker : MonoBehaviour
    {
        public static PlayerEquipmentTracker Instance { get; private set; }

        [SerializeField] private List<EquippedItemRecord> equipped = new List<EquippedItemRecord>();
        [SerializeField] private List<EquipmentItemDefinition> knownDefinitions = new List<EquipmentItemDefinition>();
        [SerializeField] private List<EquipmentSetBonusDefinition> setBonuses = new List<EquipmentSetBonusDefinition>();

        public IReadOnlyList<EquippedItemRecord> Equipped => equipped;
        public IReadOnlyList<EquipmentItemDefinition> KnownDefinitions => knownDefinitions;
        public IReadOnlyList<EquipmentSetBonusDefinition> SetBonuses => setBonuses;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSlots();
        }

        public static PlayerEquipmentTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(PlayerEquipmentTracker));
            return go.AddComponent<PlayerEquipmentTracker>();
        }

        public static bool Equip(EquipmentItemDefinition definition)
        {
            return Ensure().EquipInternal(definition);
        }

        public static bool EquipByItemId(string itemId, EquipmentSlotType slot, string displayName = "")
        {
            return Ensure().EquipRaw(itemId, slot, displayName);
        }

        public static bool Unequip(EquipmentSlotType slot)
        {
            return Ensure().UnequipInternal(slot);
        }

        public static bool HasEquippedEquipment(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
            {
                return false;
            }

            foreach (var item in Ensure().equipped)
            {
                if (item != null && item.EquipmentId == equipmentId)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasEquippedItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            foreach (var item in Ensure().equipped)
            {
                if (item != null && item.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        public static float GetFlatBonus(GearStatType stat)
        {
            return Ensure().BuildSnapshot().GetFlat(stat);
        }

        public static float GetPercentBonus(GearStatType stat)
        {
            return Ensure().BuildSnapshot().GetPercent(stat);
        }

        public static float ApplyBonuses(GearStatType stat, float baseValue)
        {
            return Ensure().BuildSnapshot().Apply(stat, baseValue);
        }

        public bool EquipInternal(EquipmentItemDefinition definition)
        {
            if (definition == null)
            {
                NotificationFeed.Post("No equipment selected.", NotificationType.Warning);
                return false;
            }

            if (!definition.RequirementsMet(out string reason))
            {
                NotificationFeed.Post(reason, NotificationType.Warning);
                return false;
            }

            if (!knownDefinitions.Contains(definition))
            {
                knownDefinitions.Add(definition);
            }

            if (definition.RemovesFromInventoryWhenEquipped)
            {
                PlayerInventoryTracker.RemoveItemId(definition.ItemId, 1);
            }

            var record = GetSlotRecord(definition.Slot);
            record.Slot = definition.Slot;
            record.EquipmentId = definition.EquipmentId;
            record.ItemId = definition.ItemId;
            record.DisplayName = definition.DisplayName;
            record.Category = definition.Category;
            record.Rarity = definition.Rarity;

            NotificationFeed.Post($"Equipped {definition.DisplayName}.", NotificationType.Inventory);

            PlayerJournalTracker.AddOrUpdateEntry(
                "equipment_" + PlayerJournalTracker.Safe(definition.EquipmentId),
                JournalEntryType.Item,
                definition.DisplayName,
                definition.Description,
                relatedId: definition.EquipmentId);

            return true;
        }

        public bool EquipRaw(string itemId, EquipmentSlotType slot, string displayName)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                NotificationFeed.Post("No item id supplied.", NotificationType.Warning);
                return false;
            }

            if (!PlayerInventoryTracker.HasItemId(itemId, 1))
            {
                NotificationFeed.Post($"Requires item: {itemId}.", NotificationType.Warning);
                return false;
            }

            var record = GetSlotRecord(slot);
            record.Slot = slot;
            record.EquipmentId = itemId;
            record.ItemId = itemId;
            record.DisplayName = string.IsNullOrWhiteSpace(displayName) ? itemId : displayName;
            record.Category = EquipmentCategory.Utility;
            record.Rarity = InventoryItemRarity.Common;

            NotificationFeed.Post($"Equipped {record.DisplayName}.", NotificationType.Inventory);
            return true;
        }

        public bool UnequipInternal(EquipmentSlotType slot)
        {
            var record = GetSlotRecord(slot);
            if (string.IsNullOrWhiteSpace(record.ItemId))
            {
                return false;
            }

            string name = record.DisplayName;
            var definition = FindDefinition(record.EquipmentId);
            if (definition != null && definition.ReturnsToInventoryWhenUnequipped)
            {
                PlayerInventoryTracker.AddItemId(definition.ItemId, 1);
            }

            record.EquipmentId = "";
            record.ItemId = "";
            record.DisplayName = "";
            record.Category = EquipmentCategory.Unknown;
            record.Rarity = InventoryItemRarity.Common;

            NotificationFeed.Post($"Unequipped {name}.", NotificationType.Inventory);
            return true;
        }

        public EquipmentItemDefinition FindDefinition(string equipmentId)
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
            {
                return null;
            }

            return knownDefinitions.Find(d => d != null && d.EquipmentId == equipmentId);
        }

        public EquippedItemRecord GetSlotRecord(EquipmentSlotType slot)
        {
            EnsureSlots();
            var record = equipped.Find(e => e.Slot == slot);
            if (record == null)
            {
                record = new EquippedItemRecord { Slot = slot };
                equipped.Add(record);
            }

            return record;
        }

        public EquipmentStatSnapshot BuildSnapshot()
        {
            var snapshot = new EquipmentStatSnapshot();

            foreach (var record in equipped)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.EquipmentId))
                {
                    continue;
                }

                var definition = FindDefinition(record.EquipmentId);
                if (definition == null)
                {
                    continue;
                }

                foreach (var modifier in definition.StatModifiers)
                {
                    snapshot.Add(modifier);
                }
            }

            foreach (var set in setBonuses)
            {
                if (set == null || !set.IsActive())
                {
                    continue;
                }

                foreach (var modifier in set.BonusModifiers)
                {
                    snapshot.Add(modifier);
                }
            }

            return snapshot;
        }

        public void ImportRecord(EquippedItemRecord record)
        {
            if (record == null)
            {
                return;
            }

            var existing = GetSlotRecord(record.Slot);
            existing.EquipmentId = record.EquipmentId;
            existing.ItemId = record.ItemId;
            existing.DisplayName = record.DisplayName;
            existing.Category = record.Category;
            existing.Rarity = record.Rarity;
        }

        public void RegisterDefinition(EquipmentItemDefinition definition)
        {
            if (definition != null && !knownDefinitions.Contains(definition))
            {
                knownDefinitions.Add(definition);
            }
        }

        public void RegisterSetBonus(EquipmentSetBonusDefinition setBonus)
        {
            if (setBonus != null && !setBonuses.Contains(setBonus))
            {
                setBonuses.Add(setBonus);
            }
        }

        public void Clear()
        {
            equipped.Clear();
            EnsureSlots();
        }

        private void EnsureSlots()
        {
            EnsureSlot(EquipmentSlotType.MainHand);
            EnsureSlot(EquipmentSlotType.OffHand);
            EnsureSlot(EquipmentSlotType.Head);
            EnsureSlot(EquipmentSlotType.Chest);
            EnsureSlot(EquipmentSlotType.Legs);
            EnsureSlot(EquipmentSlotType.Feet);
            EnsureSlot(EquipmentSlotType.Cloak);
            EnsureSlot(EquipmentSlotType.Amulet);
            EnsureSlot(EquipmentSlotType.Ring);
            EnsureSlot(EquipmentSlotType.Focus);
            EnsureSlot(EquipmentSlotType.Tool);
            EnsureSlot(EquipmentSlotType.BoatUpgrade);
            EnsureSlot(EquipmentSlotType.CreatureGear);
        }

        private void EnsureSlot(EquipmentSlotType slot)
        {
            if (equipped.Find(e => e.Slot == slot) == null)
            {
                equipped.Add(new EquippedItemRecord { Slot = slot });
            }
        }
    }
}
