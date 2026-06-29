using UnityEngine;

namespace Elementborn.Game
{
    public sealed class TamedCreatureOrphanageReturnHook : MonoBehaviour
    {
        [SerializeField] private string creatureId = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private bool disableCreatureAfterTransfer = true;

        private string CreatureId => string.IsNullOrWhiteSpace(creatureId) ? gameObject.name.Replace(" ", "_").ToLowerInvariant() : creatureId;
        private string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;

        public void NotifyCreatureDied()
        {
            TransferToOrphanage(CreatureOrphanageDepartureReason.Died, "The creature died and was recovered by Ella and Eloc for care.");
        }

        public void NotifyCreatureReleased()
        {
            TransferToOrphanage(CreatureOrphanageDepartureReason.Released, "The creature was let go and later found its way to the orphanage.");
        }

        public void NotifyCreatureRanAway()
        {
            TransferToOrphanage(CreatureOrphanageDepartureReason.RanAway, "The creature ran away and was recovered by the orphanage.");
        }

        public void NotifyCreatureRanAwayDueToTreatment()
        {
            TransferToOrphanage(CreatureOrphanageDepartureReason.Mistreatment, "The creature ran away due to poor treatment and will require trust repair before returning.");
        }

        public void TransferToOrphanage(CreatureOrphanageDepartureReason reason, string notes)
        {
            CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(CreatureId, DisplayName, reason, notes);
            if (disableCreatureAfterTransfer)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetCreatureIdentity(string id, string label)
        {
            creatureId = id;
            displayName = label;
        }
    }
}
