using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class WorldEventDirector : MonoBehaviour
    {
        [SerializeField] private List<WorldEventDefinition> events = new List<WorldEventDefinition>();
        [SerializeField] private bool scheduleOnStart = false;
        [SerializeField] private bool activateOnStart = false;

        private void Start()
        {
            if (scheduleOnStart) ScheduleAll();
            if (activateOnStart) ActivateAll();
        }

        public void ScheduleAll()
        {
            foreach (var worldEvent in events)
            {
                if (worldEvent != null) WorldEventTracker.Schedule(worldEvent, "Director schedule");
            }
        }

        public void ActivateAll()
        {
            foreach (var worldEvent in events)
            {
                if (worldEvent != null) WorldEventTracker.Activate(worldEvent, "Director activation");
            }
        }

        public bool ActivateById(string eventId)
        {
            foreach (var worldEvent in events)
            {
                if (worldEvent != null && worldEvent.EventId == eventId)
                {
                    WorldEventTracker.Activate(worldEvent, "Director activation by id");
                    return true;
                }
            }
            NotificationFeed.Post($"World event definition not assigned: {eventId}", NotificationType.Warning);
            return false;
        }

        public bool ScheduleById(string eventId)
        {
            foreach (var worldEvent in events)
            {
                if (worldEvent != null && worldEvent.EventId == eventId)
                {
                    WorldEventTracker.Schedule(worldEvent, "Director schedule by id");
                    return true;
                }
            }
            return false;
        }

        public IReadOnlyList<WorldEventDefinition> GetEvents()
        {
            return events;
        }
    }
}
