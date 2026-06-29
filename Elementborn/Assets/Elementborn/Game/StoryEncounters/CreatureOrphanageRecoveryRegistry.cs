using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureOrphanageRecoveryRegistry : MonoBehaviour
    {
        public static CreatureOrphanageRecoveryRegistry Instance { get; private set; }

        [SerializeField] private List<CreatureOrphanageResidentRecord> residents = new List<CreatureOrphanageResidentRecord>();
        [SerializeField] private int baseBuyBackCost = 75;
        [SerializeField] private int mistreatmentTrustPenalty = 35;
        [SerializeField] private int runAwayTrustPenalty = 20;
        [SerializeField] private int deathCareDebt = 25;

        public IReadOnlyList<CreatureOrphanageResidentRecord> Residents => residents;

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

        public static CreatureOrphanageRecoveryRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(CreatureOrphanageRecoveryRegistry));
            return go.AddComponent<CreatureOrphanageRecoveryRegistry>();
        }

        public CreatureOrphanageResidentRecord AdmitCreature(string creatureId, string displayName, CreatureOrphanageDepartureReason reason, string notes = "")
        {
            if (string.IsNullOrWhiteSpace(creatureId))
            {
                creatureId = "creature_" + Mathf.Abs((displayName ?? "creature").GetHashCode());
            }

            CreatureOrphanageResidentRecord record = Find(creatureId);
            if (record == null)
            {
                record = new CreatureOrphanageResidentRecord { CreatureId = creatureId };
                residents.Add(record);
            }

            record.DisplayName = string.IsNullOrWhiteSpace(displayName) ? creatureId : displayName;
            record.DepartureReason = reason;
            record.State = DetermineStartingState(reason);
            record.TimesRecovered++;
            record.Notes = notes;
            record.CareDebt = DetermineCareDebt(reason);
            record.TrustPenalty = DetermineTrustPenalty(reason);
            record.CanBeLuredBack = reason == CreatureOrphanageDepartureReason.Released || reason == CreatureOrphanageDepartureReason.RanAway || reason == CreatureOrphanageDepartureReason.Mistreatment;
            record.CanBeBoughtBack = reason != CreatureOrphanageDepartureReason.Died || record.CareDebt > 0;

            PlayerJournalTracker.AddOrUpdateEntry(
                "orphanage_resident_" + PlayerJournalTracker.Safe(record.CreatureId),
                JournalEntryType.Creature,
                record.DisplayName + " at the Crab-Sign Orphanage",
                BuildResidentDescription(record),
                "Neritha Reefwood",
                record.CreatureId);

            NotificationFeed.Post($"{record.DisplayName} was brought to the Crab-Sign Creature Orphanage.", NotificationType.Info);
            return record;
        }

        public CreatureOrphanageResidentRecord AdmitFromGameObject(GameObject creature, CreatureOrphanageDepartureReason reason, string notes = "")
        {
            if (creature == null)
            {
                return null;
            }

            string id = creature.name.Replace(" ", "_").ToLowerInvariant();
            return AdmitCreature(id, creature.name, reason, notes);
        }

        public bool BuyBack(string creatureId)
        {
            CreatureOrphanageResidentRecord record = Find(creatureId);
            if (record == null || !record.CanBeBoughtBack)
            {
                return false;
            }

            record.State = CreatureOrphanageResidentState.ReturnedToPlayer;
            record.CareDebt = 0;
            NotificationFeed.Post($"{record.DisplayName} has been bought back from the orphanage care ledger.", NotificationType.Quest);
            return true;
        }

        public bool LureBack(string creatureId)
        {
            CreatureOrphanageResidentRecord record = Find(creatureId);
            if (record == null || !record.CanBeLuredBack)
            {
                return false;
            }

            record.State = CreatureOrphanageResidentState.ReturnedToPlayer;
            record.TrustPenalty = Mathf.Max(0, record.TrustPenalty - 10);
            NotificationFeed.Post($"{record.DisplayName} agrees to return after careful luring and better treatment.", NotificationType.Quest);
            return true;
        }

        public bool PermanentlyRehome(string creatureId)
        {
            CreatureOrphanageResidentRecord record = Find(creatureId);
            if (record == null)
            {
                return false;
            }

            record.State = CreatureOrphanageResidentState.PermanentlyRehomed;
            NotificationFeed.Post($"{record.DisplayName} has been permanently rehomed by Ella and Eloc.", NotificationType.Info);
            return true;
        }

        public CreatureOrphanageResidentRecord Find(string creatureId)
        {
            return residents.Find(r => r != null && r.CreatureId == creatureId);
        }

        public void ReplaceResidents(List<CreatureOrphanageResidentRecord> values)
        {
            residents.Clear();
            if (values == null)
            {
                return;
            }

            foreach (CreatureOrphanageResidentRecord record in values)
            {
                if (record != null)
                {
                    residents.Add(record);
                }
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Crab-Sign Creature Orphanage Recovery Registry");
            if (residents.Count == 0)
            {
                sb.AppendLine("- No recovered creatures yet.");
            }

            foreach (CreatureOrphanageResidentRecord record in residents)
            {
                if (record != null)
                {
                    sb.AppendLine($"- {record.DisplayName} [{record.DepartureReason}/{record.State}] debt={record.CareDebt}, trust penalty={record.TrustPenalty}, lure={record.CanBeLuredBack}, buy={record.CanBeBoughtBack}");
                }
            }

            return sb.ToString();
        }

        private CreatureOrphanageResidentState DetermineStartingState(CreatureOrphanageDepartureReason reason)
        {
            switch (reason)
            {
                case CreatureOrphanageDepartureReason.Released:
                case CreatureOrphanageDepartureReason.RanAway:
                case CreatureOrphanageDepartureReason.Mistreatment:
                    return CreatureOrphanageResidentState.AvailableToLureBack;
                case CreatureOrphanageDepartureReason.Died:
                    return CreatureOrphanageResidentState.Recovering;
                default:
                    return CreatureOrphanageResidentState.Recovering;
            }
        }

        private int DetermineCareDebt(CreatureOrphanageDepartureReason reason)
        {
            switch (reason)
            {
                case CreatureOrphanageDepartureReason.Died: return deathCareDebt;
                case CreatureOrphanageDepartureReason.Mistreatment: return baseBuyBackCost + 50;
                case CreatureOrphanageDepartureReason.RanAway: return baseBuyBackCost + 25;
                case CreatureOrphanageDepartureReason.Released: return baseBuyBackCost;
                default: return baseBuyBackCost;
            }
        }

        private int DetermineTrustPenalty(CreatureOrphanageDepartureReason reason)
        {
            switch (reason)
            {
                case CreatureOrphanageDepartureReason.Mistreatment: return mistreatmentTrustPenalty;
                case CreatureOrphanageDepartureReason.RanAway: return runAwayTrustPenalty;
                default: return 0;
            }
        }

        private string BuildResidentDescription(CreatureOrphanageResidentRecord record)
        {
            return $"{record.DisplayName} is at the Crab-Sign Creature Orphanage. Reason: {record.DepartureReason}. State: {record.State}. Care debt: {record.CareDebt}. Trust penalty: {record.TrustPenalty}. {record.Notes}";
        }
    }
}
