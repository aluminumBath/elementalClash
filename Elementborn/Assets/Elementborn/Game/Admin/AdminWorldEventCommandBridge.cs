using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AdminWorldEventCommandBridge : MonoBehaviour
    {
        [SerializeField] private WorldEventDirector director;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;
            string trimmed = command.Trim();
            if (trimmed.StartsWith("event.activate "))
            {
                string id = trimmed.Substring("event.activate ".Length).Trim();
                return director != null && director.ActivateById(id);
            }
            if (trimmed.StartsWith("event.schedule "))
            {
                string id = trimmed.Substring("event.schedule ".Length).Trim();
                return director != null && director.ScheduleById(id);
            }
            if (trimmed.StartsWith("event.complete "))
            {
                string id = trimmed.Substring("event.complete ".Length).Trim();
                return WorldEventTracker.Complete(id, "Admin command");
            }
            if (trimmed.StartsWith("event.cancel "))
            {
                string id = trimmed.Substring("event.cancel ".Length).Trim();
                return WorldEventTracker.Cancel(id, "Admin command");
            }
            if (trimmed == "event.list")
            {
                foreach (var record in WorldEventTracker.Ensure().Records)
                {
                    if (record != null) Debug.Log($"{record.EventId} | {record.DisplayName} | {record.State}");
                }
                return true;
            }
            return false;
        }
    }
}
