using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SocialGroupEventTrigger : BaseInteractable
    {
        [SerializeField] private SocialGroupEventDefinition eventDefinition;
        [SerializeField] private string promptPrefix = "Observe";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = eventDefinition != null ? $"{promptPrefix}: {eventDefinition.DisplayName}" : promptPrefix;
            return InteractionPromptData.Simple(title, "Observe");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return eventDefinition != null;
        }

        public override void Interact(GameObject interactor)
        {
            if (eventDefinition != null)
            {
                SocialGroupRegistry.Ensure().ActivateEvent(eventDefinition.EventId);
            }
        }

        public void SetEvent(SocialGroupEventDefinition value)
        {
            eventDefinition = value;
        }
    }
}
