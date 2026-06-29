using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CapitalPressureEventTrigger : BaseInteractable
    {
        [SerializeField] private CapitalPressureEventDefinition eventDefinition;
        [SerializeField] private string prompt = "Investigate political pressure";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = eventDefinition != null ? $"{prompt}: {eventDefinition.DisplayName}" : prompt;
            return InteractionPromptData.Simple(title, "Investigate");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return eventDefinition != null;
        }

        public override void Interact(GameObject interactor)
        {
            CapitalWorldStateTracker.Ensure().ApplyEvent(eventDefinition);
            ElementbornAudioService.PlayAt(ElementbornSoundEventId.UiConfirm, transform.position);
        }

        public void SetEvent(CapitalPressureEventDefinition value)
        {
            eventDefinition = value;
        }
    }
}
