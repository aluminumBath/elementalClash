using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Optional bootstrap helper that seeds useful default facts/lore for early prototype scenes.
    /// Add once to a bootstrap scene if you want NPCs to know basic world facts before custom quest setup.
    /// </summary>
    public sealed class ElementbornDefaultKnowledgeSeeder : MonoBehaviour
    {
        [SerializeField] private bool seedOnStart = true;

        private void Start()
        {
            if (seedOnStart)
            {
                Seed();
            }
        }

        [ContextMenu("Seed Default Elementborn Knowledge")]
        public void Seed()
        {
            DialogueMemoryTracker.Remember(
                DialogueMemoryType.LocationFact,
                "Neritha Reefwood",
                "Neritha Reefwood is a giant coral forest island where water channelers are common.",
                "World",
                "Neritha Reefwood",
                important: true);

            DialogueMemoryTracker.Remember(
                DialogueMemoryType.WorldFact,
                "Boats",
                "Boats move best when the sail is raised with the wind behind them. Lower the sail when fighting a headwind.",
                "Kram",
                important: true);

            DialogueMemoryTracker.Remember(
                DialogueMemoryType.CreatureFact,
                "Last Ridden Creature",
                "The last creature the player rode can be tracked from the map.",
                "Map",
                important: true);

            PlayerJournalTracker.AddLocation(
                "Neritha Reefwood",
                "A vast coral forest island rising above the sea. Water channelers are common here.",
                "Neritha Reefwood");

            PlayerJournalTracker.AddOrUpdateEntry(
                "boat_sailing_basics",
                JournalEntryType.Boat,
                "Sailing Basics",
                "Raise the sail when the wind is favorable. Lower it and paddle when the wind turns against you.");

            PlayerJournalTracker.AddFaction(
                "Unification Circle",
                "A faction that argues that mixed bloodlines and elemental cooperation are strengths, not threats.");

            PlayerJournalTracker.AddFaction(
                "Element Supremacists",
                "A dangerous faction that believes elemental purity should determine power and status.");
        }
    }
}
