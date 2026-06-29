using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureFeedInteractable : BaseInteractable
    {
        [SerializeField] private string creatureRecordId = "";
        [SerializeField] private string treatItemId = "CreatureTreat";
        [SerializeField] private int bondGain = 10;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple("Creature", $"Feed {treatItemId}");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && PlayerInventoryTracker.HasItemId(treatItemId, 1);
        }

        public override void Interact(GameObject interactor)
        {
            string recordId = creatureRecordId;
            if (string.IsNullOrWhiteSpace(recordId))
            {
                var first = CreatureBondingTracker.GetFirstAvailable();
                recordId = first != null ? first.RecordId : "";
            }

            if (string.IsNullOrWhiteSpace(recordId))
            {
                NotificationFeed.Post("No owned creature to feed.", NotificationType.Warning);
                return;
            }

            CreatureBondingTracker.FeedCreature(recordId, treatItemId, bondGain);
            base.Interact(interactor);
        }
    }
}
