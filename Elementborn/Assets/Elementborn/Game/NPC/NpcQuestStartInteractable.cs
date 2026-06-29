using UnityEngine;

namespace Elementborn.Game
{
    public sealed class NpcQuestStartInteractable : BaseInteractable
    {
        [SerializeField] private NpcWorldEntryDefinition npc;
        [SerializeField] private QuestUiDefinition quest;
        [SerializeField] private bool startOnlyOnce = true;
        [SerializeField] private bool started;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = npc != null ? npc.DisplayName : "Quest Giver";
            return InteractionPromptData.Simple(title, started ? "Quest Started" : "Accept Quest");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor) && quest != null && (!started || !startOnlyOnce);
        }

        public override void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            QuestUiTracker.StartQuest(quest);
            started = true;
            base.Interact(interactor);
        }

        public void Configure(NpcWorldEntryDefinition entry, QuestUiDefinition questDefinition)
        {
            npc = entry;
            quest = questDefinition;
            if (npc != null)
            {
                SetPrompt(npc.DisplayName, "Accept Quest");
            }
        }
    }
}
