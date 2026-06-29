using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EquipmentStationInteractable : BaseInteractable
    {
        [SerializeField] private string stationName = "Equipment";
        [SerializeField] private EquipmentItemDefinition[] quickEquipOptions;
        [SerializeField] private bool equipFirstAvailableOnInteract = false;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(stationName, "Manage Gear");
        }

        public override void Interact(GameObject interactor)
        {
            if (equipFirstAvailableOnInteract)
            {
                EquipFirstAvailable();
            }
            else
            {
                Debug.Log($"Open equipment UI for {stationName}.");
            }

            base.Interact(interactor);
        }

        public bool EquipFirstAvailable()
        {
            if (quickEquipOptions == null)
            {
                return false;
            }

            foreach (var option in quickEquipOptions)
            {
                if (option != null && PlayerEquipmentTracker.Equip(option))
                {
                    return true;
                }
            }

            NotificationFeed.Post("No available gear can be equipped.", NotificationType.Warning);
            return false;
        }
    }
}
