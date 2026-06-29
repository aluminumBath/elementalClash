using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class RecipeBookTracker : MonoBehaviour
    {
        public static RecipeBookTracker Instance { get; private set; }

        [SerializeField] private List<string> knownRecipeIds = new List<string>();

        public IReadOnlyList<string> KnownRecipeIds => knownRecipeIds;

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

        public static RecipeBookTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(RecipeBookTracker));
            return go.AddComponent<RecipeBookTracker>();
        }

        public static bool Knows(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return false;
            }

            return Ensure().knownRecipeIds.Contains(recipeId);
        }

        public static void Learn(CraftingRecipeDefinition recipe)
        {
            if (recipe == null)
            {
                return;
            }

            Learn(recipe.RecipeId, recipe.DisplayName, recipe.Description);
        }

        public static void Learn(string recipeId, string displayName = "", string description = "")
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return;
            }

            var tracker = Ensure();
            if (!tracker.knownRecipeIds.Contains(recipeId))
            {
                tracker.knownRecipeIds.Add(recipeId);
                NotificationFeed.Post($"Recipe learned: {(string.IsNullOrWhiteSpace(displayName) ? recipeId : displayName)}", NotificationType.Journal);

                PlayerJournalTracker.AddOrUpdateEntry(
                    "recipe_" + PlayerJournalTracker.Safe(recipeId),
                    JournalEntryType.Item,
                    string.IsNullOrWhiteSpace(displayName) ? recipeId : displayName,
                    string.IsNullOrWhiteSpace(description) ? "A crafting recipe." : description,
                    relatedId: recipeId);
            }
        }

        public static void Forget(string recipeId)
        {
            Ensure().knownRecipeIds.Remove(recipeId);
        }

        public static void Clear()
        {
            Ensure().knownRecipeIds.Clear();
        }

        public void ImportKnownRecipe(string recipeId)
        {
            if (!knownRecipeIds.Contains(recipeId))
            {
                knownRecipeIds.Add(recipeId);
            }
        }
    }
}
