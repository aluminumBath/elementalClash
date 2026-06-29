using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class FactionReputationTracker : MonoBehaviour
    {
        public static FactionReputationTracker Instance { get; private set; }

        [SerializeField] private List<FactionReputationRecord> reputations = new List<FactionReputationRecord>();

        public IReadOnlyList<FactionReputationRecord> Reputations => reputations;

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

        public static FactionReputationTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(FactionReputationTracker));
            return go.AddComponent<FactionReputationTracker>();
        }

        public static FactionReputationRecord Get(ElementbornFactionId faction)
        {
            var tracker = Ensure();
            var record = tracker.reputations.Find(r => r.Faction == faction);
            if (record != null)
            {
                return record;
            }

            record = new FactionReputationRecord
            {
                Faction = faction
            };
            tracker.reputations.Add(record);
            return record;
        }

        public static void AddReputation(ElementbornFactionId faction, int amount, string reason = "")
        {
            var record = Get(faction);
            record.Reputation = Mathf.Clamp(record.Reputation + amount, -100, 100);
            record.LastReason = reason ?? "";

            string direction = amount >= 0 ? "increased" : "decreased";
            NotificationFeed.Post($"{faction} reputation {direction}: {record.GetStanding()}", NotificationType.Faction);

            PlayerJournalTracker.AddFaction(
                faction.ToString(),
                $"Current standing: {record.GetStanding()} ({record.Reputation}). Last change: {record.LastReason}");
        }

        public static string GetStanding(ElementbornFactionId faction)
        {
            return Get(faction).GetStanding();
        }

        public static int GetValue(ElementbornFactionId faction)
        {
            return Get(faction).Reputation;
        }

        public static bool IsHostile(ElementbornFactionId faction)
        {
            return GetValue(faction) <= -40;
        }

        public static bool IsTrusted(ElementbornFactionId faction)
        {
            return GetValue(faction) >= 40;
        }

        public static void Clear()
        {
            Ensure().reputations.Clear();
        }
    }
}
