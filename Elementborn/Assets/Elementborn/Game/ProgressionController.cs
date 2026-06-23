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
        public PerkState Perks { get; } = new PerkState();
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
            Perks.Changed += RaiseChanged;
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
            int baseXp = xpPerDefeat;
            if (System.Enum.TryParse(kind, out CreatureKind ck))
            {
                var stats = CreatureCombat.For(ck);
                baseXp = Experience.ForCreature(stats.MaxHealth, stats.Damage);
            }
            Progression.AddXp(Mathf.RoundToInt(baseXp * Perks.XpMultiplier));
        }

        private void OnQuestCompleted(string questId) =>
            Progression.AddXp(Mathf.RoundToInt(xpPerQuest * Perks.XpMultiplier));

        private void OnLeveledUp(int levels)
        {
            Perks.Grant(levels);
            int reward = Mathf.RoundToInt(20 * Progression.Level * Perks.RewardMultiplier);
            PlayerInventory.Instance?.AddCurrency(Currency.Silver, reward);

            string msg = "Level up! Level " + Progression.Level + "  (+" + reward + " Silver, +" + levels + " perk pt)";
            var unlocked = AbilityUnlocks.NewlyUnlocked(Progression.Level - levels, Progression.Level);
            if (unlocked.Count > 0)
                msg += "  Unlocked: " + string.Join(", ", unlocked.ConvertAll(AbilityUnlocks.DisplayName));
            GameHud.Instance?.Toast(msg);

            AudioController.Instance?.LevelUp();
            ApplyBonus();
        }

        /// <summary>Spends a perk point to rank a perk up. Returns false if it can't be ranked.</summary>
        public bool SpendPerk(PerkId id)
        {
            bool ok = Perks.Rank(id);
            if (ok) ApplyBonus();
            return ok;
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
            _playerBody.SetMaxHealth(_baseMaxHealth + Progression.BonusMaxHealth + Perks.BonusMaxHealth);
        }

        public void CaptureInto(SaveData data)
        {
            if (data == null) return;
            data.level = Progression.Level;
            data.xp = Progression.Xp;
            data.perkPoints = Perks.AvailablePoints;
            data.perkIds.Clear(); data.perkRanks.Clear();
            foreach (PerkId id in System.Enum.GetValues(typeof(PerkId)))
            {
                int rank = Perks.RankOf(id);
                if (rank > 0) { data.perkIds.Add(id.ToString()); data.perkRanks.Add(rank); }
            }
        }

        public void RestoreFrom(SaveData data)
        {
            if (data == null) return;
            Progression.Restore(data.level, data.xp);
            var ids = new System.Collections.Generic.List<PerkId>();
            var ranks = new System.Collections.Generic.List<int>();
            int n = System.Math.Min(data.perkIds.Count, data.perkRanks.Count);
            for (int i = 0; i < n; i++)
                if (System.Enum.TryParse(data.perkIds[i], out PerkId pid)) { ids.Add(pid); ranks.Add(data.perkRanks[i]); }
            Perks.Restore(data.perkPoints, ids, ranks);
            ApplyBonus();
        }
    }
}
