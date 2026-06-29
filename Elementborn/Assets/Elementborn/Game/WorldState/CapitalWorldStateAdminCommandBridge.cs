using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// world.capitals
    /// world.capital CapitalId
    /// world.sync
    /// world.pressure CapitalId|PressureType|delta
    /// world.legitimacy CapitalId|delta
    /// world.stability CapitalId|delta
    /// </summary>
    public sealed class CapitalWorldStateAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;
            string trimmed = command.Trim();
            var tracker = CapitalWorldStateTracker.Ensure();

            if (trimmed == "world.capitals")
            {
                Debug.Log(tracker.BuildWorldSummary());
                return true;
            }

            if (trimmed == "world.sync")
            {
                tracker.SyncRegionalSystems();
                Debug.Log(tracker.BuildWorldSummary());
                return true;
            }

            if (trimmed.StartsWith("world.capital "))
            {
                if (System.Enum.TryParse(trimmed.Substring("world.capital ".Length).Trim(), true, out CapitalId capitalId))
                {
                    Debug.Log(tracker.BuildCapitalSummary(capitalId));
                }
                return true;
            }

            if (trimmed.StartsWith("world.pressure "))
            {
                string[] parts = trimmed.Substring("world.pressure ".Length).Split('|');
                if (parts.Length >= 3 &&
                    System.Enum.TryParse(parts[0], true, out CapitalId capitalId) &&
                    System.Enum.TryParse(parts[1], true, out CapitalPressureType type) &&
                    int.TryParse(parts[2], out int delta))
                {
                    tracker.AddPressure(capitalId, type, delta, "admin command");
                }
                return true;
            }

            if (trimmed.StartsWith("world.legitimacy "))
            {
                string[] parts = trimmed.Substring("world.legitimacy ".Length).Split('|');
                if (parts.Length >= 2 &&
                    System.Enum.TryParse(parts[0], true, out CapitalId capitalId) &&
                    int.TryParse(parts[1], out int delta))
                {
                    tracker.AddLegitimacy(capitalId, delta, "admin command");
                }
                return true;
            }

            if (trimmed.StartsWith("world.stability "))
            {
                string[] parts = trimmed.Substring("world.stability ".Length).Split('|');
                if (parts.Length >= 2 &&
                    System.Enum.TryParse(parts[0], true, out CapitalId capitalId) &&
                    int.TryParse(parts[1], out int delta))
                {
                    tracker.AddStability(capitalId, delta, "admin command");
                }
                return true;
            }

            return false;
        }
    }
}
