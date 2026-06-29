using System;
using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class CreatureBondingTracker : MonoBehaviour
    {
        public static CreatureBondingTracker Instance { get; private set; }

        [SerializeField] private List<OwnedCreatureRecord> ownedCreatures = new List<OwnedCreatureRecord>();
        [SerializeField] private string activeCreatureRecordId = "";
        [SerializeField] private string lastRiddenCreatureRecordId = "";

        public IReadOnlyList<OwnedCreatureRecord> OwnedCreatures => ownedCreatures;
        public OwnedCreatureRecord ActiveCreature => Find(activeCreatureRecordId);
        public OwnedCreatureRecord LastRiddenCreature => Find(lastRiddenCreatureRecordId);

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

        public static CreatureBondingTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(CreatureBondingTracker));
            return go.AddComponent<CreatureBondingTracker>();
        }

        public static OwnedCreatureRecord TameCreature(CreatureDefinition definition, string customName, Vector3 position)
        {
            if (definition == null)
            {
                return null;
            }

            var tracker = Ensure();
            var record = new OwnedCreatureRecord
            {
                RecordId = Guid.NewGuid().ToString("N"),
                CreatureId = definition.CreatureId,
                DisplayName = definition.DisplayName,
                CustomName = customName ?? "",
                TraversalType = definition.TraversalType,
                Temperament = definition.Temperament,
                State = CreatureRideState.Stable,
                BondXp = Mathf.Max(0, definition.BaseBondGain),
                LastKnownPosition = position
            };

            tracker.ownedCreatures.Add(record);

            PlayerJournalTracker.AddCreature(
                record.DisplayName,
                string.IsNullOrWhiteSpace(definition.Description)
                    ? $"A newly tamed {record.DisplayName}."
                    : definition.Description);

            PlayerMapMarkerTracker.ReportLastRiddenCreature(position, record.NameForDisplay);
            NotificationFeed.Post($"Tamed {record.NameForDisplay}.", NotificationType.Journal);

            return record;
        }

        public static bool TryTame(CreatureDefinition definition, string customName, Vector3 position, int bonus = 0)
        {
            if (definition == null)
            {
                return false;
            }

            int chance = Mathf.Clamp(100 - definition.TameDifficulty + bonus, 5, 95);
            bool success = UnityEngine.Random.Range(0, 100) < chance;

            if (success)
            {
                TameCreature(definition, customName, position);
            }
            else
            {
                NotificationFeed.Post($"{definition.DisplayName} resists being tamed.", NotificationType.Warning);
            }

            return success;
        }

        public static bool FeedCreature(string recordId, string itemId, int baseBondGain = 5)
        {
            var record = Ensure().Find(recordId);
            if (record == null)
            {
                return false;
            }

            if (!PlayerInventoryTracker.HasItemId(itemId, 1))
            {
                NotificationFeed.Post($"Missing treat: {itemId}", NotificationType.Warning);
                return false;
            }

            PlayerInventoryTracker.RemoveItemId(itemId, 1);
            record.TimesFed++;
            AddBond(recordId, Mathf.Max(1, baseBondGain), $"Fed {record.NameForDisplay}.");

            return true;
        }

        public static void AddBond(string recordId, int amount, string reason = "")
        {
            var record = Ensure().Find(recordId);
            if (record == null)
            {
                return;
            }

            CreatureBondStage oldStage = record.BondStage;
            record.BondXp = Mathf.Max(0, record.BondXp + amount);
            CreatureBondStage newStage = record.BondStage;

            if (newStage != oldStage)
            {
                NotificationFeed.Post($"{record.NameForDisplay} is now {newStage}.", NotificationType.Journal);
                PlayerJournalTracker.AddCreature(
                    record.NameForDisplay,
                    $"{record.NameForDisplay}'s bond has grown to {newStage}.");
            }
            else if (!string.IsNullOrWhiteSpace(reason))
            {
                NotificationFeed.Post(reason, NotificationType.Info);
            }
        }

        public static void SetCreatureState(string recordId, CreatureRideState state, Vector3 position)
        {
            var tracker = Ensure();
            var record = tracker.Find(recordId);
            if (record == null)
            {
                return;
            }

            record.State = state;
            record.LastKnownPosition = position;

            if (state == CreatureRideState.Ridden)
            {
                record.TimesRidden++;
                tracker.activeCreatureRecordId = recordId;
                tracker.lastRiddenCreatureRecordId = recordId;
                PlayerMapMarkerTracker.ReportLastRiddenCreature(position, record.NameForDisplay);
            }
            else if (state == CreatureRideState.Following)
            {
                tracker.activeCreatureRecordId = recordId;
                PlayerMapMarkerTracker.ReportActiveCompanion(position, record.NameForDisplay);
            }
            else if (state == CreatureRideState.Stable || state == CreatureRideState.Resting)
            {
                if (tracker.activeCreatureRecordId == recordId)
                {
                    tracker.activeCreatureRecordId = "";
                }

                PlayerMapMarkerTracker.ReportLastRiddenCreature(position, record.NameForDisplay);
            }
        }

        public static void AssignStable(string recordId, string stableId, Vector3 stablePosition)
        {
            var record = Ensure().Find(recordId);
            if (record == null)
            {
                return;
            }

            record.StableId = stableId ?? "";
            record.State = CreatureRideState.Stable;
            record.LastKnownPosition = stablePosition;

            PlayerMapMarkerTracker.ReportStable(stablePosition, string.IsNullOrWhiteSpace(stableId) ? "Stable" : stableId);
            NotificationFeed.Post($"{record.NameForDisplay} is resting at the stable.", NotificationType.Journal);
        }

        public static void Rename(string recordId, string newName)
        {
            var record = Ensure().Find(recordId);
            if (record == null)
            {
                return;
            }

            record.CustomName = newName ?? "";
            NotificationFeed.Post($"Creature renamed: {record.NameForDisplay}", NotificationType.Journal);
        }

        public static OwnedCreatureRecord GetFirstAvailable()
        {
            var tracker = Ensure();
            return tracker.ownedCreatures.Count > 0 ? tracker.ownedCreatures[0] : null;
        }

        public OwnedCreatureRecord Find(string recordId)
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                return null;
            }

            return ownedCreatures.Find(c => c.RecordId == recordId);
        }

        public void Clear()
        {
            ownedCreatures.Clear();
            activeCreatureRecordId = "";
            lastRiddenCreatureRecordId = "";
        }

        public void Import(OwnedCreatureRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.RecordId))
            {
                return;
            }

            ownedCreatures.RemoveAll(c => c.RecordId == record.RecordId);
            ownedCreatures.Add(record);
        }
    }
}
