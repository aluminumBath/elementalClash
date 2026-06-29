using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CraftingSystem : MonoBehaviour
    {
        public static CraftingSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static CraftingSystem Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(CraftingSystem));
            return go.AddComponent<CraftingSystem>();
        }

        public static bool CanCraft(CraftingRecipeDefinition recipe, CraftingStationType stationType, out string reason)
        {
            reason = "";

            if (recipe == null)
            {
                reason = "No recipe selected.";
                return false;
            }

            if (!recipe.KnownByDefault && !RecipeBookTracker.Knows(recipe.RecipeId))
            {
                reason = "Recipe not known.";
                return false;
            }

            if (recipe.RequiredStation != CraftingStationType.Any
                && stationType != CraftingStationType.Any
                && recipe.RequiredStation != stationType)
            {
                reason = $"Requires {recipe.RequiredStation}.";
                return false;
            }

            if (PlayerInventoryTracker.Ensure().Currency < recipe.CurrencyCost)
            {
                reason = "Not enough coins.";
                return false;
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.ItemId))
                {
                    continue;
                }

                if (!PlayerInventoryTracker.HasItemId(ingredient.ItemId, ingredient.RequiredQuantity))
                {
                    reason = $"Missing {ingredient.DisplayName} x{ingredient.RequiredQuantity}.";
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(recipe.OutputItemId))
            {
                reason = "Recipe has no output.";
                return false;
            }

            return true;
        }

        public static CraftingResult Craft(CraftingRecipeDefinition recipe, CraftingStationType stationType)
        {
            if (!CanCraft(recipe, stationType, out string reason))
            {
                NotificationFeed.Post(reason, NotificationType.Warning);
                return CraftingResult.Fail(reason);
            }

            if (recipe.CurrencyCost > 0)
            {
                PlayerInventoryTracker.SpendCurrency(recipe.CurrencyCost);
            }

            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient == null || string.IsNullOrWhiteSpace(ingredient.ItemId))
                {
                    continue;
                }

                PlayerInventoryTracker.RemoveItemId(ingredient.ItemId, ingredient.RequiredQuantity);
            }

            InventoryTransactionResult addResult = recipe.OutputItem != null
                ? PlayerInventoryTracker.AddItem(recipe.OutputItem, recipe.OutputQuantity)
                : PlayerInventoryTracker.AddItemId(recipe.OutputItemId, recipe.OutputQuantity);

            if (addResult.Moved <= 0)
            {
                return CraftingResult.Fail("Could not add crafted item to inventory.");
            }

            string message = $"Crafted {recipe.OutputDisplayName} x{addResult.Moved}.";
            NotificationFeed.Post(message, NotificationType.Inventory);

            if (recipe.AddJournalEntryOnCraft || recipe.ImportantRecipe)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "crafted_" + PlayerJournalTracker.Safe(recipe.RecipeId),
                    JournalEntryType.Item,
                    recipe.DisplayName,
                    recipe.Description,
                    relatedId: recipe.RecipeId);
            }

            return CraftingResult.Ok(message);
        }
    }
}
