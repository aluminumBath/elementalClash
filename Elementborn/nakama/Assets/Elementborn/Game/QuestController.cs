using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The runtime home of the quest loop. Seeds a <see cref="QuestLog"/> from <see cref="QuestCatalog"/>,
    /// forwards <see cref="QuestEvents"/> (defeats, tames, currency, NPC talks) into it, and grants the reward on
    /// turn-in via <see cref="PlayerInventory"/>. UI (the dialogue and quest-log panels) talks to this; put one on
    /// a bootstrap object (the bootstrap scene adds it).
    /// </summary>
    public sealed class QuestController : MonoBehaviour
    {
        public static QuestController Instance { get; private set; }

        public QuestLog Log { get; private set; }
        public event System.Action Changed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Log = new QuestLog(QuestCatalog.All);
            Log.Changed += RaiseChanged;
        }

        private void OnEnable()
        {
            QuestEvents.CreatureDefeated += OnDefeated;
            QuestEvents.CreatureTamed += OnTamed;
            QuestEvents.CurrencyGained += OnCurrency;
            QuestEvents.TalkedToNpc += OnTalked;
        }

        private void OnDisable()
        {
            QuestEvents.CreatureDefeated -= OnDefeated;
            QuestEvents.CreatureTamed -= OnTamed;
            QuestEvents.CurrencyGained -= OnCurrency;
            QuestEvents.TalkedToNpc -= OnTalked;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void RaiseChanged() => Changed?.Invoke();

        private void OnDefeated(string kind) => Log.Record(ObjectiveKind.DefeatCreature, kind);
        private void OnTamed(string kind) => Log.Record(ObjectiveKind.TameCreature, kind);
        private void OnCurrency(string currency, int amount) => Log.Record(ObjectiveKind.CollectCurrency, currency, amount);
        private void OnTalked(string npcId) => Log.Record(ObjectiveKind.TalkToNpc, npcId);

        public bool Start(string questId) => Log != null && Log.Start(questId);

        /// <summary>Turns in a ready quest and grants its reward. Returns false if it wasn't ready.</summary>
        public bool TurnIn(string questId)
        {
            if (Log == null) return false;
            var reward = Log.TurnIn(questId);
            if (reward == null) return false;
            if (reward.Amount > 0) PlayerInventory.Instance?.AddCurrency(reward.Currency, reward.Amount);
            string msg = "Quest complete!";
            if (reward.Amount > 0) msg += "  +" + reward.Amount + " " + reward.Currency;
            if (!string.IsNullOrEmpty(reward.Note)) msg += "  " + reward.Note;
            GameHud.Instance?.Toast(msg);
            return true;
        }
    }
}
