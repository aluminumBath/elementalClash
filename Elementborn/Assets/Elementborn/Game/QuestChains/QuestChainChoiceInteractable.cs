using UnityEngine;

namespace Elementborn.Game
{
    public sealed class QuestChainChoiceInteractable : BaseInteractable
    {
        [SerializeField] private string chainId = "";
        [SerializeField] private string stageId = "";
        [SerializeField] private string choiceId = "";
        [SerializeField] private string prompt = "Choose";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = $"{prompt}: {choiceId}";
            return InteractionPromptData.Simple(title, "Choose");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrWhiteSpace(chainId) &&
                   !string.IsNullOrWhiteSpace(stageId) &&
                   !string.IsNullOrWhiteSpace(choiceId);
        }

        public override void Interact(GameObject interactor)
        {
            QuestChainDirector.Ensure().ApplyChoice(chainId, stageId, choiceId);
        }

        public void Configure(string chain, string stage, string choice)
        {
            chainId = chain;
            stageId = stage;
            choiceId = choice;
        }
    }
}
