using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Drives <see cref="AchievementProgress"/> from the <see cref="QuestEvents"/> bus (defeats, tames, sightings,
    /// casts, items, currency, quests, NPCs), celebrates each unlock with a toast + the level-up fanfare, and
    /// persists the counts through <see cref="PlayerInventory"/>. The bootstrap scene adds one. The viewer
    /// (<see cref="AchievementsViewer"/>) reads its progress.
    /// </summary>
    public sealed class AchievementController : MonoBehaviour
    {
        public static AchievementController Instance { get; private set; }

        private AchievementProgress _progress = new AchievementProgress();
        public AchievementProgress Progress => _progress;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void OnEnable()
        {
            QuestEvents.CreatureDefeated += OnDefeated;
            QuestEvents.CreatureTamed += OnTamed;
            QuestEvents.CreatureSighted += OnSighted;
            QuestEvents.AbilityCast += OnCast;
            QuestEvents.ItemCollected += OnItem;
            QuestEvents.CurrencyGained += OnCurrency;
            QuestEvents.QuestCompleted += OnQuest;
            QuestEvents.TalkedToNpc += OnNpc;
        }

        private void OnDisable()
        {
            QuestEvents.CreatureDefeated -= OnDefeated;
            QuestEvents.CreatureTamed -= OnTamed;
            QuestEvents.CreatureSighted -= OnSighted;
            QuestEvents.AbilityCast -= OnCast;
            QuestEvents.ItemCollected -= OnItem;
            QuestEvents.CurrencyGained -= OnCurrency;
            QuestEvents.QuestCompleted -= OnQuest;
            QuestEvents.TalkedToNpc -= OnNpc;
        }

        private void OnDefeated(string kind) => Apply(AchievementMetric.CreaturesDefeated, 1, kind);
        private void OnTamed(string kind) => Apply(AchievementMetric.CreaturesTamed, 1, kind);
        private void OnSighted(string kind) => Apply(AchievementMetric.CreaturesSighted, 1, kind);
        private void OnCast(string element, string intent) => Apply(AchievementMetric.AbilitiesCast, 1, element);
        private void OnItem(string itemId, int amount) => Apply(AchievementMetric.ItemsCollected, amount, itemId);
        private void OnCurrency(string currency, int amount) => Apply(AchievementMetric.CurrencyEarned, amount, currency);
        private void OnQuest(string questId) => Apply(AchievementMetric.QuestsCompleted, 1, questId);
        private void OnNpc(string npcId) => Apply(AchievementMetric.NpcsMet, 1, npcId);

        private void Apply(AchievementMetric metric, int amount, string param)
        {
            var unlocked = _progress.Record(metric, amount, param);
            for (int i = 0; i < unlocked.Count; i++)
            {
                GameHud.Instance?.Toast("Achievement unlocked — " + unlocked[i].Name);
                AudioController.Instance?.LevelUp();
            }
        }

        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.achievementKeys.Clear();
            d.achievementCounts.Clear();
            foreach (var kv in _progress.ToSave()) { d.achievementKeys.Add(kv.Key); d.achievementCounts.Add(kv.Value); }
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            var map = new Dictionary<string, int>();
            int n = Mathf.Min(d.achievementKeys.Count, d.achievementCounts.Count);
            for (int i = 0; i < n; i++) map[d.achievementKeys[i]] = d.achievementCounts[i];
            _progress.LoadFrom(map);
        }
    }
}
