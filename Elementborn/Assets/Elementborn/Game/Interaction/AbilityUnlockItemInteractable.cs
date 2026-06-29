using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AbilityUnlockItemInteractable : BaseInteractable
    {
        [SerializeField] private AbilityDefinition ability;
        [SerializeField] private bool destroyAfterUnlock = true;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = ability != null ? ability.DisplayName : "Ability";
            return InteractionPromptData.Simple(label, "Learn");
        }

        public override void Interact(GameObject interactor)
        {
            if (ability != null)
            {
                PlayerAbilityTracker.Unlock(ability, AbilityUnlockSource.Item, spendSkillPoints: false);
            }

            base.Interact(interactor);

            if (destroyAfterUnlock)
            {
                Destroy(gameObject);
            }
        }
    }
}
