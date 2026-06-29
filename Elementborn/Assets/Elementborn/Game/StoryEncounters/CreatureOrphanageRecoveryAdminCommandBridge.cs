using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// orphanage.recovery
    /// orphanage.admit creatureId|displayName|reason
    /// orphanage.buy creatureId
    /// orphanage.lure creatureId
    /// orphanage.rehome creatureId
    /// </summary>
    public sealed class CreatureOrphanageRecoveryAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var registry = CreatureOrphanageRecoveryRegistry.Ensure();

            if (trimmed == "orphanage.recovery")
            {
                Debug.Log(registry.BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("orphanage.admit "))
            {
                string[] parts = trimmed.Substring("orphanage.admit ".Length).Split('|');
                string id = parts.Length > 0 ? parts[0].Trim() : "creature";
                string label = parts.Length > 1 ? parts[1].Trim() : id;
                CreatureOrphanageDepartureReason reason = CreatureOrphanageDepartureReason.RanAway;
                if (parts.Length > 2)
                {
                    System.Enum.TryParse(parts[2].Trim(), true, out reason);
                }

                registry.AdmitCreature(id, label, reason, "admin command");
                return true;
            }

            if (trimmed.StartsWith("orphanage.buy "))
            {
                registry.BuyBack(trimmed.Substring("orphanage.buy ".Length).Trim());
                return true;
            }

            if (trimmed.StartsWith("orphanage.lure "))
            {
                registry.LureBack(trimmed.Substring("orphanage.lure ".Length).Trim());
                return true;
            }

            if (trimmed.StartsWith("orphanage.rehome "))
            {
                registry.PermanentlyRehome(trimmed.Substring("orphanage.rehome ".Length).Trim());
                return true;
            }

            return false;
        }
    }
}
