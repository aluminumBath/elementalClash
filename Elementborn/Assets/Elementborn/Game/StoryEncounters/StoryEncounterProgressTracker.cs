using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class StoryEncounterProgressTracker : MonoBehaviour
    {
        public static StoryEncounterProgressTracker Instance { get; private set; }

        [SerializeField] private List<StoryEncounterRuntimeRecord> records = new List<StoryEncounterRuntimeRecord>();

        public IReadOnlyList<StoryEncounterRuntimeRecord> Records => records;

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

        public static StoryEncounterProgressTracker Ensure()
        {
            if (Instance != null) return Instance;
            GameObject go = new GameObject(nameof(StoryEncounterProgressTracker));
            return go.AddComponent<StoryEncounterProgressTracker>();
        }

        public StoryEncounterRuntimeRecord GetOrCreate(string encounterId)
        {
            StoryEncounterRuntimeRecord record = records.Find(r => r != null && r.EncounterId == encounterId);
            if (record != null) return record;
            record = new StoryEncounterRuntimeRecord { EncounterId = encounterId, State = StoryEncounterState.Discovered };
            records.Add(record);
            return record;
        }

        public void StartEncounter(string encounterId, string notes = "")
        {
            StoryEncounterRuntimeRecord record = GetOrCreate(encounterId);
            record.State = StoryEncounterState.Active;
            record.TimesStarted++;
            record.Notes = notes;
            NotificationFeed.Post($"Encounter started: {encounterId}", NotificationType.Quest);
        }

        public void ResolveEncounter(string encounterId, string notes = "")
        {
            StoryEncounterRuntimeRecord record = GetOrCreate(encounterId);
            record.State = StoryEncounterState.Resolved;
            record.TimesResolved++;
            record.SpecialMechanicComplete = true;
            record.Notes = notes;
            NotificationFeed.Post($"Encounter resolved: {encounterId}", NotificationType.Quest);
        }

        public void FailEncounter(string encounterId, string notes = "")
        {
            StoryEncounterRuntimeRecord record = GetOrCreate(encounterId);
            record.State = StoryEncounterState.Failed;
            record.TimesFailed++;
            record.Notes = notes;
            NotificationFeed.Post($"Encounter failed: {encounterId}", NotificationType.Info);
        }

        public void SetRespawning(string encounterId, string notes = "")
        {
            StoryEncounterRuntimeRecord record = GetOrCreate(encounterId);
            record.State = StoryEncounterState.Respawning;
            record.Notes = notes;
        }

        public void SetAllied(string encounterId, string notes = "")
        {
            StoryEncounterRuntimeRecord record = GetOrCreate(encounterId);
            record.State = StoryEncounterState.Allied;
            record.Notes = notes;
        }

        public void RecordLeaderDefeated(string encounterId, bool primary, float time)
        {
            StoryEncounterRuntimeRecord record = GetOrCreate(encounterId);
            if (primary) record.PrimaryLeaderDefeated = true;
            else record.SecondaryLeaderDefeated = true;
            if (record.FirstLeaderDefeatedTime < 0f) record.FirstLeaderDefeatedTime = time;
        }

        public void SetLastChoice(string encounterId, string choiceId)
        {
            GetOrCreate(encounterId).LastChoiceId = choiceId;
        }

        public void ReplaceRecords(List<StoryEncounterRuntimeRecord> values)
        {
            records.Clear();
            if (values == null) return;
            foreach (StoryEncounterRuntimeRecord record in values)
            {
                if (record != null) records.Add(record);
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Story Encounter Progress");
            if (records.Count == 0) sb.AppendLine("- No encounter progress yet.");
            foreach (StoryEncounterRuntimeRecord record in records)
            {
                if (record != null)
                {
                    sb.AppendLine($"- {record.EncounterId}: {record.State}, started {record.TimesStarted}x, resolved {record.TimesResolved}x, failed {record.TimesFailed}x, choice={record.LastChoiceId}");
                }
            }
            return sb.ToString();
        }
    }
}
