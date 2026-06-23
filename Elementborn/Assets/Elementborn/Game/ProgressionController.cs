using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The runtime home of leveling. Awards XP when the player defeats a creature or completes a quest (off the
    /// existing <see cref="QuestEvents"/>), applies the level's max-health bonus to the player's
    /// <see cref="Damageable"/>, and persists level/XP through the same save path as quests and items. UI reads
    /// <see cref="Progression"/>. Put one on a bootstrap object (the scene adds it).
    /// </summary>
    public sealed class ProgressionController : MonoBehaviour
    {
        public static ProgressionController Instance { get; private set; }

        public Progression Progression { get; } = new Progression();
        public event System.Action Changed;

        [SerializeField] private int xpPerDefeat = 25;
        [SerializeField] private int xpPerQuest = 75;

        private Damageable _playerBody;
        private float _baseMaxHealth = -1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Progression.Changed += RaiseChanged;
            Progression.LeveledUp += OnLeveledUp;
        }

        private void Start()
        {
            EnsureBody();
            if (_playerBody != null && _playerBody.Health != null && _baseMaxHealth < 0f)
                _baseMaxHealth = _playerBody.Health.Max;
            ApplyBonus(); // reflect any level already loaded from a save
        }

        private void OnEnable()
        {
            QuestEvents.CreatureDefeated += OnDefeated;
            QuestEvents.QuestCompleted += OnQuestCompleted;
        }

        private void OnDisable()
        {
            QuestEvents.CreatureDefeated -= OnDefeated;
            QuestEvents.QuestCompleted -= OnQuestCompleted;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void RaiseChanged() => Changed?.Invoke();

        private void OnDefeated(string kind)
        {
            int xp = xpPerDefeat;
            if (System.Enum.TryParse(kind, out CreatureKind ck))
            {
                var stats = CreatureCombat.For(ck);
                xp = Experience.ForCreature(stats.MaxHealth, stats.Damage);
            }
            Progression.AddXp(xp);
        }

        private void OnQuestCompleted(string questId) => Progression.AddXp(xpPerQuest);

        private void OnLeveledUp(int levels)
        {
            int reward = 20 * Progression.Level;
            PlayerInventory.Instance?.AddCurrency(Currency.Silver, reward);
            GameHud.Instance?.Toast("Level up! Level " + Progression.Level + "  (+" + reward + " Silver)");
            ApplyBonus();
        }

        private void EnsureBody()
        {
            if (_playerBody != null) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _playerBody = p.GetComponentInParent<Damageable>() ?? p.GetComponentInChildren<Damageable>();
        }

        private void ApplyBonus()
        {
            EnsureBody();
            if (_playerBody == null || _playerBody.Health == null) return;
            if (_baseMaxHealth < 0f) _baseMaxHealth = _playerBody.Health.Max; // first-seen base
            _playerBody.SetMaxHealth(_baseMaxHealth + Progression.BonusMaxHealth);
        }

        public void CaptureInto(SaveData data)
        {
            if (data == null) return;
            data.level = Progression.Level;
            data.xp = Progression.Xp;
        }

        public void RestoreFrom(SaveData data)
        {
            if (data == null) return;
            Progression.Restore(data.level, data.xp);
            ApplyBonus();
        }
    }
}
