using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class NamedShipRegistry : MonoBehaviour
    {
        public static NamedShipRegistry Instance { get; private set; }

        [SerializeField] private List<NamedShipDefinition> ships = new List<NamedShipDefinition>();
        [SerializeField] private bool registerMapMarkersOnStart = true;
        [SerializeField] private bool registerJournalEntriesOnStart = true;

        public IReadOnlyList<NamedShipDefinition> Ships => ships;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (registerMapMarkersOnStart)
            {
                RegisterMapMarkers();
            }

            if (registerJournalEntriesOnStart)
            {
                RegisterJournalEntries();
            }
        }

        public static NamedShipRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(NamedShipRegistry));
            return go.AddComponent<NamedShipRegistry>();
        }

        public void SetShips(List<NamedShipDefinition> values)
        {
            ships = values ?? new List<NamedShipDefinition>();
        }

        public NamedShipDefinition Find(string shipId)
        {
            return ships.Find(s => s != null && s.ShipId == shipId);
        }

        public void RegisterMapMarkers()
        {
            foreach (var ship in ships)
            {
                if (ship == null)
                {
                    continue;
                }

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "ship_" + PlayerJournalTracker.Safe(ship.ShipId),
                    MapMarkerType.Boat,
                    ship.WorldPosition,
                    ship.DisplayName,
                    true,
                    $"{ship.DockLocation} — {ship.Description}");
            }
        }

        public void RegisterJournalEntries()
        {
            foreach (var ship in ships)
            {
                if (ship == null)
                {
                    continue;
                }

                PlayerJournalTracker.AddOrUpdateEntry(
                    "ship_" + PlayerJournalTracker.Safe(ship.ShipId),
                    JournalEntryType.Location,
                    ship.DisplayName,
                    BuildShipSummary(ship),
                    ship.Region,
                    ship.ShipId);
            }
        }

        public string BuildShipSummary(NamedShipDefinition ship)
        {
            if (ship == null)
            {
                return "";
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(ship.Description);
            sb.AppendLine($"Dock: {ship.DockLocation}");
            sb.AppendLine($"Captain: {(ship.Captain != null ? ship.Captain.DisplayName : "Unknown")}");
            sb.AppendLine($"First Mate: {(ship.FirstMate != null ? ship.FirstMate.DisplayName : "Unknown")}");
            sb.AppendLine($"Reputation: {ship.StartingReputation}");
            sb.AppendLine($"Raid Style: {ship.RaidStyle}");
            sb.AppendLine($"Celebration Style: {ship.CelebrationStyle}");
            sb.AppendLine("Crew:");
            foreach (var member in ship.Crew)
            {
                if (member?.Npc == null)
                {
                    continue;
                }

                sb.AppendLine($"- {member.Npc.DisplayName}, {member.CrewRole}: {member.Station}");
            }
            return sb.ToString();
        }

        public string BuildRegistrySummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Named Ships: {ships.Count}");
            foreach (var ship in ships)
            {
                if (ship != null)
                {
                    sb.AppendLine($"- {ship.DisplayName} [{ship.StartingReputation}] {ship.Region} / {ship.DockLocation}");
                }
            }
            return sb.ToString();
        }
    }
}
