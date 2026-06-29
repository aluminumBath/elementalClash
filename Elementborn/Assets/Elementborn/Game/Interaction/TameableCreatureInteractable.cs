using UnityEngine;

namespace Elementborn.Game
{
    public sealed class TameableCreatureInteractable : BaseInteractable
    {
        [SerializeField] private CreatureDefinition creatureDefinition;
        [SerializeField] private string requiredTreatItemId = "CreatureTreat";
        [SerializeField] private int requiredTreatQuantity = 1;
        [SerializeField] private int tamingBonus = 15;
        [SerializeField] private bool consumeTreatOnAttempt = true;
        [SerializeField] private bool destroyOnTame = false;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string name = creatureDefinition != null ? creatureDefinition.DisplayName : "Creature";
            return InteractionPromptData.Simple(name, $"Tame ({requiredTreatItemId} x{requiredTreatQuantity})");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor)
                && creatureDefinition != null
                && PlayerInventoryTracker.HasItemId(requiredTreatItemId, requiredTreatQuantity);
        }

        public override void Interact(GameObject interactor)
        {
            if (creatureDefinition == null)
            {
                NotificationFeed.Post("No creature definition assigned.", NotificationType.Warning);
                return;
            }

            if (!PlayerInventoryTracker.HasItemId(requiredTreatItemId, requiredTreatQuantity))
            {
                NotificationFeed.Post($"Need {requiredTreatItemId} x{requiredTreatQuantity}.", NotificationType.Warning);
                return;
            }

            if (consumeTreatOnAttempt)
            {
                PlayerInventoryTracker.RemoveItemId(requiredTreatItemId, requiredTreatQuantity);
            }

            bool success = CreatureBondingTracker.TryTame(
                creatureDefinition,
                creatureDefinition.DisplayName,
                transform.position,
                tamingBonus);

            base.Interact(interactor);

            if (success && destroyOnTame)
            {
                Destroy(gameObject);
            }
        }
    }
}
