using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ConsumableTests
    {
        [Test]
        public void KnownConsumablesAreUsableOthersAreNot()
        {
            Assert.IsTrue(Consumables.IsConsumable("healing_tonic"));
            Assert.IsTrue(Consumables.IsConsumable("stamina_draught"));
            Assert.IsTrue(Consumables.IsConsumable("elixir_of_vigor"));
            Assert.IsFalse(Consumables.IsConsumable("hide"));      // material
            Assert.IsFalse(Consumables.IsConsumable("ore_marrow_bone")); // food
            Assert.IsFalse(Consumables.IsConsumable(null));
        }

        [Test]
        public void EffectsMatchTheirItems()
        {
            Assert.IsTrue(Consumables.TryGet("healing_tonic", out var tonic));
            Assert.AreEqual(40, tonic.Heal);
            Assert.IsFalse(tonic.RefillStamina);

            Assert.IsTrue(Consumables.TryGet("stamina_draught", out var draught));
            Assert.AreEqual(0, draught.Heal);
            Assert.IsTrue(draught.RefillStamina);

            Assert.IsTrue(Consumables.TryGet("elixir_of_vigor", out var elixir));
            Assert.AreEqual(70, elixir.Heal);
            Assert.IsTrue(elixir.RefillStamina);
        }

        [Test]
        public void TryGetIsFalseForNonConsumables()
        {
            Assert.IsFalse(Consumables.TryGet("old_relic", out _));
            Assert.IsFalse(Consumables.TryGet(null, out _));
        }

        [Test]
        public void EveryUsableConsumableIsARealConsumableItem()
        {
            foreach (var id in new[] { "healing_tonic", "stamina_draught", "elixir_of_vigor" })
            {
                Assert.IsTrue(ItemCatalog.Exists(id), id + " must exist in the catalog");
                Assert.AreEqual(ItemCategory.Consumable, ItemCatalog.Get(id).Category, id + " should be a Consumable");
            }
        }
    }
}
