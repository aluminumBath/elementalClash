using UnityEngine;

namespace Elementborn.Game
{
    public sealed class PoliticalWorldEventTrigger : BaseInteractable
    {
        [SerializeField] private PoliticalWorldEventDefinition eventDefinition;
        [SerializeField] private string prompt = "Respond to world event";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = eventDefinition != null ? $"{prompt}: {eventDefinition.DisplayName}" : prompt;
            return InteractionPromptData.Simple(title, "Respond");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return eventDefinition != null;
        }

        public override void Interact(GameObject interactor)
        {
            if (eventDefinition != null)
            {
                PoliticalWorldEventDirector.Ensure().Activate(eventDefinition.EventId, "player interaction");
            }
        }

        public void SetEvent(PoliticalWorldEventDefinition value)
        {
            eventDefinition = value;
        }
    }
}
