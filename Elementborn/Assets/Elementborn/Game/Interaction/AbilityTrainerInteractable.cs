using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AbilityTrainerInteractable : BaseInteractable
    {
        [SerializeField] private string trainerName = "Trainer";
        [SerializeField] private List<AbilityDefinition> teachableAbilities = new List<AbilityDefinition>();
        [SerializeField] private bool teachFirstAvailableOnInteract = true;
        [SerializeField] private bool markOnMap = true;

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportTrainerNpc(transform.position, trainerName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(trainerName, "Train");
        }

        public override void Interact(GameObject interactor)
        {
            if (teachFirstAvailableOnInteract)
            {
                TeachFirstAvailable();
            }
            else
            {
                Debug.Log($"Open trainer UI for {trainerName}. Teachable abilities: {teachableAbilities.Count}");
            }

            base.Interact(interactor);
        }

        public bool TeachFirstAvailable()
        {
            foreach (var ability in teachableAbilities)
            {
                if (ability == null || PlayerAbilityTracker.HasUnlocked(ability.AbilityId))
                {
                    continue;
                }

                var unlocked = PlayerAbilityTracker.Unlock(ability, AbilityUnlockSource.Trainer);
                return unlocked != null;
            }

            NotificationFeed.Post("No available ability to train.", NotificationType.Warning);
            return false;
        }

        public IReadOnlyList<AbilityDefinition> GetTeachableAbilities()
        {
            return teachableAbilities;
        }
    }
}
