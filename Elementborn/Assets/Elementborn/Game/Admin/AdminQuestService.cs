using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Runtime custom quest database for admin-authored quests.
    /// Saves JSON under Application.persistentDataPath/admin/custom_quests.json.
    /// </summary>
    public sealed class AdminQuestService : MonoBehaviour
    {
        public static AdminQuestService Instance { get; private set; }

        [SerializeField] private List<AdminQuestRecord> quests = new List<AdminQuestRecord>();
        [SerializeField] private bool loadOnStart = true;

        public IReadOnlyList<AdminQuestRecord> Quests => quests;

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

        private void Start()
        {
            if (loadOnStart)
            {
                Load();
            }
        }

        public static AdminQuestService Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(AdminQuestService));
            return go.AddComponent<AdminQuestService>();
        }

        public AdminQuestRecord CreateOrUpdate(AdminQuestRecord quest)
        {
            if (quest == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(quest.QuestId))
            {
                quest.QuestId = "custom_quest_" + quests.Count;
            }

            var existing = quests.Find(q => q.QuestId == quest.QuestId);
            if (existing == null)
            {
                quests.Add(quest);
                existing = quest;
            }
            else
            {
                existing.Title = quest.Title;
                existing.Description = quest.Description;
                existing.Region = quest.Region;
                existing.GiverName = quest.GiverName;
                existing.Repeatable = quest.Repeatable;
                existing.Enabled = quest.Enabled;
                existing.Objectives = quest.Objectives;
                existing.Rewards = quest.Rewards;
            }

            Save();
            PlayerJournalTracker.AddOrUpdateEntry(
                "admin_quest_" + PlayerJournalTracker.Safe(existing.QuestId),
                JournalEntryType.Quest,
                existing.Title,
                existing.Description,
                existing.Region,
                existing.QuestId);

            NotificationFeed.Post($"Admin quest saved: {existing.Title}", NotificationType.Quest);
            return existing;
        }

        public bool Delete(string questId)
        {
            int removed = quests.RemoveAll(q => q.QuestId == questId);
            if (removed > 0)
            {
                Save();
                NotificationFeed.Post($"Admin quest deleted: {questId}", NotificationType.Quest);
                return true;
            }

            return false;
        }

        public AdminQuestRecord Find(string questId)
        {
            return quests.Find(q => q.QuestId == questId);
        }

        public bool ActivateQuest(string questId)
        {
            var quest = Find(questId);
            if (quest == null || !quest.Enabled)
            {
                NotificationFeed.Post($"Quest not found or disabled: {questId}", NotificationType.Warning);
                return false;
            }

            var first = quest.FirstObjective;
            if (first != null)
            {
                QuestObjectiveTracker.Ensure().SetObjective(
                    quest.QuestId,
                    first.ObjectiveId,
                    first.Title,
                    string.IsNullOrWhiteSpace(first.Description) ? quest.Description : first.Description,
                    first.WorldPosition,
                    first.HasWorldPosition);

                if (first.HasWorldPosition)
                {
                    PlayerMapMarkerTracker.ReportCurrentObjective(
                        first.WorldPosition,
                        first.Title,
                        quest.QuestId,
                        first.Description);
                }
            }

            PlayerJournalTracker.AddOrUpdateEntry(
                "active_quest_" + PlayerJournalTracker.Safe(quest.QuestId),
                JournalEntryType.Quest,
                quest.Title,
                quest.Description,
                quest.Region,
                quest.QuestId);

            NotificationFeed.Post($"Quest started: {quest.Title}", NotificationType.Quest);
            return true;
        }

        public bool CompleteQuest(string questId)
        {
            var quest = Find(questId);
            if (quest == null)
            {
                return false;
            }

            if (quest.Objectives != null)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective != null)
                    {
                        QuestObjectiveTracker.Ensure().CompleteObjective(quest.QuestId, objective.ObjectiveId);
                    }
                }
            }

            GrantRewards(quest);

            PlayerJournalTracker.Complete("active_quest_" + PlayerJournalTracker.Safe(quest.QuestId));
            NotificationFeed.Post($"Quest complete: {quest.Title}", NotificationType.Quest);
            return true;
        }

        public void GrantRewards(AdminQuestRecord quest)
        {
            if (quest == null || quest.Rewards == null)
            {
                return;
            }

            foreach (var reward in quest.Rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(reward.ItemId) && reward.Quantity > 0)
                {
                    PlayerInventoryTracker.AddItemId(reward.ItemId, reward.Quantity);
                }

                if (reward.Currency > 0)
                {
                    PlayerInventoryTracker.AddCurrency(reward.Currency);
                }

                if (reward.ReputationFaction != ElementbornFactionId.Unknown && reward.ReputationAmount != 0)
                {
                    FactionReputationTracker.AddReputation(
                        reward.ReputationFaction,
                        reward.ReputationAmount,
                        "Quest reward: " + quest.Title);
                }
            }
        }

        public void Save()
        {
            var file = new AdminQuestDatabaseFile();
            foreach (var quest in quests)
            {
                file.Quests.Add(quest);
            }

            File.WriteAllText(GetPath(), JsonUtility.ToJson(file, prettyPrint: true));
        }

        public void Load()
        {
            string path = GetPath();
            if (!File.Exists(path))
            {
                return;
            }

            var file = JsonUtility.FromJson<AdminQuestDatabaseFile>(File.ReadAllText(path));
            if (file == null)
            {
                return;
            }

            quests.Clear();
            foreach (var quest in file.Quests)
            {
                quests.Add(quest);
            }
        }

        public static string GetPath()
        {
            string dir = Path.Combine(Application.persistentDataPath, "admin");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "custom_quests.json");
        }
    }
}
