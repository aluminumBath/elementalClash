using UnityEngine;

namespace Elementborn.Game
{
    public sealed class RecipeUnlockInteractable : BaseInteractable
    {
        [SerializeField] private CraftingRecipeDefinition recipe;
        [SerializeField] private bool destroyAfterLearning = true;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = recipe != null ? recipe.DisplayName : "Recipe";
            return InteractionPromptData.Simple(label, "Learn Recipe");
        }

        public override void Interact(GameObject interactor)
        {
            if (recipe != null)
            {
                RecipeBookTracker.Learn(recipe);
            }

            base.Interact(interactor);

            if (destroyAfterLearning)
            {
                Destroy(gameObject);
            }
        }
    }
}
