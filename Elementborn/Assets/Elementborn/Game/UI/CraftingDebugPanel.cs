using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class CraftingDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private CraftingStationInteractable station;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
        }

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (text == null || station == null)
            {
                return;
            }

            IReadOnlyList<CraftingRecipeDefinition> recipes = station.GetAvailableRecipes();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Crafting");

            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    continue;
                }

                bool can = CraftingSystem.CanCraft(recipe, CraftingStationType.Any, out string reason);
                sb.AppendLine($"- {recipe.DisplayName}: {(can ? "Ready" : reason)}");
            }

            text.text = sb.ToString();
        }
    }
}
