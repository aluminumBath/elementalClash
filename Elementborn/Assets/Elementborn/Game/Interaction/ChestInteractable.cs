using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class ChestInteractable : BaseInteractable
    {
        [SerializeField] private string chestLabel = "Treasure Chest";
        [SerializeField] private bool opened;
        [SerializeField] private bool markOnMap = true;
        [SerializeField] private Transform markerLocation;

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportTreasure(markerLocation != null ? markerLocation.position : transform.position, chestLabel);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(chestLabel, opened ? "Already Opened" : "Open");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && !opened;
        }

        public override void Interact(GameObject interactor)
        {
            if (opened)
            {
                return;
            }

            opened = true;
            PlayerMapMarkerTracker.RemoveMarker(PlayerMapMarkerTracker.PositionMarkerId("treasure", markerLocation != null ? markerLocation.position : transform.position));
            base.Interact(interactor);
        }
    }
}
