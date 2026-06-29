using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShrineAbilityUnlockInteractable : BaseInteractable
    {
        [SerializeField] private string shrineName = "Shrine";
        [SerializeField] private AbilityDefinition ability;
        [SerializeField] private bool markOnMap = true;

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportShrine(transform.position, shrineName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = ability != null ? ability.DisplayName : "Ability";
            return InteractionPromptData.Simple(shrineName, $"Learn {label}");
        }

        public override void Interact(GameObject interactor)
        {
            if (ability != null)
            {
                PlayerAbilityTracker.Unlock(ability, AbilityUnlockSource.Shrine);
            }

            base.Interact(interactor);
        }
    }
}
