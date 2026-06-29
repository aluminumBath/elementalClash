using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Crafting/Recipe", fileName = "CraftingRecipe")]
    public sealed class CraftingRecipeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string recipeId = "";
        [SerializeField] private string displayName = "Recipe";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Classification")]
        [SerializeField] private CraftingRecipeCategory category = CraftingRecipeCategory.Unknown;
        [SerializeField] private CraftingStationType requiredStation = CraftingStationType.Any;
        [SerializeField] private bool knownByDefault = true;
        [SerializeField] private bool importantRecipe;

        [Header("Cost")]
        [SerializeField] private int currencyCost;
        [SerializeField] private List<CraftingIngredientRequirement> ingredients = new List<CraftingIngredientRequirement>();

        [Header("Output")]
        [SerializeField] private InventoryItemDefinition outputItem;
        [SerializeField] private string fallbackOutputItemId = "";
        [SerializeField] private int outputQuantity = 1;

        [Header("Journal")]
        [SerializeField] private bool addJournalEntryOnCraft = true;

        public string RecipeId => string.IsNullOrWhiteSpace(recipeId) ? name : recipeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? RecipeId : displayName;
        public string Description => description;
        public CraftingRecipeCategory Category => category;
        public CraftingStationType RequiredStation => requiredStation;
        public bool KnownByDefault => knownByDefault;
        public bool ImportantRecipe => importantRecipe;
        public int CurrencyCost => Mathf.Max(0, currencyCost);
        public IReadOnlyList<CraftingIngredientRequirement> Ingredients => ingredients;
        public InventoryItemDefinition OutputItem => outputItem;
        public string OutputItemId => outputItem != null ? outputItem.ItemId : fallbackOutputItemId;
        public string OutputDisplayName => outputItem != null ? outputItem.DisplayName : OutputItemId;
        public int OutputQuantity => Mathf.Max(1, outputQuantity);
        public bool AddJournalEntryOnCraft => addJournalEntryOnCraft;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                recipeId = name;
            }

            currencyCost = Mathf.Max(0, currencyCost);
            outputQuantity = Mathf.Max(1, outputQuantity);
        }
    }
}
