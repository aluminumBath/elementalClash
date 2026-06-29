using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// wind.summary
    /// wind.fervor
    /// wind.fervor.add amount
    /// wind.hook id
    /// wind.reveal id
    /// wind.sarah
    /// </summary>
    public sealed class WindCapitalAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var registry = WindCapitalRegistry.Ensure();

            if (trimmed == "wind.summary")
            {
                Debug.Log(registry.BuildSummary());
                return true;
            }

            if (trimmed == "wind.fervor")
            {
                Debug.Log(ReligiousFervorTracker.Ensure().BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("wind.fervor.add "))
            {
                if (int.TryParse(trimmed.Substring("wind.fervor.add ".Length).Trim(), out int amount))
                {
                    ReligiousFervorTracker.Ensure().AddFervor(amount, "admin command");
                }
                return true;
            }

            if (trimmed.StartsWith("wind.hook "))
            {
                string id = trimmed.Substring("wind.hook ".Length).Trim();
                var hook = registry.FindHook(id);
                Debug.Log(hook != null ? $"{hook.Title}\n{hook.Summary}\nSecret: {hook.SecretTruth}" : "Wind hook not found.");
                return true;
            }

            if (trimmed.StartsWith("wind.reveal "))
            {
                string id = trimmed.Substring("wind.reveal ".Length).Trim();
                var hook = registry.FindHook(id);
                WindCapitalSecretTracker.Ensure().Reveal(hook);
                return true;
            }

            if (trimmed == "wind.sarah")
            {
                var hook = registry.FindHook("sarah_wind_capital_exile");
                Debug.Log(hook != null ? $"{hook.Title}\n{hook.Summary}\nSecret: {hook.SecretTruth}" : "Sarah hook not generated yet.");
                return true;
            }

            return false;
        }
    }
}
