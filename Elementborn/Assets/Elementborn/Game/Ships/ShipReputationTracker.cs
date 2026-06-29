using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShipReputationTracker : MonoBehaviour
    {
        public static ShipReputationTracker Instance { get; private set; }

        [SerializeField] private List<ShipRuntimeRecord> records = new List<ShipRuntimeRecord>();

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

        public static ShipReputationTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(ShipReputationTracker));
            return go.AddComponent<ShipReputationTracker>();
        }

        public ShipRuntimeRecord GetOrCreate(string shipId, ShipReputationTier startingTier = ShipReputationTier.Raucous)
        {
            ShipRuntimeRecord record = records.Find(r => r != null && r.ShipId == shipId);
            if (record != null)
            {
                return record;
            }

            record = new ShipRuntimeRecord
            {
                ShipId = shipId,
                Tier = startingTier,
                KnownToPlayer = true
            };
            records.Add(record);
            return record;
        }

        public void AddReputation(string shipId, int amount)
        {
            var ship = NamedShipRegistry.Ensure().Find(shipId);
            var record = GetOrCreate(shipId, ship != null ? ship.StartingReputation : ShipReputationTier.Raucous);
            record.ReputationPoints += amount;
            record.Tier = CalculateTier(record.ReputationPoints, record.Tier);
            NotificationFeed.Post($"{shipId} reputation changed by {amount}.", NotificationType.Info);
        }

        public void RecordRaid(string shipId, bool victorious)
        {
            var record = GetOrCreate(shipId);
            if (victorious)
            {
                record.RaidsWon++;
                AddReputation(shipId, 10);
            }
            else
            {
                AddReputation(shipId, -5);
            }
        }

        public void RecordCelebration(string shipId)
        {
            var record = GetOrCreate(shipId);
            record.CelebrationsThrown++;
            AddReputation(shipId, 3);
        }

        private ShipReputationTier CalculateTier(int points, ShipReputationTier fallback)
        {
            if (points >= 80) return ShipReputationTier.Legendary;
            if (points >= 40) return ShipReputationTier.Respected;
            if (points >= 0) return ShipReputationTier.Raucous;
            if (points >= -40) return ShipReputationTier.Feared;
            if (points < -40) return ShipReputationTier.Notorious;
            return fallback;
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var record in records)
            {
                if (record != null)
                {
                    sb.AppendLine($"{record.ShipId}: {record.Tier}, reputation {record.ReputationPoints}, raids {record.RaidsWon}, parties {record.CelebrationsThrown}");
                }
            }
            return sb.ToString();
        }
    }
}
