using UnityEngine;

namespace Elementborn.Game
{
    public sealed class WorldEventRumorInteractable : BaseInteractable
    {
        [SerializeField] private string sourceName = "Rumor";
        [SerializeField] private WorldEventDefinition worldEvent;
        [SerializeField] private bool scheduleInsteadOfActivate = true;
        [SerializeField] private bool addRumorOnly = false;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = worldEvent != null ? worldEvent.DisplayName : sourceName;
            return InteractionPromptData.Simple(label, "Hear Rumor");
        }

        public override void Interact(GameObject interactor)
        {
            if (worldEvent == null) return;
            string rumor = string.IsNullOrWhiteSpace(worldEvent.RumorText) ? worldEvent.Description : worldEvent.RumorText;
            if (!string.IsNullOrWhiteSpace(rumor))
            {
                RumorTracker.AddRumor(rumor, RumorType.Unknown, sourceName, worldEvent.Region, worldEvent.Important, true, worldEvent.WorldPosition, worldEvent.HasWorldPosition);
            }
            if (!addRumorOnly)
            {
                if (scheduleInsteadOfActivate) WorldEventTracker.Schedule(worldEvent, "Rumor heard");
                else WorldEventTracker.Activate(worldEvent, "Rumor heard");
            }
            base.Interact(interactor);
        }
    }
}
