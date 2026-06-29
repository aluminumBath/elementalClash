using UnityEngine;

namespace Elementborn.Game
{
    public sealed class NpcTalkInteractable : BaseInteractable
    {
        [SerializeField] private string npcDisplayName = "NPC";
        [SerializeField] private NpcConversationController conversationController;
        [SerializeField] private bool markNpcOnMap = true;

        private void Start()
        {
            if (conversationController == null)
            {
                conversationController = GetComponent<NpcConversationController>();
            }

            if (markNpcOnMap)
            {
                PlayerMapMarkerTracker.ReportGuideNpc(transform.position, npcDisplayName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(npcDisplayName, "Talk");
        }

        public override void Interact(GameObject interactor)
        {
            if (conversationController != null)
            {
                conversationController.BeginConversation(interactor);
            }

            base.Interact(interactor);
        }
    }
}
