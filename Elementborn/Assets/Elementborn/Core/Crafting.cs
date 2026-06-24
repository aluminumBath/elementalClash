using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>An item + quantity used in (or produced by) a recipe.</summary>
    public readonly struct CraftIngredient
    {
        public readonly string ItemId;
        public readonly int Count;
        public CraftIngredient(string itemId, int count) { ItemId = itemId; Count = count < 1 ? 1 : count; }
    }

    /// <summary>A recipe: consume the inputs, produce <see cref="OutputCount"/> of <see cref="OutputItemId"/>.</summary>
    public sealed class Recipe
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<CraftIngredient> Inputs { get; }
        public string OutputItemId { get; }
        public int OutputCount { get; }

        public Recipe(string id, string name, string outputItemId, int outputCount, params CraftIngredient[] inputs)
        {
            Id = id; Name = name; OutputItemId = outputItemId;
            OutputCount = outputCount < 1 ? 1 : outputCount;
            Inputs = inputs ?? new CraftIngredient[0];
        }
    }

    public static class RecipeBook
    {
        public static IReadOnlyList<Recipe> All { get; } = new List<Recipe>
        {
            new Recipe("tough_leather", "Tough Leather", "tough_leather", 1,
                new CraftIngredient("hide", 3)),
            new Recipe("healing_tonic", "Healing Tonic", "healing_tonic", 1,
                new CraftIngredient("river_pearl", 2), new CraftIngredient("sunflower_seeds", 1)),
            new Recipe("stamina_draught", "Stamina Draught", "stamina_draught", 1,
                new CraftIngredient("ember_shard", 2), new CraftIngredient("sunflower_seeds", 1)),
            new Recipe("elixir_of_vigor", "Elixir of Vigor", "elixir_of_vigor", 1,
                new CraftIngredient("healing_tonic", 1), new CraftIngredient("stamina_draught", 1),
                new CraftIngredient("deep_jelly", 1)),
            new Recipe("elemental_charm", "Elemental Charm", "elemental_charm", 1,
                new CraftIngredient("ember_shard", 1), new CraftIngredient("river_pearl", 1),
                new CraftIngredient("ore_marrow_bone", 1), new CraftIngredient("iridescent_beetle", 1)),
            new Recipe("old_relic", "Reforged Relic", "old_relic", 1,
                new CraftIngredient("compost_truffle", 2), new CraftIngredient("iridescent_beetle", 2),
                new CraftIngredient("river_pearl", 1)),
        };

        public static Recipe Get(string id)
        {
            foreach (var r in All) if (r.Id == id) return r;
            return null;
        }
    }

    /// <summary>Pure recipe checks against a snapshot of held counts. The runtime layer feeds the inventory and
    /// performs the consume/grant; this decides craftability and what's short. Unit-tested.</summary>
    public static class Crafting
    {
        public static bool CanCraft(Recipe recipe, IReadOnlyDictionary<string, int> have)
        {
            if (recipe == null || have == null) return false;
            foreach (var ing in recipe.Inputs)
            {
                have.TryGetValue(ing.ItemId, out int n);
                if (n < ing.Count) return false;
            }
            return true;
        }

        /// <summary>What's still needed to craft (item id + shortfall). Empty when the recipe is craftable.</summary>
        public static IReadOnlyList<CraftIngredient> Missing(Recipe recipe, IReadOnlyDictionary<string, int> have)
        {
            var missing = new List<CraftIngredient>();
            if (recipe == null) return missing;
            foreach (var ing in recipe.Inputs)
            {
                int n = 0;
                have?.TryGetValue(ing.ItemId, out n);
                if (n < ing.Count) missing.Add(new CraftIngredient(ing.ItemId, ing.Count - n));
            }
            return missing;
        }
    }
}
