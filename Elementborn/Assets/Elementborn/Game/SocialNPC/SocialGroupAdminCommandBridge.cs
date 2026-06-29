using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// socialgroup.summary
    /// socialgroup.register
    /// socialgroup.event eventId
    /// socialgroup.next
    /// </summary>
    public sealed class SocialGroupAdminCommandBridge : MonoBehaviour
    {
        [SerializeField] private SocialGroupScheduleDirector scheduleDirector;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var registry = SocialGroupRegistry.Ensure();

            if (trimmed == "socialgroup.summary")
            {
                Debug.Log(registry.BuildSummary());
                return true;
            }

            if (trimmed == "socialgroup.register")
            {
                registry.RegisterJournalAndMap();
                return true;
            }

            if (trimmed.StartsWith("socialgroup.event "))
            {
                registry.ActivateEvent(trimmed.Substring("socialgroup.event ".Length).Trim());
                return true;
            }

            if (trimmed == "socialgroup.next")
            {
                if (scheduleDirector == null)
                {
                    scheduleDirector = GetComponent<SocialGroupScheduleDirector>();
                }

                if (scheduleDirector != null)
                {
                    scheduleDirector.ActivateNextEvent();
                }
                return true;
            }

            return false;
        }
    }
}
