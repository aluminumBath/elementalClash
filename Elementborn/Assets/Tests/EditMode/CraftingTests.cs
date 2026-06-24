using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class CraftingTests
    {
        private static Dictionary<string, int> Have(params (string id, int n)[] items)
        {
            var d = new Dictionary<string, int>();
            foreach (var (id, n) in items) d[id] = n;
            return d;
        }

        [Test]
        public void CanCraftWhenAllInputsPresent()
        {
            var leather = RecipeBook.Get("tough_leather"); // 3 hide
            Assert.IsTrue(Crafting.CanCraft(leather, Have(("hide", 3))));
            Assert.IsTrue(Crafting.CanCraft(leather, Have(("hide", 10)))); // surplus is fine
        }

        [Test]
        public void CannotCraftWhenShortAndMissingListsTheShortfall()
        {
            var leather = RecipeBook.Get("tough_leather"); // 3 hide
            Assert.IsFalse(Crafting.CanCraft(leather, Have(("hide", 2))));
            var missing = Crafting.Missing(leather, Have(("hide", 2)));
            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual("hide", missing[0].ItemId);
            Assert.AreEqual(1, missing[0].Count); // needs 1 more
        }

        [Test]
        public void MissingListsEveryShortfallWhenNothingHeld()
        {
            var charm = RecipeBook.Get("elemental_charm"); // 1 each of four materials
            var missing = Crafting.Missing(charm, Have());
            Assert.AreEqual(charm.Inputs.Count, missing.Count);
            Assert.IsTrue(missing.All(m => m.Count >= 1));
        }

        [Test]
        public void MissingIsEmptyWhenCraftable()
        {
            var tonic = RecipeBook.Get("healing_tonic"); // 2 river_pearl + 1 sunflower_seeds
            Assert.AreEqual(0, Crafting.Missing(tonic, Have(("river_pearl", 2), ("sunflower_seeds", 1))).Count);
        }

        [Test]
        public void NullsAreHandledSafely()
        {
            Assert.IsFalse(Crafting.CanCraft(null, Have(("hide", 9))));
            Assert.IsFalse(Crafting.CanCraft(RecipeBook.Get("tough_leather"), null));
            Assert.AreEqual(0, Crafting.Missing(null, Have()).Count);
        }

        [Test]
        public void RecipeBookHasUniqueIdsAndValidItems()
        {
            var ids = RecipeBook.All.Select(r => r.Id).ToList();
            Assert.AreEqual(ids.Count, ids.Distinct().Count());
            foreach (var r in RecipeBook.All)
            {
                Assert.IsTrue(r.OutputCount >= 1, r.Id + " output count");
                Assert.IsTrue(ItemCatalog.Exists(r.OutputItemId), r.Id + " output '" + r.OutputItemId + "' must exist");
                Assert.IsTrue(r.Inputs.Count >= 1, r.Id + " has no inputs");
                foreach (var ing in r.Inputs)
                    Assert.IsTrue(ItemCatalog.Exists(ing.ItemId), r.Id + " input '" + ing.ItemId + "' must exist");
            }
        }

        [Test]
        public void GetReturnsNullForUnknownRecipe()
        {
            Assert.IsNull(RecipeBook.Get("not_a_recipe"));
            Assert.IsNotNull(RecipeBook.Get("elemental_charm"));
        }
    }
}
