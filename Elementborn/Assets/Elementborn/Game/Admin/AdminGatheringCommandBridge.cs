using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for gathering.
    ///
    /// Commands:
    /// gather.respawn.all
    /// gather.stats
    /// gather.give.tool itemId
    /// gather.give.resource itemId|quantity
    /// </summary>
    public sealed class AdminGatheringCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed == "gather.respawn.all")
            {
                foreach (var node in ElementbornFindUtility.FindAll<HarvestableResourceNode>())
                {
                    if (node != null)
                    {
                        node.Respawn();
                    }
                }

                NotificationFeed.Post("All harvest nodes respawned.", NotificationType.Map);
                return true;
            }

            if (trimmed == "gather.stats")
            {
                var tracker = ResourceHarvestingTracker.Ensure();
                Debug.Log($"Harvests={tracker.TotalHarvests}, Rare={tracker.RareHarvests}");
                return true;
            }

            if (trimmed.StartsWith("gather.give.tool "))
            {
                string itemId = trimmed.Substring("gather.give.tool ".Length).Trim();
                PlayerInventoryTracker.AddItemId(itemId, 1);
                return true;
            }

            if (trimmed.StartsWith("gather.give.resource "))
            {
                string payload = trimmed.Substring("gather.give.resource ".Length).Trim();
                string[] parts = payload.Split('|');
                string itemId = parts.Length > 0 ? parts[0].Trim() : "";
                int quantity = 1;
                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out quantity);
                    quantity = Mathf.Max(1, quantity);
                }

                PlayerInventoryTracker.AddItemId(itemId, quantity);
                return true;
            }

            return false;
        }
    }
}
