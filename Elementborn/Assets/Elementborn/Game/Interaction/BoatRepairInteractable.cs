using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Prototype boat repair interaction. It consumes a repair item and invokes a repair event.
    /// Later, wire this directly into BoatController/boat Damageable health.
    /// </summary>
    public sealed class BoatRepairInteractable : BaseInteractable
    {
        [SerializeField] private string repairItemId = "BoatRepairKit";
        [SerializeField] private int requiredQuantity = 1;
        [SerializeField] private int repairAmount = 25;
        [SerializeField] private GameObjectUnityEvent onRepaired;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple("Boat", $"Repair ({repairItemId} x{requiredQuantity})");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && PlayerInventoryTracker.HasItemId(repairItemId, requiredQuantity);
        }

        public override void Interact(GameObject interactor)
        {
            if (!PlayerInventoryTracker.HasItemId(repairItemId, requiredQuantity))
            {
                NotificationFeed.Post($"Need {repairItemId} x{requiredQuantity}.", NotificationType.Warning);
                return;
            }

            PlayerInventoryTracker.RemoveItemId(repairItemId, requiredQuantity);
            NotificationFeed.Post($"Boat repaired for {repairAmount}.", NotificationType.Inventory);
            onRepaired?.Invoke(interactor);
            base.Interact(interactor);
        }
    }
}
