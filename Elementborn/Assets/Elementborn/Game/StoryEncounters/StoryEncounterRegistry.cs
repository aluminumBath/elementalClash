using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class StoryEncounterRegistry : MonoBehaviour
    {
        public static StoryEncounterRegistry Instance { get; private set; }

        [SerializeField] private List<StoryEncounterDefinition> encounters = new List<StoryEncounterDefinition>();
        [SerializeField] private bool registerJournalAndMapOnStart = true;

        public IReadOnlyList<StoryEncounterDefinition> Encounters => encounters;

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
            if (registerJournalAndMapOnStart)
            {
                RegisterJournalAndMap();
            }
        }

        public static StoryEncounterRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(StoryEncounterRegistry));
            return go.AddComponent<StoryEncounterRegistry>();
        }

        public void SetEncounters(List<StoryEncounterDefinition> values)
        {
            encounters = values ?? new List<StoryEncounterDefinition>();
        }

        public StoryEncounterDefinition Find(string encounterId)
        {
            return encounters.Find(e => e != null && e.EncounterId == encounterId);
        }

        public void RegisterJournalAndMap()
        {
            foreach (StoryEncounterDefinition encounter in encounters)
            {
                if (encounter == null)
                {
                    continue;
                }

                PlayerJournalTracker.AddOrUpdateEntry(
                    "story_encounter_" + PlayerJournalTracker.Safe(encounter.EncounterId),
                    JournalEntryType.Quest,
                    encounter.DisplayName,
                    encounter.PlayerFacingSummary + "\n\nMechanics: " + encounter.MechanicsNotes,
                    encounter.RelatedCapital.ToString(),
                    encounter.EncounterId);

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "story_encounter_" + PlayerJournalTracker.Safe(encounter.EncounterId),
                    MapMarkerType.QuestObjective,
                    encounter.WorldPosition,
                    encounter.DisplayName,
                    true,
                    encounter.PlayerFacingSummary);
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Story Encounters");
            foreach (StoryEncounterDefinition encounter in encounters)
            {
                if (encounter != null)
                {
                    sb.AppendLine($"- {encounter.DisplayName} [{encounter.ThreatType}] {encounter.RelatedCapital}");
                }
            }

            return sb.ToString();
        }
    }
}
