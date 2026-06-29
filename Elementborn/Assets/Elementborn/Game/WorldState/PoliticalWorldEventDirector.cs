using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class PoliticalWorldEventDirector : MonoBehaviour
    {
        public static PoliticalWorldEventDirector Instance { get; private set; }

        [SerializeField] private List<PoliticalWorldEventDefinition> eventDefinitions = new List<PoliticalWorldEventDefinition>();
        [SerializeField] private List<PoliticalWorldEventRuntimeRecord> runtimeRecords = new List<PoliticalWorldEventRuntimeRecord>();
        [SerializeField] private bool evaluateOnStart = true;
        [SerializeField] private bool autoActivateEligibleEvents = false;
        [SerializeField] private int currentWorldDay = 1;

        public IReadOnlyList<PoliticalWorldEventDefinition> EventDefinitions => eventDefinitions;
        public IReadOnlyList<PoliticalWorldEventRuntimeRecord> RuntimeRecords => runtimeRecords;
        public int CurrentWorldDay => currentWorldDay;

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
            if (evaluateOnStart)
            {
                EvaluateAll();
            }
        }

        public static PoliticalWorldEventDirector Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(PoliticalWorldEventDirector));
            return go.AddComponent<PoliticalWorldEventDirector>();
        }

        public void SetEvents(List<PoliticalWorldEventDefinition> definitions)
        {
            eventDefinitions = definitions ?? new List<PoliticalWorldEventDefinition>();
        }

        public void SetCurrentWorldDay(int day)
        {
            currentWorldDay = Mathf.Max(1, day);
        }

        public void ClearRuntimeRecords()
        {
            runtimeRecords.Clear();
        }

        public void ImportRuntimeRecord(PoliticalWorldEventRuntimeRecord record)
        {
            if (record == null)
            {
                return;
            }

            PoliticalWorldEventRuntimeRecord existing = runtimeRecords.Find(r => r != null && r.EventId == record.EventId);
            if (existing != null)
            {
                runtimeRecords.Remove(existing);
            }

            runtimeRecords.Add(record);
        }

        public void ReplaceRuntimeRecords(List<PoliticalWorldEventRuntimeRecord> records)
        {
            runtimeRecords.Clear();
            if (records == null)
            {
                return;
            }

            foreach (PoliticalWorldEventRuntimeRecord record in records)
            {
                ImportRuntimeRecord(record);
            }
        }


        public void AdvanceDay(int days = 1)
        {
            currentWorldDay += Mathf.Max(1, days);
            EvaluateAll();
        }

        public void EvaluateAll()
        {
            CapitalWorldStateTracker tracker = CapitalWorldStateTracker.Ensure();
            tracker.SyncRegionalSystems();

            foreach (PoliticalWorldEventDefinition definition in eventDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                PoliticalWorldEventRuntimeRecord record = GetOrCreateRecord(definition.EventId);
                if (record.CooldownUntilDay > currentWorldDay)
                {
                    record.State = PoliticalWorldEventState.Cooldown;
                    continue;
                }

                bool eligible = definition.AreConditionsMet(tracker);
                if (!eligible)
                {
                    if (record.State != PoliticalWorldEventState.Resolved)
                    {
                        record.State = PoliticalWorldEventState.Dormant;
                    }
                    continue;
                }

                if (record.State == PoliticalWorldEventState.Dormant || record.State == PoliticalWorldEventState.Cooldown)
                {
                    record.State = PoliticalWorldEventState.Eligible;
                    PoliticalWorldEventHub.RaiseEligible(definition);
                    NotificationFeed.Post($"World event eligible: {definition.DisplayName}", NotificationType.Info);
                    RegisterEligibleJournalAndMarker(definition);
                }

                if (autoActivateEligibleEvents || definition.AutoActivateWhenEligible)
                {
                    Activate(definition.EventId, "auto-activated by world-state conditions");
                }
            }
        }

        public bool Activate(string eventId, string reason = "")
        {
            PoliticalWorldEventDefinition definition = Find(eventId);
            if (definition == null)
            {
                Debug.LogWarning($"Political world event not found: {eventId}");
                return false;
            }

            PoliticalWorldEventRuntimeRecord record = GetOrCreateRecord(definition.EventId);
            if (record.CooldownUntilDay > currentWorldDay)
            {
                Debug.LogWarning($"Political world event is on cooldown: {eventId}");
                return false;
            }

            CapitalWorldStateTracker tracker = CapitalWorldStateTracker.Ensure();

            foreach (PoliticalWorldEventConsequence consequence in definition.Consequences)
            {
                consequence?.Apply(tracker, definition.PrimaryCapital);
            }

            if (definition.QuestToStart != null)
            {
                QuestUiTracker.StartQuest(definition.QuestToStart);
            }

            if (definition.Sound != ElementbornSoundEventId.None)
            {
                ElementbornAudioService.PlayAt(definition.Sound, definition.WorldPosition);
            }

            PlayerJournalTracker.AddOrUpdateEntry(
                "political_event_" + PlayerJournalTracker.Safe(definition.EventId),
                JournalEntryType.Quest,
                definition.DisplayName,
                definition.PlayerFacingSummary + "\n\nDirector notes: " + definition.HiddenDirectorNotes,
                definition.PrimaryCapital.ToString(),
                definition.EventId);

            if (definition.CreateMapMarker)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "political_event_" + PlayerJournalTracker.Safe(definition.EventId),
                    MapMarkerType.QuestObjective,
                    definition.WorldPosition,
                    definition.DisplayName,
                    true,
                    definition.PlayerFacingSummary);
            }

            record.State = PoliticalWorldEventState.Active;
            record.TimesActivated++;
            record.CooldownUntilDay = currentWorldDay + definition.CooldownDays;
            record.LastActivationReason = reason;

            PoliticalWorldEventHub.RaiseActivated(definition);
            NotificationFeed.Post($"World event activated: {definition.DisplayName}", NotificationType.Quest);
            return true;
        }

        public bool Resolve(string eventId, string reason = "")
        {
            PoliticalWorldEventDefinition definition = Find(eventId);
            if (definition == null)
            {
                return false;
            }

            PoliticalWorldEventRuntimeRecord record = GetOrCreateRecord(definition.EventId);
            record.State = PoliticalWorldEventState.Resolved;
            record.LastActivationReason = reason;

            PoliticalWorldEventHub.RaiseResolved(definition);
            NotificationFeed.Post($"World event resolved: {definition.DisplayName}", NotificationType.Info);
            return true;
        }

        public PoliticalWorldEventDefinition Find(string eventId)
        {
            return eventDefinitions.Find(e => e != null && e.EventId == eventId);
        }

        public PoliticalWorldEventRuntimeRecord GetOrCreateRecord(string eventId)
        {
            PoliticalWorldEventRuntimeRecord record = runtimeRecords.Find(r => r != null && r.EventId == eventId);
            if (record != null)
            {
                return record;
            }

            record = new PoliticalWorldEventRuntimeRecord { EventId = eventId };
            runtimeRecords.Add(record);
            return record;
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"World Event Director — Day {currentWorldDay}");
            foreach (PoliticalWorldEventDefinition definition in eventDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                PoliticalWorldEventRuntimeRecord record = GetOrCreateRecord(definition.EventId);
                sb.AppendLine($"- {definition.DisplayName} [{definition.Category}] {record.State}, activated {record.TimesActivated}x");
            }

            return sb.ToString();
        }

        private void RegisterEligibleJournalAndMarker(PoliticalWorldEventDefinition definition)
        {
            PlayerJournalTracker.AddOrUpdateEntry(
                "political_event_eligible_" + PlayerJournalTracker.Safe(definition.EventId),
                JournalEntryType.Quest,
                definition.DisplayName + " — Eligible",
                definition.PlayerFacingSummary,
                definition.PrimaryCapital.ToString(),
                definition.EventId);

            if (definition.CreateMapMarker)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "political_event_eligible_" + PlayerJournalTracker.Safe(definition.EventId),
                    MapMarkerType.QuestObjective,
                    definition.WorldPosition,
                    definition.DisplayName,
                    true,
                    definition.PlayerFacingSummary);
            }
        }
    }
}
