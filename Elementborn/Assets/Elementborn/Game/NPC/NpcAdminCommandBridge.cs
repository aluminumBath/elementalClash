using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Admin command bridge for NPC browsing and simple placement.
    ///
    /// Commands:
    /// npc.list
    /// npc.search text
    /// npc.region regionName
    /// npc.element elementName
    /// npc.id npcId
    /// npc.place npcId
    /// </summary>
    public sealed class NpcAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var registry = NpcAdminRegistry.Ensure();

            if (trimmed == "npc.list")
            {
                Debug.Log(registry.BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("npc.search "))
            {
                Debug.Log(registry.BuildSummary(new NpcAdminFilter { SearchText = trimmed.Substring("npc.search ".Length).Trim() }));
                return true;
            }

            if (trimmed.StartsWith("npc.region "))
            {
                Debug.Log(registry.BuildSummary(new NpcAdminFilter { Region = trimmed.Substring("npc.region ".Length).Trim() }));
                return true;
            }

            if (trimmed.StartsWith("npc.element "))
            {
                Debug.Log(registry.BuildSummary(new NpcAdminFilter { Element = trimmed.Substring("npc.element ".Length).Trim() }));
                return true;
            }

            if (trimmed.StartsWith("npc.id "))
            {
                var npc = registry.FindById(trimmed.Substring("npc.id ".Length).Trim());
                Debug.Log(npc != null
                    ? $"{npc.DisplayName}\n{npc.TitleOrRank}\n{npc.Region} / {npc.LocationName}\n{npc.PrimaryElement}+{npc.SecondaryElement}\n{npc.Notes}"
                    : "NPC not found.");
                return true;
            }

            if (trimmed.StartsWith("npc.place "))
            {
                string npcId = trimmed.Substring("npc.place ".Length).Trim();
                var npc = registry.FindById(npcId);
                if (npc == null)
                {
                    Debug.LogWarning($"NPC not found: {npcId}");
                    return true;
                }

                var tool = gameObject.GetComponent<NpcPlacementAdminTool>();
                if (tool == null)
                {
                    tool = gameObject.AddComponent<NpcPlacementAdminTool>();
                }

                tool.SetNpc(npc);
                tool.SpawnPlaceholder();
                return true;
            }

            return false;
        }
    }
}
