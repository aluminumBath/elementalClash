using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ReligiousFervorTracker : MonoBehaviour
    {
        public static ReligiousFervorTracker Instance { get; private set; }

        [SerializeField] private int fervor = 65;
        [SerializeField] private ReligiousFervorState state = ReligiousFervorState.Chaotic;

        public int Fervor => fervor;
        public ReligiousFervorState State => state;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Recalculate();
        }

        public static ReligiousFervorTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(ReligiousFervorTracker));
            return go.AddComponent<ReligiousFervorTracker>();
        }

        public void AddFervor(int amount, string reason = "")
        {
            fervor = Mathf.Clamp(fervor + amount, 0, 100);
            Recalculate();
            NotificationFeed.Post($"Wind Capital fervor is now {state} ({fervor}). {reason}", NotificationType.Info);
        }

        public void SetFervor(int value)
        {
            fervor = Mathf.Clamp(value, 0, 100);
            Recalculate();
        }

        private void Recalculate()
        {
            if (fervor < 20) state = ReligiousFervorState.Quiet;
            else if (fervor < 45) state = ReligiousFervorState.Tense;
            else if (fervor < 65) state = ReligiousFervorState.Fervent;
            else if (fervor < 85) state = ReligiousFervorState.Chaotic;
            else state = ReligiousFervorState.Riotous;
        }

        public string BuildSummary()
        {
            return $"Wind Capital religious fervor: {state} ({fervor})";
        }
    }
}
