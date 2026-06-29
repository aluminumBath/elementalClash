using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// encounter.progress
    /// encounter.start encounterId
    /// encounter.resolve encounterId
    /// encounter.fail encounterId
    /// encounter.choice encounterId|choiceId
    /// </summary>
    public sealed class StoryEncounterProgressAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;
            string trimmed = command.Trim();
            var tracker = StoryEncounterProgressTracker.Ensure();

            if (trimmed == "encounter.progress")
            {
                Debug.Log(tracker.BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("encounter.start "))
            {
                tracker.StartEncounter(trimmed.Substring("encounter.start ".Length).Trim(), "admin command");
                return true;
            }

            if (trimmed.StartsWith("encounter.resolve "))
            {
                tracker.ResolveEncounter(trimmed.Substring("encounter.resolve ".Length).Trim(), "admin command");
                return true;
            }

            if (trimmed.StartsWith("encounter.fail "))
            {
                tracker.FailEncounter(trimmed.Substring("encounter.fail ".Length).Trim(), "admin command");
                return true;
            }

            if (trimmed.StartsWith("encounter.choice "))
            {
                string[] parts = trimmed.Substring("encounter.choice ".Length).Split('|');
                if (parts.Length >= 2) tracker.SetLastChoice(parts[0].Trim(), parts[1].Trim());
                return true;
            }

            return false;
        }
    }
}
