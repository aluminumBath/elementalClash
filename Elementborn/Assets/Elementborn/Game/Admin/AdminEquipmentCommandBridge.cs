using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for equipment.
    ///
    /// Commands:
    /// equip.raw itemId|slot
    /// equip.unequip slot
    /// equip.stat stat
    /// equip.list
    /// </summary>
    public sealed class AdminEquipmentCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed.StartsWith("equip.raw "))
            {
                string payload = trimmed.Substring("equip.raw ".Length).Trim();
                string[] parts = payload.Split('|');
                if (parts.Length < 2)
                {
                    NotificationFeed.Post("Usage: equip.raw itemId|slot", NotificationType.Warning);
                    return true;
                }

                if (!System.Enum.TryParse(parts[1], true, out EquipmentSlotType slot))
                {
                    NotificationFeed.Post($"Unknown slot: {parts[1]}", NotificationType.Warning);
                    return true;
                }

                PlayerEquipmentTracker.EquipByItemId(parts[0].Trim(), slot);
                return true;
            }

            if (trimmed.StartsWith("equip.unequip "))
            {
                string slotText = trimmed.Substring("equip.unequip ".Length).Trim();
                if (System.Enum.TryParse(slotText, true, out EquipmentSlotType slot))
                {
                    PlayerEquipmentTracker.Unequip(slot);
                }

                return true;
            }

            if (trimmed.StartsWith("equip.stat "))
            {
                string statText = trimmed.Substring("equip.stat ".Length).Trim();
                if (System.Enum.TryParse(statText, true, out GearStatType stat))
                {
                    Debug.Log($"{stat}: flat={PlayerEquipmentTracker.GetFlatBonus(stat)}, percent={PlayerEquipmentTracker.GetPercentBonus(stat)}");
                }

                return true;
            }

            if (trimmed == "equip.list")
            {
                foreach (var record in PlayerEquipmentTracker.Ensure().Equipped)
                {
                    if (record != null)
                    {
                        Debug.Log($"{record.Slot}: {record.DisplayName} ({record.ItemId})");
                    }
                }

                return true;
            }

            return false;
        }
    }
}
