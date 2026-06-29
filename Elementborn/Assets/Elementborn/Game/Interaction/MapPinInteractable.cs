using UnityEngine;

namespace Elementborn.Game
{
    public sealed class MapPinInteractable : BaseInteractable
    {
        [SerializeField] private string pinLabel = "Pinned Location";
        [TextArea]
        [SerializeField] private string notes = "";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(pinLabel, "Add Map Pin");
        }

        public override void Interact(GameObject interactor)
        {
            PlayerMapMarkerTracker.ReportCustomPin(
                PlayerMapMarkerTracker.PositionMarkerId("custom_pin", transform.position),
                transform.position,
                pinLabel,
                notes);

            base.Interact(interactor);
        }
    }
}
