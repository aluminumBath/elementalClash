using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Lightweight runtime diagnostics for the generated Elementborn prototype systems.
    /// Attach to a scene object or create through the editor hardening menu.
    /// </summary>
    public sealed class ElementbornIntegrationDiagnostics : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool logPassedChecks = false;
        [SerializeField] private List<string> lastMessages = new List<string>();

        public IReadOnlyList<string> LastMessages => lastMessages;

        private void Start()
        {
            if (runOnStart)
            {
                RunDiagnostics();
            }
        }

        [ContextMenu("Run Elementborn Diagnostics")]
        public void RunDiagnostics()
        {
            lastMessages.Clear();

            CheckSingleton<PlayerInventoryTracker>("PlayerInventoryTracker");
            CheckSingleton<PlayerMapMarkerTracker>("PlayerMapMarkerTracker");
            CheckSingleton<WaypointTracker>("WaypointTracker");
            CheckSingleton<NotificationFeed>("NotificationFeed");
            CheckSingleton<PlayerJournalTracker>("PlayerJournalTracker");
            CheckSingleton<FactionReputationTracker>("FactionReputationTracker");
            CheckSingleton<PlayerAbilityTracker>("PlayerAbilityTracker");
            CheckSingleton<PlayerEquipmentTracker>("PlayerEquipmentTracker");
            CheckSingleton<SpellCooldownTracker>("SpellCooldownTracker");
            CheckSingleton<QuestUiTracker>("QuestUiTracker");

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Warn("No GameObject tagged Player found. The playable setup builder should create one.");
            }
            else
            {
                CheckComponent<PlayerInteractor>(player, "PlayerInteractor");
                CheckComponent<SimpleCombatHealth>(player, "SimpleCombatHealth");
                CheckComponent<StaminaResource>(player, "StaminaResource");
                CheckComponent<CombatDefenseController>(player, "CombatDefenseController");
                CheckComponent<SpellCastController>(player, "SpellCastController");
                CheckComponent<SpellLoadoutController>(player, "SpellLoadoutController");
            }

            if (ElementbornFindUtility.FindFirst<Canvas>() == null)
            {
                Warn("No Canvas found. Run Elementborn → Playable Setup → Create Quest/Combat UI Canvas.");
            }

            if (ElementbornFindUtility.FindFirst<EventSystemCompatMarker>() == null && UnityEngine.EventSystems.EventSystem.current == null)
            {
                Warn("No active EventSystem found. UI buttons and input modules may not work.");
            }

            if (lastMessages.Count == 0)
            {
                Pass("No obvious runtime integration issues detected by diagnostics.");
            }
        }

        private void CheckSingleton<T>(string label) where T : Component
        {
            T found = ElementbornFindUtility.FindFirst<T>();
            if (found == null)
            {
                Warn($"Missing runtime system: {label}. ElementbornRuntimeBootstrap can create this.");
            }
            else if (logPassedChecks)
            {
                Pass($"Found {label}.");
            }
        }

        private void CheckComponent<T>(GameObject go, string label) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                Warn($"Player is missing {label}.");
            }
            else if (logPassedChecks)
            {
                Pass($"Player has {label}.");
            }
        }

        private void Warn(string message)
        {
            lastMessages.Add(message);
            Debug.LogWarning("[Elementborn Diagnostics] " + message, this);
        }

        private void Pass(string message)
        {
            if (logPassedChecks)
            {
                lastMessages.Add(message);
                Debug.Log("[Elementborn Diagnostics] " + message, this);
            }
        }

        private sealed class EventSystemCompatMarker : MonoBehaviour { }
    }
}
