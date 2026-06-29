using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// ship.list
    /// ship.id shipId
    /// ship.celebrate shipId
    /// ship.raid shipId
    /// ship.rep shipId|amount
    /// ship.reps
    /// </summary>
    public sealed class ShipAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var registry = NamedShipRegistry.Ensure();

            if (trimmed == "ship.list")
            {
                Debug.Log(registry.BuildRegistrySummary());
                return true;
            }

            if (trimmed == "ship.reps")
            {
                Debug.Log(ShipReputationTracker.Ensure().BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("ship.id "))
            {
                string id = trimmed.Substring("ship.id ".Length).Trim();
                var ship = registry.Find(id);
                Debug.Log(ship != null ? registry.BuildShipSummary(ship) : "Ship not found.");
                return true;
            }

            if (trimmed.StartsWith("ship.celebrate "))
            {
                string id = trimmed.Substring("ship.celebrate ".Length).Trim();
                var ship = registry.Find(id);
                var controller = GetOrCreateCelebrationController(ship);
                controller.TriggerCelebration();
                return true;
            }

            if (trimmed.StartsWith("ship.raid "))
            {
                string id = trimmed.Substring("ship.raid ".Length).Trim();
                var ship = registry.Find(id);
                var controller = GetOrCreateCelebrationController(ship);
                controller.TriggerRaidVictory();
                return true;
            }

            if (trimmed.StartsWith("ship.rep "))
            {
                string payload = trimmed.Substring("ship.rep ".Length).Trim();
                string[] parts = payload.Split('|');
                string id = parts.Length > 0 ? parts[0].Trim() : "";
                int amount = parts.Length > 1 && int.TryParse(parts[1], out int parsed) ? parsed : 0;
                ShipReputationTracker.Ensure().AddReputation(id, amount);
                return true;
            }

            return false;
        }

        private ShipRaidCelebrationController GetOrCreateCelebrationController(NamedShipDefinition ship)
        {
            var controller = gameObject.GetComponent<ShipRaidCelebrationController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<ShipRaidCelebrationController>();
            }

            controller.SetShip(ship);
            return controller;
        }
    }
}
