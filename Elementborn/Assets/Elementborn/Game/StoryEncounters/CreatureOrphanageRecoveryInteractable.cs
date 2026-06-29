using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureOrphanageRecoveryInteractable : BaseInteractable
    {
        [SerializeField] private string defaultCreatureId = "";
        [SerializeField] private bool buyBackDefaultCreature = false;
        [SerializeField] private string prompt = "Review recovered creatures";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = prompt;
            return InteractionPromptData.Simple(title, "Review");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return true;
        }

        public override void Interact(GameObject interactor)
        {
            var registry = CreatureOrphanageRecoveryRegistry.Ensure();
            if (!string.IsNullOrWhiteSpace(defaultCreatureId))
            {
                bool result = buyBackDefaultCreature ? registry.BuyBack(defaultCreatureId) : registry.LureBack(defaultCreatureId);
                NotificationFeed.Post(result ? "Creature recovery succeeded." : "Creature recovery was not available yet.", NotificationType.Info);
            }
            else
            {
                NotificationFeed.Post(registry.BuildSummary(), NotificationType.Info);
            }
        }
    }
}
