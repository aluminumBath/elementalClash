using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for custom quests.
    /// Wire ExecuteCommand(string) to your existing cheat-code/admin input field.
    ///
    /// Commands:
    /// quest.create id|title|description|x|y|z
    /// quest.start id
    /// quest.complete id
    /// quest.delete id
    /// quest.reload
    /// quest.save
    /// </summary>
    public sealed class AdminQuestCheatCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed.StartsWith("quest.create "))
            {
                return Create(trimmed.Substring("quest.create ".Length));
            }

            if (trimmed.StartsWith("quest.start "))
            {
                string id = trimmed.Substring("quest.start ".Length).Trim();
                return AdminQuestService.Ensure().ActivateQuest(id);
            }

            if (trimmed.StartsWith("quest.complete "))
            {
                string id = trimmed.Substring("quest.complete ".Length).Trim();
                return AdminQuestService.Ensure().CompleteQuest(id);
            }

            if (trimmed.StartsWith("quest.delete "))
            {
                string id = trimmed.Substring("quest.delete ".Length).Trim();
                return AdminQuestService.Ensure().Delete(id);
            }

            if (trimmed == "quest.reload")
            {
                AdminQuestService.Ensure().Load();
                NotificationFeed.Post("Admin quests reloaded.", NotificationType.Quest);
                return true;
            }

            if (trimmed == "quest.save")
            {
                AdminQuestService.Ensure().Save();
                NotificationFeed.Post("Admin quests saved.", NotificationType.Quest);
                return true;
            }

            return false;
        }

        private bool Create(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length < 3)
            {
                NotificationFeed.Post("Usage: quest.create id|title|description|x|y|z", NotificationType.Warning);
                return false;
            }

            var quest = new AdminQuestRecord
            {
                QuestId = parts[0].Trim(),
                Title = parts[1].Trim(),
                Description = parts[2].Trim(),
                GiverName = "Admin",
                Enabled = true
            };

            var objective = new AdminQuestObjectiveRecord
            {
                ObjectiveId = "objective",
                Title = quest.Title,
                Description = quest.Description,
                HasWorldPosition = parts.Length >= 6
            };

            if (parts.Length >= 6)
            {
                float.TryParse(parts[3], out objective.X);
                float.TryParse(parts[4], out objective.Y);
                float.TryParse(parts[5], out objective.Z);
            }

            quest.Objectives.Add(objective);
            AdminQuestService.Ensure().CreateOrUpdate(quest);
            return true;
        }
    }
}
