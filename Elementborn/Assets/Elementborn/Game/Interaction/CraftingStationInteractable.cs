using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CraftingStationInteractable : BaseInteractable
    {
        [SerializeField] private string stationName = "Crafting Station";
        [SerializeField] private CraftingStationType stationType = CraftingStationType.Any;
        [SerializeField] private List<CraftingRecipeDefinition> availableRecipes = new List<CraftingRecipeDefinition>();
        [SerializeField] private bool markOnMap;
        [SerializeField] private bool craftFirstAvailableOnInteract = false;

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportCraftingStation(transform.position, stationName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(stationName, "Craft");
        }

        public override void Interact(GameObject interactor)
        {
            if (craftFirstAvailableOnInteract)
            {
                CraftFirstAvailable();
            }
            else
            {
                Debug.Log($"Open crafting UI for {stationName}. Available recipes: {availableRecipes.Count}");
            }

            base.Interact(interactor);
        }

        public bool CraftRecipe(CraftingRecipeDefinition recipe)
        {
            return CraftingSystem.Craft(recipe, stationType).Success;
        }

        public bool CraftFirstAvailable()
        {
            foreach (var recipe in availableRecipes)
            {
                if (CraftingSystem.CanCraft(recipe, stationType, out _))
                {
                    return CraftRecipe(recipe);
                }
            }

            NotificationFeed.Post("No available recipe can be crafted.", NotificationType.Warning);
            return false;
        }

        public IReadOnlyList<CraftingRecipeDefinition> GetAvailableRecipes()
        {
            return availableRecipes;
        }
    }
}
