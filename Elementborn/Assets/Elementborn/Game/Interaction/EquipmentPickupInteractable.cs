using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EquipmentPickupInteractable : BaseInteractable
    {
        [SerializeField] private EquipmentItemDefinition equipment;
        [SerializeField] private bool addInventoryItem = true;
        [SerializeField] private bool equipImmediately = false;
        [SerializeField] private bool destroyOnPickup = true;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = equipment != null ? equipment.DisplayName : "Equipment";
            return InteractionPromptData.Simple(label, equipImmediately ? "Equip" : "Pick Up");
        }

        public override void Interact(GameObject interactor)
        {
            if (equipment == null)
            {
                NotificationFeed.Post("No equipment assigned.", NotificationType.Warning);
                return;
            }

            if (addInventoryItem)
            {
                if (equipment.SourceItem != null)
                {
                    PlayerInventoryTracker.AddItem(equipment.SourceItem, 1);
                }
                else
                {
                    PlayerInventoryTracker.AddItemId(equipment.ItemId, 1);
                }
            }

            PlayerEquipmentTracker.Ensure().RegisterDefinition(equipment);

            if (equipImmediately)
            {
                PlayerEquipmentTracker.Equip(equipment);
            }

            base.Interact(interactor);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
