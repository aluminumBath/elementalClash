using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for abilities.
    ///
    /// Commands:
    /// ability.unlock id
    /// ability.equip id|slot
    /// ability.points amount
    /// ability.level level
    /// ability.cooldowns.clear
    /// </summary>
    public sealed class AdminAbilityCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed.StartsWith("ability.unlock "))
            {
                string id = trimmed.Substring("ability.unlock ".Length).Trim();
                PlayerAbilityTracker.UnlockById(id, AbilityUnlockSource.Admin);
                return true;
            }

            if (trimmed.StartsWith("ability.equip "))
            {
                string payload = trimmed.Substring("ability.equip ".Length).Trim();
                string[] parts = payload.Split('|');
                if (parts.Length < 2)
                {
                    NotificationFeed.Post("Usage: ability.equip id|slot", NotificationType.Warning);
                    return true;
                }

                if (!System.Enum.TryParse(parts[1], true, out AbilitySlotType slot))
                {
                    NotificationFeed.Post($"Unknown ability slot: {parts[1]}", NotificationType.Warning);
                    return true;
                }

                PlayerAbilityTracker.Equip(parts[0].Trim(), slot);
                return true;
            }

            if (trimmed.StartsWith("ability.points "))
            {
                string value = trimmed.Substring("ability.points ".Length).Trim();
                if (int.TryParse(value, out int points))
                {
                    PlayerAbilityTracker.AddSkillPoints(points, "Admin command");
                }

                return true;
            }

            if (trimmed.StartsWith("ability.level "))
            {
                string value = trimmed.Substring("ability.level ".Length).Trim();
                if (int.TryParse(value, out int level))
                {
                    PlayerAbilityTracker.SetPlayerLevel(level);
                }

                return true;
            }

            if (trimmed == "ability.cooldowns.clear")
            {
                AbilityCooldownTracker.ClearAll();
                NotificationFeed.Post("Ability cooldowns cleared.", NotificationType.Journal);
                return true;
            }

            return false;
        }
    }
}
