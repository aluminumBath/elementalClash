using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// fire.summary
    /// fire.hook hookId
    /// fire.resolve hookId
    /// fire.volcano
    /// fire.calm
    /// </summary>
    public sealed class FireCapitalAdminCommandBridge : MonoBehaviour
    {
        [SerializeField] private FireCapitalVolcanoHazardController volcano;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            if (trimmed == "fire.summary")
            {
                Debug.Log(FireCapitalRegistry.Ensure().BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("fire.hook "))
            {
                FireCapitalRegistry.Ensure().StartHook(trimmed.Substring("fire.hook ".Length).Trim());
                return true;
            }

            if (trimmed.StartsWith("fire.resolve "))
            {
                FireCapitalRegistry.Ensure().ResolveHook(trimmed.Substring("fire.resolve ".Length).Trim(), "Resolved by admin.");
                return true;
            }

            if (trimmed == "fire.volcano")
            {
                if (volcano == null) volcano = GetComponent<FireCapitalVolcanoHazardController>();
                if (volcano != null) volcano.PulseVolcanoPressure();
                return true;
            }

            if (trimmed == "fire.calm")
            {
                if (volcano == null) volcano = GetComponent<FireCapitalVolcanoHazardController>();
                if (volcano != null) volcano.CalmVolcano();
                return true;
            }

            return false;
        }
    }
}
