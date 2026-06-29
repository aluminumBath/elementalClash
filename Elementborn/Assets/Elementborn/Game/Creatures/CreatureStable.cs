using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureStable : MonoBehaviour
    {
        [SerializeField] private string stableId = "";
        [SerializeField] private string displayName = "Stable";
        [SerializeField] private int capacity = 8;
        [SerializeField] private List<string> storedCreatureRecordIds = new List<string>();

        public string StableId => string.IsNullOrWhiteSpace(stableId) ? name : stableId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? StableId : displayName;
        public IReadOnlyList<string> StoredCreatureRecordIds => storedCreatureRecordIds;
        public int Capacity => Mathf.Max(1, capacity);

        private void Start()
        {
            PlayerMapMarkerTracker.ReportStable(transform.position, DisplayName);
        }

        public bool Store(string recordId)
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                return false;
            }

            if (storedCreatureRecordIds.Contains(recordId))
            {
                return true;
            }

            if (storedCreatureRecordIds.Count >= Capacity)
            {
                NotificationFeed.Post("Stable is full.", NotificationType.Warning);
                return false;
            }

            storedCreatureRecordIds.Add(recordId);
            CreatureBondingTracker.AssignStable(recordId, StableId, transform.position);
            return true;
        }

        public bool ReleaseToFollow(string recordId)
        {
            if (!storedCreatureRecordIds.Remove(recordId))
            {
                return false;
            }

            CreatureBondingTracker.SetCreatureState(recordId, CreatureRideState.Following, transform.position);
            return true;
        }

        public bool StoreFirstAvailable()
        {
            var creature = CreatureBondingTracker.GetFirstAvailable();
            return creature != null && Store(creature.RecordId);
        }

        public bool ReleaseFirstStored()
        {
            if (storedCreatureRecordIds.Count == 0)
            {
                NotificationFeed.Post("No creatures are stored here.", NotificationType.Warning);
                return false;
            }

            return ReleaseToFollow(storedCreatureRecordIds[0]);
        }

        public void ImportStored(string recordId)
        {
            if (!storedCreatureRecordIds.Contains(recordId))
            {
                storedCreatureRecordIds.Add(recordId);
            }
        }

        public void Clear()
        {
            storedCreatureRecordIds.Clear();
        }
    }
}
