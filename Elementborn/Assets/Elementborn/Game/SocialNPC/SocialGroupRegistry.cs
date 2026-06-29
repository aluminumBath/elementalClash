using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class SocialGroupRegistry : MonoBehaviour
    {
        public static SocialGroupRegistry Instance { get; private set; }

        [SerializeField] private List<SocialNpcGroupDefinition> groups = new List<SocialNpcGroupDefinition>();
        [SerializeField] private List<SocialGroupEventDefinition> events = new List<SocialGroupEventDefinition>();
        [SerializeField] private List<SocialGroupRuntimeRecord> runtimeRecords = new List<SocialGroupRuntimeRecord>();
        [SerializeField] private bool registerJournalAndMapOnStart = true;

        public IReadOnlyList<SocialNpcGroupDefinition> Groups => groups;
        public IReadOnlyList<SocialGroupEventDefinition> Events => events;
        public IReadOnlyList<SocialGroupRuntimeRecord> RuntimeRecords => runtimeRecords;

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

        public static SocialGroupRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(SocialGroupRegistry));
            return go.AddComponent<SocialGroupRegistry>();
        }

        public void SetData(List<SocialNpcGroupDefinition> groupValues, List<SocialGroupEventDefinition> eventValues)
        {
            groups = groupValues ?? new List<SocialNpcGroupDefinition>();
            events = eventValues ?? new List<SocialGroupEventDefinition>();
        }

        public SocialNpcGroupDefinition FindGroup(string groupId)
        {
            string needle = (groupId ?? "").Trim().ToLowerInvariant();
            return groups.Find(g => g != null && (g.GroupId ?? "").ToLowerInvariant() == needle);
        }

        public SocialGroupEventDefinition FindEvent(string eventId)
        {
            string needle = (eventId ?? "").Trim().ToLowerInvariant();
            return events.Find(e => e != null && (e.EventId ?? "").ToLowerInvariant() == needle);
        }

        public SocialGroupRuntimeRecord GetOrCreateRecord(string groupId)
        {
            SocialGroupRuntimeRecord record = runtimeRecords.Find(r => r != null && r.GroupId == groupId);
            if (record != null)
            {
                return record;
            }

            record = new SocialGroupRuntimeRecord { GroupId = groupId };
            runtimeRecords.Add(record);
            return record;
        }

        public void ActivateEvent(string eventId)
        {
            SocialGroupEventDefinition evt = FindEvent(eventId);
            if (evt == null)
            {
                Debug.LogWarning($"Social group event not found: {eventId}");
                return;
            }

            SocialNpcGroupDefinition group = evt.Group;
            SocialGroupRuntimeRecord record = GetOrCreateRecord(group != null ? group.GroupId : "unknown");
            record.LastEventId = evt.EventId;
            record.TimesActivated++;

            if (evt.EventType == SocialGroupEventType.AccidentalFire ||
                evt.EventType == SocialGroupEventType.DomesticArgument ||
                evt.EventType == SocialGroupEventType.RumorDrift)
            {
                record.ChaosLevel = Mathf.Clamp(record.ChaosLevel + 10, 0, 100);
            }
            else if (evt.EventType == SocialGroupEventType.CleanupCrisis ||
                     evt.EventType == SocialGroupEventType.AdviceSession ||
                     evt.EventType == SocialGroupEventType.NeighborhoodProtection)
            {
                record.NeighborhoodTrust = Mathf.Clamp(record.NeighborhoodTrust + 8, -100, 100);
                record.ChaosLevel = Mathf.Clamp(record.ChaosLevel - 8, 0, 100);
            }

            CapitalId capital = group != null ? group.PrimaryCapital : CapitalId.Unknown;
            if (capital != CapitalId.Unknown && evt.PressureDelta != 0)
            {
                CapitalWorldStateTracker.Ensure().AddPressure(capital, evt.PressureType, evt.PressureDelta, evt.DisplayName);
            }

            if (evt.QuestToStart != null)
            {
                QuestUiTracker.StartQuest(evt.QuestToStart);
            }

            if (evt.CreateJournalEntry)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "social_group_event_" + PlayerJournalTracker.Safe(evt.EventId),
                    JournalEntryType.Quest,
                    evt.DisplayName,
                    evt.PlayerFacingSummary + "\n\nDirector notes: " + evt.DirectorNotes,
                    capital.ToString(),
                    evt.EventId);
            }

            if (evt.CreateMapMarker && group != null)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "social_group_event_" + PlayerJournalTracker.Safe(evt.EventId),
                    MapMarkerType.QuestObjective,
                    group.GroupCenter,
                    evt.DisplayName,
                    true,
                    evt.PlayerFacingSummary);
            }

            NotificationFeed.Post(evt.DisplayName + ": " + evt.PlayerFacingSummary, NotificationType.Quest);
        }

        public void RegisterJournalAndMap()
        {
            foreach (SocialNpcGroupDefinition group in groups)
            {
                if (group == null)
                {
                    continue;
                }

                PlayerJournalTracker.AddOrUpdateEntry(
                    "social_group_" + PlayerJournalTracker.Safe(group.GroupId),
                    JournalEntryType.Location,
                    group.DisplayName,
                    group.Summary,
                    group.PrimaryCapital.ToString(),
                    group.GroupId);

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "social_group_" + PlayerJournalTracker.Safe(group.GroupId),
                    MapMarkerType.GuideNpc,
                    group.GroupCenter,
                    group.DisplayName,
                    true,
                    group.Summary);
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Social Groups");
            foreach (SocialNpcGroupDefinition group in groups)
            {
                if (group != null)
                {
                    SocialGroupRuntimeRecord record = GetOrCreateRecord(group.GroupId);
                    sb.AppendLine($"- {group.DisplayName}: members={group.Members.Count}, events={CountEventsForGroup(group)}, trust={record.NeighborhoodTrust}, chaos={record.ChaosLevel}, last={record.LastEventId}");
                }
            }

            if (groups.Count == 0)
            {
                sb.AppendLine("- No social groups loaded.");
            }

            return sb.ToString();
        }

        private int CountEventsForGroup(SocialNpcGroupDefinition group)
        {
            int count = 0;
            foreach (SocialGroupEventDefinition evt in events)
            {
                if (evt != null && evt.Group == group)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
