using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ThievesGuildReputationTracker : MonoBehaviour
    {
        public static ThievesGuildReputationTracker Instance { get; private set; }

        [SerializeField] private int reputation;
        [SerializeField] private ThievesGuildStanding standing = ThievesGuildStanding.Unknown;

        public int Reputation => reputation;
        public ThievesGuildStanding Standing => standing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            RecalculateStanding();
        }

        public static ThievesGuildReputationTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(ThievesGuildReputationTracker));
            return go.AddComponent<ThievesGuildReputationTracker>();
        }

        public void AddReputation(int amount, string reason = "")
        {
            reputation += amount;
            RecalculateStanding();

            string sign = amount >= 0 ? "+" : "";
            NotificationFeed.Post($"Thieves Guild reputation {sign}{amount}: {Standing}. {reason}", NotificationType.Info);
        }

        public void SetReputation(int value)
        {
            reputation = value;
            RecalculateStanding();
        }

        private void RecalculateStanding()
        {
            if (reputation <= -50) standing = ThievesGuildStanding.Marked;
            else if (reputation < 0) standing = ThievesGuildStanding.Untrusted;
            else if (reputation < 25) standing = ThievesGuildStanding.Unknown;
            else if (reputation < 60) standing = ThievesGuildStanding.Useful;
            else if (reputation < 100) standing = ThievesGuildStanding.Trusted;
            else standing = ThievesGuildStanding.InnerCircle;
        }

        public string BuildSummary()
        {
            return $"Thieves Guild: {Standing} ({reputation})";
        }
    }
}
