using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class ItemPickupInteractable : BaseInteractable
    {
        [SerializeField] private InventoryItemDefinition itemDefinition;
        [SerializeField] private string fallbackItemId = "";
        [SerializeField] private int quantity = 1;
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private bool markImportantItemsOnMap = true;

        private string markerId;

        private void Start()
        {
            if (markImportantItemsOnMap && IsImportant())
            {
                MapMarkerType markerType = itemDefinition != null && itemDefinition.QuestItem
                    ? MapMarkerType.QuestItem
                    : itemDefinition != null && itemDefinition.Category == InventoryItemCategory.Weapon
                        ? MapMarkerType.Weapon
                        : MapMarkerType.RareItem;

                string label = itemDefinition != null ? itemDefinition.DisplayName : fallbackItemId;
                markerId = PlayerMapMarkerTracker.PositionMarkerId(markerType.ToString(), transform.position);
                PlayerMapMarkerTracker.ReportOrUpdateMarker(markerId, markerType, transform.position, label, isPersistent: true);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = itemDefinition != null ? itemDefinition.DisplayName : fallbackItemId;
            return InteractionPromptData.Simple(label, $"Pick Up x{Mathf.Max(1, quantity)}");
        }

        public override void Interact(GameObject interactor)
        {
            int amount = Mathf.Max(1, quantity);
            InventoryTransactionResult result = itemDefinition != null
                ? PlayerInventoryTracker.AddItem(itemDefinition, amount)
                : PlayerInventoryTracker.AddItemId(fallbackItemId, amount);

            if (!result.Success && result.Moved <= 0)
            {
                NotificationFeed.Post(result.Message, NotificationType.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(markerId))
            {
                PlayerMapMarkerTracker.RemoveMarker(markerId);
            }

            base.Interact(interactor);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }

        private bool IsImportant()
        {
            if (itemDefinition != null)
            {
                return itemDefinition.Important;
            }

            return !string.IsNullOrWhiteSpace(fallbackItemId);
        }
    }
}
