using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Simple pickup for tool item ids such as Pickaxe, Sickle, FishingRod, Net, etc.
    /// </summary>
    public sealed class HarvestToolInteractable : BaseInteractable
    {
        [SerializeField] private string toolItemId = "Pickaxe";
        [SerializeField] private bool destroyOnPickup = true;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(toolItemId, "Pick Up Tool");
        }

        public override void Interact(GameObject interactor)
        {
            PlayerInventoryTracker.AddItemId(toolItemId, 1);
            base.Interact(interactor);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
