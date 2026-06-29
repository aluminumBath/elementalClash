using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class ShopInventoryTests
    {
        [Test]
        public void InventoryAddRemoveCountAndClear()
        {
            var inv = new Inventory();
            inv.Add("hide", 3);
            Assert.AreEqual(3, inv.Count("hide"));
            Assert.IsTrue(inv.Has("hide", 3));
            Assert.IsFalse(inv.Has("hide", 4));
            Assert.AreEqual(3, inv.Total);

            Assert.IsFalse(inv.Remove("hide", 4));   // insufficient — no change
            Assert.AreEqual(3, inv.Count("hide"));
            Assert.IsTrue(inv.Remove("hide", 3));     // exact — clears the entry
            Assert.AreEqual(0, inv.Count("hide"));
            Assert.AreEqual(0, inv.Entries().Count);
            Assert.IsFalse(inv.Remove("nope"));
        }

        [Test]
        public void BuyDeductsValueAndAddsItemWhenAffordable()
        {
            var inv = new Inventory();
            var wallet = new Wallet();
            wallet.Add(Currency.Silver, 100);

            var r = Shop.Buy(inv, wallet, "ember_shard");   // value 18
            Assert.IsTrue(r.Success);
            Assert.AreEqual(1, inv.Count("ember_shard"));
            Assert.AreEqual(82, wallet.TotalValue);
        }

        [Test]
        public void BuyFailsWhenTooPoorAndChangesNothing()
        {
            var inv = new Inventory();
            var wallet = new Wallet();
            wallet.Add(Currency.Silver, 5);

            var r = Shop.Buy(inv, wallet, "old_relic");      // value 40
            Assert.IsFalse(r.Success);
            Assert.AreEqual(0, inv.Count("old_relic"));
            Assert.AreEqual(5, wallet.TotalValue);
        }

        [Test]
        public void SellRemovesItemAndPaysHalfValue()
        {
            var inv = new Inventory();
            var wallet = new Wallet();
            inv.Add("ember_shard", 1);                       // value 18 -> sells for 9

            var r = Shop.Sell(inv, wallet, "ember_shard");
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, inv.Count("ember_shard"));
            Assert.AreEqual(9, wallet.TotalValue);
        }

        [Test]
        public void SellFailsWithoutTheItem()
        {
            var inv = new Inventory();
            var wallet = new Wallet();
            var r = Shop.Sell(inv, wallet, "river_pearl");
            Assert.IsFalse(r.Success);
            Assert.AreEqual(0, wallet.TotalValue);
        }

        [Test]
        public void PricesAndUnknownItemsBehave()
        {
            Assert.AreEqual(18, Shop.BuyPrice("ember_shard"));
            Assert.AreEqual(9, Shop.SellPrice("ember_shard"));
            Assert.AreEqual(0, Shop.BuyPrice("does_not_exist"));
            Assert.IsFalse(Shop.Buy(new Inventory(), new Wallet(), "does_not_exist").Success);
        }

        [Test]
        public void SellPriceIsCategoryAware()
        {
            // Treasure is sell-only fodder -> near full price (0.9).
            Assert.AreEqual(36, Shop.SellPrice("old_relic"));       // 40 * 0.9
            Assert.AreEqual(126, Shop.SellPrice("elemental_charm")); // 140 * 0.9
            // Consumables take a steeper resale haircut (0.4).
            Assert.AreEqual(6, Shop.SellPrice("healing_tonic"));    // 15 * 0.4
            // Crafting material stays at the base rate (0.5).
            Assert.AreEqual(12, Shop.SellPrice("river_pearl"));     // 25 * 0.5 -> 12 (truncated)
        }

        [Test]
        public void NoCategoryAllowsArbitrage()
        {
            foreach (ItemCategory c in System.Enum.GetValues(typeof(ItemCategory)))
                Assert.Less(Shop.SellFractionFor(c), 1.0f, $"{c} would let players buy and resell for profit");
            // Treasure fetches more than a tool of equal value.
            Assert.Greater(Shop.SellFractionFor(ItemCategory.Treasure), Shop.SellFractionFor(ItemCategory.Tool));
        }

        [Test]
        public void EverySidekickFoodIsARealItem()
        {
            foreach (WillowSidekick s in System.Enum.GetValues(typeof(WillowSidekick)))
                Assert.IsTrue(ItemCatalog.Exists(ItemCatalog.FoodFor(s)), "missing food item for " + s);
        }
    }
}
