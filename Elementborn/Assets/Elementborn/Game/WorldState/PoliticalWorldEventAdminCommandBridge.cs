using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// eventdir.summary
    /// eventdir.eval
    /// eventdir.day
    /// eventdir.advance days
    /// eventdir.activate eventId
    /// eventdir.resolve eventId
    /// </summary>
    public sealed class PoliticalWorldEventAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var director = PoliticalWorldEventDirector.Ensure();

            if (trimmed == "eventdir.summary")
            {
                Debug.Log(director.BuildSummary());
                return true;
            }

            if (trimmed == "eventdir.eval")
            {
                director.EvaluateAll();
                Debug.Log(director.BuildSummary());
                return true;
            }

            if (trimmed == "eventdir.day")
            {
                Debug.Log($"World event day: {director.CurrentWorldDay}");
                return true;
            }

            if (trimmed.StartsWith("eventdir.advance "))
            {
                int days = int.TryParse(trimmed.Substring("eventdir.advance ".Length).Trim(), out int parsed) ? parsed : 1;
                director.AdvanceDay(days);
                Debug.Log(director.BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("eventdir.activate "))
            {
                string id = trimmed.Substring("eventdir.activate ".Length).Trim();
                director.Activate(id, "admin command");
                return true;
            }

            if (trimmed.StartsWith("eventdir.resolve "))
            {
                string id = trimmed.Substring("eventdir.resolve ".Length).Trim();
                director.Resolve(id, "admin command");
                return true;
            }

            return false;
        }
    }
}
