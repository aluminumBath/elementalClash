using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureStableInteractable : BaseInteractable
    {
        [SerializeField] private CreatureStable stable;
        [SerializeField] private bool releaseFirstStoredOnInteract = false;

        private void Awake()
        {
            if (stable == null)
            {
                stable = GetComponent<CreatureStable>();
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = stable != null ? stable.DisplayName : "Stable";
            return InteractionPromptData.Simple(label, releaseFirstStoredOnInteract ? "Release Creature" : "Store Creature");
        }

        public override void Interact(GameObject interactor)
        {
            if (stable == null)
            {
                return;
            }

            if (releaseFirstStoredOnInteract)
            {
                stable.ReleaseFirstStored();
            }
            else
            {
                stable.StoreFirstAvailable();
            }

            base.Interact(interactor);
        }
    }
}
