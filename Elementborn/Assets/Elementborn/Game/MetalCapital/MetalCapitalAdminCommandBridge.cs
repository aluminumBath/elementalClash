using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// metal.summary
    /// metal.guild
    /// metal.guild.add amount
    /// metal.contact id
    /// metal.hook id
    /// metal.reveal npcId
    /// </summary>
    public sealed class MetalCapitalAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var registry = MetalCapitalRegistry.Ensure();

            if (trimmed == "metal.summary")
            {
                Debug.Log(registry.BuildSummary());
                return true;
            }

            if (trimmed == "metal.guild")
            {
                Debug.Log(ThievesGuildReputationTracker.Ensure().BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("metal.guild.add "))
            {
                if (int.TryParse(trimmed.Substring("metal.guild.add ".Length).Trim(), out int amount))
                {
                    ThievesGuildReputationTracker.Ensure().AddReputation(amount, "admin command");
                }
                return true;
            }

            if (trimmed.StartsWith("metal.contact "))
            {
                string id = trimmed.Substring("metal.contact ".Length).Trim();
                var contact = registry.FindContact(id);
                Debug.Log(contact != null ? $"{contact.DisplayName}\n{contact.Description}\nSecret: {contact.Secret}" : "Contact not found.");
                return true;
            }

            if (trimmed.StartsWith("metal.hook "))
            {
                string id = trimmed.Substring("metal.hook ".Length).Trim();
                var hook = registry.FindHook(id);
                Debug.Log(hook != null ? $"{hook.Title}\n{hook.Summary}\nRumor: {hook.PlayerFacingRumor}\nSecret: {hook.SecretTruth}" : "Hook not found.");
                return true;
            }

            if (trimmed.StartsWith("metal.reveal "))
            {
                string npcId = trimmed.Substring("metal.reveal ".Length).Trim();
                NpcWorldEntryDefinition npc = NpcAdminRegistry.Ensure().FindById(npcId);
                HiddenChannelerSecretTracker.Ensure().RevealSecret(npc, "Revealed through Metal Capital intrigue.");
                return true;
            }

            return false;
        }
    }
}
