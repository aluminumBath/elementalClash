using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class EconomyTests
    {
        [Test]
        public void ValueLadderIsTimesFive()
        {
            Assert.AreEqual(1, Wallet.ValueOf(Currency.Silver));
            Assert.AreEqual(5, Wallet.ValueOf(Currency.Ruby));
            Assert.AreEqual(25, Wallet.ValueOf(Currency.Emerald));
            Assert.AreEqual(125, Wallet.ValueOf(Currency.Sapphire));
            Assert.AreEqual(625, Wallet.ValueOf(Currency.Diamond));
        }

        [Test]
        public void AddAccumulatesValue()
        {
            var w = new Wallet();
            w.Add(Currency.Diamond, 1);
            w.Add(Currency.Silver, 3);
            Assert.AreEqual(1, w.CountOf(Currency.Diamond));
            Assert.AreEqual(628, w.TotalValue);
        }

        [Test]
        public void SpendChecksAffordabilityAndDeducts()
        {
            var w = new Wallet();
            w.Add(Currency.Diamond, 1); // 625
            w.Add(Currency.Silver, 3);  // +3 -> 628

            Assert.IsFalse(w.Spend(700)); // too poor, unchanged
            Assert.AreEqual(628, w.TotalValue);

            Assert.IsTrue(w.Spend(600));
            Assert.AreEqual(28, w.TotalValue);
        }

        [Test]
        public void SpendMakesChangeFromLargeCoins()
        {
            var w = new Wallet();
            w.Add(Currency.Diamond, 1); // only a diamond
            Assert.IsTrue(w.Spend(5));
            Assert.AreEqual(620, w.TotalValue);
            Assert.AreEqual(0, w.CountOf(Currency.Diamond)); // melted into smaller coins
        }

        [Test]
        public void BreakdownIsGreedyLargestFirst()
        {
            var d = Wallet.Breakdown(631); // 625 + 5 + 1
            Assert.AreEqual(1, d[Currency.Diamond]);
            Assert.AreEqual(1, d[Currency.Ruby]);
            Assert.AreEqual(1, d[Currency.Silver]);
            Assert.IsFalse(d.ContainsKey(Currency.Sapphire));
        }
    }
}
