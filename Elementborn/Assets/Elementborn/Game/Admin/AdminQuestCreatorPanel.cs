using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Basic Unity UI admin panel for creating a custom quest at runtime.
    /// Wire these InputFields/Buttons in a Canvas, similar to a cheat/admin panel.
    /// </summary>
    public sealed class AdminQuestCreatorPanel : MonoBehaviour
    {
        [Header("Quest Fields")]
        [SerializeField] private InputField questIdInput;
        [SerializeField] private InputField titleInput;
        [SerializeField] private InputField descriptionInput;
        [SerializeField] private InputField regionInput;
        [SerializeField] private InputField giverInput;

        [Header("Objective Fields")]
        [SerializeField] private InputField objectiveIdInput;
        [SerializeField] private InputField objectiveTitleInput;
        [SerializeField] private InputField objectiveDescriptionInput;
        [SerializeField] private InputField objectiveXInput;
        [SerializeField] private InputField objectiveYInput;
        [SerializeField] private InputField objectiveZInput;

        [Header("Reward Fields")]
        [SerializeField] private InputField rewardItemIdInput;
        [SerializeField] private InputField rewardQuantityInput;
        [SerializeField] private InputField rewardCurrencyInput;

        [Header("Status")]
        [SerializeField] private Text statusText;

        public void CreateOrUpdateQuest()
        {
            var quest = BuildQuestFromFields();
            AdminQuestService.Ensure().CreateOrUpdate(quest);
            SetStatus($"Saved quest: {quest.Title}");
        }

        public void CreateAndActivateQuest()
        {
            var quest = BuildQuestFromFields();
            AdminQuestService.Ensure().CreateOrUpdate(quest);
            AdminQuestService.Ensure().ActivateQuest(quest.QuestId);
            SetStatus($"Created and activated: {quest.Title}");
        }

        public void ActivateQuestFromField()
        {
            string id = GetText(questIdInput, "custom_quest");
            bool ok = AdminQuestService.Ensure().ActivateQuest(id);
            SetStatus(ok ? $"Activated quest: {id}" : $"Could not activate quest: {id}");
        }

        public void CompleteQuestFromField()
        {
            string id = GetText(questIdInput, "custom_quest");
            bool ok = AdminQuestService.Ensure().CompleteQuest(id);
            SetStatus(ok ? $"Completed quest: {id}" : $"Could not complete quest: {id}");
        }

        public void DeleteQuestFromField()
        {
            string id = GetText(questIdInput, "custom_quest");
            bool ok = AdminQuestService.Ensure().Delete(id);
            SetStatus(ok ? $"Deleted quest: {id}" : $"Could not delete quest: {id}");
        }

        public void ReloadQuestDatabase()
        {
            AdminQuestService.Ensure().Load();
            SetStatus("Reloaded admin quest database.");
        }

        public void SaveQuestDatabase()
        {
            AdminQuestService.Ensure().Save();
            SetStatus("Saved admin quest database.");
        }

        private AdminQuestRecord BuildQuestFromFields()
        {
            var quest = new AdminQuestRecord
            {
                QuestId = GetText(questIdInput, "custom_quest"),
                Title = GetText(titleInput, "Custom Quest"),
                Description = GetText(descriptionInput, ""),
                Region = GetText(regionInput, ""),
                GiverName = GetText(giverInput, "Admin"),
                Enabled = true
            };

            var objective = new AdminQuestObjectiveRecord
            {
                ObjectiveId = GetText(objectiveIdInput, "objective"),
                Title = GetText(objectiveTitleInput, quest.Title),
                Description = GetText(objectiveDescriptionInput, quest.Description),
                HasWorldPosition = true
            };

            objective.X = GetFloat(objectiveXInput, 0f);
            objective.Y = GetFloat(objectiveYInput, 0f);
            objective.Z = GetFloat(objectiveZInput, 0f);
            quest.Objectives.Add(objective);

            string rewardItemId = GetText(rewardItemIdInput, "");
            int rewardQty = GetInt(rewardQuantityInput, 0);
            int currency = GetInt(rewardCurrencyInput, 0);
            if (!string.IsNullOrWhiteSpace(rewardItemId) || rewardQty > 0 || currency > 0)
            {
                quest.Rewards.Add(new AdminQuestRewardRecord
                {
                    ItemId = rewardItemId,
                    Quantity = Mathf.Max(0, rewardQty),
                    Currency = Mathf.Max(0, currency)
                });
            }

            return quest;
        }

        private static string GetText(InputField field, string fallback)
        {
            return field != null && !string.IsNullOrWhiteSpace(field.text) ? field.text : fallback;
        }

        private static int GetInt(InputField field, int fallback)
        {
            if (field == null || !int.TryParse(field.text, out int value))
            {
                return fallback;
            }

            return value;
        }

        private static float GetFloat(InputField field, float fallback)
        {
            if (field == null || !float.TryParse(field.text, out float value))
            {
                return fallback;
            }

            return value;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            NotificationFeed.Post(message, NotificationType.Quest);
        }
    }
}
