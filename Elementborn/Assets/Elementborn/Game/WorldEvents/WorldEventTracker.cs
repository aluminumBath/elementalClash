using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class WorldEventTracker : MonoBehaviour
    {
        public static WorldEventTracker Instance { get; private set; }

        [SerializeField] private List<WorldEventRecord> records = new List<WorldEventRecord>();

        public IReadOnlyList<WorldEventRecord> Records => records;

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

        private void Update()
        {
            Tick(Time.unscaledTime);
        }

        public static WorldEventTracker Ensure()
        {
            if (Instance != null) return Instance;
            var go = new GameObject(nameof(WorldEventTracker));
            return go.AddComponent<WorldEventTracker>();
        }

        public static WorldEventRecord Schedule(WorldEventDefinition definition, string reason = "")
        {
            if (definition == null) return null;
            var tracker = Ensure();
            var record = tracker.GetOrCreate(definition);
            if (definition.Unique && record.State == WorldEventState.Completed) return record;
            record.State = WorldEventState.Scheduled;
            record.ScheduledAtUnscaledTime = Time.unscaledTime + definition.ActivationDelaySeconds;
            record.LastReason = reason ?? "";
            NotificationFeed.Post($"Event scheduled: {definition.DisplayName}", NotificationType.Map);
            return record;
        }

        public static WorldEventRecord Activate(WorldEventDefinition definition, string reason = "")
        {
            return Ensure().ActivateInternal(definition, reason);
        }

        public static bool Complete(string eventId, string reason = "")
        {
            var record = Ensure().Find(eventId);
            if (record == null) return false;
            record.State = WorldEventState.Completed;
            record.LastReason = reason ?? "";
            PlayerMapMarkerTracker.RemoveMarker("world_event_" + PlayerJournalTracker.Safe(eventId));
            NotificationFeed.Post($"Event complete: {record.DisplayName}", NotificationType.Map);
            return true;
        }

        public static bool Cancel(string eventId, string reason = "")
        {
            var record = Ensure().Find(eventId);
            if (record == null) return false;
            record.State = WorldEventState.Cancelled;
            record.LastReason = reason ?? "";
            PlayerMapMarkerTracker.RemoveMarker("world_event_" + PlayerJournalTracker.Safe(eventId));
            NotificationFeed.Post($"Event cancelled: {record.DisplayName}", NotificationType.Map);
            return true;
        }

        public static bool IsActive(string eventId)
        {
            var record = Ensure().Find(eventId);
            return record != null && record.State == WorldEventState.Active;
        }

        public static bool HasCompleted(string eventId)
        {
            var record = Ensure().Find(eventId);
            return record != null && record.State == WorldEventState.Completed;
        }

        public static List<WorldEventRecord> GetActiveEvents()
        {
            var results = new List<WorldEventRecord>();
            foreach (var record in Ensure().records)
            {
                if (record != null && record.State == WorldEventState.Active) results.Add(record);
            }
            return results;
        }

        public void Import(WorldEventRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.EventId)) return;
            records.RemoveAll(r => r.EventId == record.EventId);
            records.Add(record);
        }

        public void Clear()
        {
            records.Clear();
        }

        private void Tick(float now)
        {
            foreach (var record in records)
            {
                if (record == null) continue;
                if (record.State == WorldEventState.Scheduled && record.ScheduledAtUnscaledTime >= 0f && now >= record.ScheduledAtUnscaledTime)
                {
                    record.State = WorldEventState.Active;
                    record.ActivatedAtUnscaledTime = now;
                    record.TimesActivated++;
                    NotificationFeed.Post($"Event active: {record.DisplayName}", NotificationType.Map);
                }

                if (record.State == WorldEventState.Active && record.IsExpired(now))
                {
                    record.State = WorldEventState.Expired;
                    PlayerMapMarkerTracker.RemoveMarker("world_event_" + PlayerJournalTracker.Safe(record.EventId));
                    NotificationFeed.Post($"Event expired: {record.DisplayName}", NotificationType.Map);
                }
            }
        }

        private WorldEventRecord ActivateInternal(WorldEventDefinition definition, string reason)
        {
            if (definition == null) return null;
            var record = GetOrCreate(definition);
            if (definition.Unique && record.State == WorldEventState.Completed)
            {
                NotificationFeed.Post($"Event already completed: {definition.DisplayName}", NotificationType.Warning);
                return record;
            }

            if (!RequirementsMet(definition, out string requirementReason))
            {
                NotificationFeed.Post(requirementReason, NotificationType.Warning);
                return record;
            }

            record.State = WorldEventState.Active;
            record.ActivatedAtUnscaledTime = Time.unscaledTime;
            record.TimesActivated++;
            record.LastReason = reason ?? "";
            record.ExpiresAtUnscaledTime = definition.DurationSeconds > 0f ? Time.unscaledTime + definition.DurationSeconds : -1f;

            if (definition.AddMapMarker && definition.HasWorldPosition)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "world_event_" + PlayerJournalTracker.Safe(definition.EventId),
                    definition.MarkerType,
                    definition.WorldPosition,
                    definition.DisplayName,
                    isPersistent: false,
                    expiresInSeconds: definition.DurationSeconds > 0f ? definition.DurationSeconds : -1f,
                    notes: definition.Description);
            }

            if (definition.AddRumor)
            {
                string rumor = string.IsNullOrWhiteSpace(definition.RumorText) ? definition.Description : definition.RumorText;
                if (!string.IsNullOrWhiteSpace(rumor))
                {
                    RumorTracker.AddRumor(rumor, ToRumorType(definition.EventType), "World Event", definition.Region, definition.Important, true, definition.WorldPosition, definition.HasWorldPosition);
                }
            }

            if (definition.AddJournalEntry)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "world_event_" + PlayerJournalTracker.Safe(definition.EventId),
                    JournalEntryType.Discovery,
                    definition.DisplayName,
                    definition.Description,
                    definition.Region,
                    definition.EventId);
            }

            foreach (var encounter in definition.Encounters)
            {
                WorldEventEncounterSpawner.SpawnEncounter(encounter, definition.WorldPosition, definition.Region, definition.EventId);
            }

            NotificationFeed.Post($"Event active: {definition.DisplayName}", NotificationType.Map);
            return record;
        }

        private WorldEventRecord GetOrCreate(WorldEventDefinition definition)
        {
            var record = Find(definition.EventId);
            if (record != null) return record;
            record = new WorldEventRecord
            {
                EventId = definition.EventId,
                DisplayName = definition.DisplayName,
                EventType = definition.EventType,
                Region = definition.Region,
                HasWorldPosition = definition.HasWorldPosition,
                WorldPosition = definition.WorldPosition,
                State = WorldEventState.Inactive
            };
            records.Add(record);
            return record;
        }

        private WorldEventRecord Find(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            return records.Find(r => r.EventId == eventId);
        }

        private bool RequirementsMet(WorldEventDefinition definition, out string reason)
        {
            reason = "";
            if (definition.RelatedFaction != ElementbornFactionId.Unknown
                && definition.RequiredFactionReputation > -999
                && FactionReputationTracker.GetValue(definition.RelatedFaction) < definition.RequiredFactionReputation)
            {
                reason = $"World event requires {definition.RelatedFaction} reputation {definition.RequiredFactionReputation}.";
                return false;
            }
            return true;
        }

        private static RumorType ToRumorType(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.RareCreatureSighting => RumorType.Creature,
                WorldEventType.SeaMonster => RumorType.Sea,
                WorldEventType.Storm => RumorType.Weather,
                WorldEventType.WindCurrent => RumorType.Weather,
                WorldEventType.FactionPatrol => RumorType.Faction,
                WorldEventType.MerchantCaravan => RumorType.Location,
                WorldEventType.ResourceRespawn => RumorType.Treasure,
                WorldEventType.BossAwakening => RumorType.Threat,
                WorldEventType.Ambush => RumorType.Threat,
                _ => RumorType.Unknown
            };
        }
    }
}
