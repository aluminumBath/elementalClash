using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ResourceNodeInteractable : BaseInteractable
    {
        [SerializeField] private string resourceName = "Resource";
        [SerializeField] private int remainingUses = 3;
        [SerializeField] private bool markOnMap = true;

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportResourceNode(transform.position, resourceName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(resourceName, remainingUses > 0 ? "Gather" : "Depleted");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && remainingUses > 0;
        }

        public override void Interact(GameObject interactor)
        {
            if (remainingUses <= 0)
            {
                return;
            }

            remainingUses--;
            Debug.Log($"Gathered {resourceName}. Remaining uses: {remainingUses}");
            base.Interact(interactor);

            if (remainingUses <= 0)
            {
                PlayerMapMarkerTracker.RemoveMarker(PlayerMapMarkerTracker.PositionMarkerId("resource_node", transform.position));
            }
        }
    }
}
