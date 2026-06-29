using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Simple in-world board that can start the first enabled admin quest.
    /// Later, wire this to a full quest selection UI.
    /// </summary>
    public sealed class RuntimeQuestBoardInteractable : BaseInteractable
    {
        [SerializeField] private string boardName = "Quest Board";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(boardName, "View Quests");
        }

        public override void Interact(GameObject interactor)
        {
            var service = AdminQuestService.Ensure();
            foreach (var quest in service.Quests)
            {
                if (quest != null && quest.Enabled)
                {
                    service.ActivateQuest(quest.QuestId);
                    base.Interact(interactor);
                    return;
                }
            }

            NotificationFeed.Post("No admin quests available.", NotificationType.Warning);
            base.Interact(interactor);
        }
    }
}
