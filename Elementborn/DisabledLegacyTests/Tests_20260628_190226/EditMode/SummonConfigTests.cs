using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SummonConfigTests
    {
        [Test]
        public void DefaultRatesLeaveRoomForRare()
        {
            var cfg = SummonConfig.Default;
            Assert.Greater(cfg.LegendaryChance, 0.0);
            Assert.Greater(cfg.EpicChance, 0.0);
            // Legendary + Epic must stay under 1 so Rare keeps the remaining probability mass.
            Assert.Less(cfg.LegendaryChance + cfg.EpicChance, 1.0);
            Assert.Greater(1.0 - (cfg.LegendaryChance + cfg.EpicChance), 0.0, "Rare band must be non-empty");
        }

        [Test]
        public void PityAndCostsArePositiveAndConsistent()
        {
            var cfg = SummonConfig.Default;
            Assert.Greater(cfg.HardPity, 0);
            Assert.Greater(cfg.SingleCost, 0);
            Assert.AreEqual(cfg.SingleCost * 10, cfg.TenCost, "a ten-pull should cost ten singles");
            Assert.AreEqual(1600, cfg.TenCost, "documented ten-pull price");
            Assert.AreEqual(10, cfg.TenPullSize);
        }

        [Test]
        public void FeaturedChanceIsAFiftyFifty()
        {
            var cfg = SummonConfig.Default;
            Assert.AreEqual(0.5, cfg.FeaturedChance, 1e-9);
            Assert.Greater(cfg.FeaturedExchangeCost, 0);
        }

        [Test]
        public void RefundForScalesWithRarity()
        {
            var cfg = SummonConfig.Default;
            Assert.AreEqual(cfg.RefundRare, cfg.RefundFor(SummonRarity.Rare));
            Assert.AreEqual(cfg.RefundEpic, cfg.RefundFor(SummonRarity.Epic));
            Assert.AreEqual(cfg.RefundLegendary, cfg.RefundFor(SummonRarity.Legendary));

            // A rarer duplicate must never refund less than a commoner one.
            Assert.LessOrEqual(cfg.RefundFor(SummonRarity.Rare), cfg.RefundFor(SummonRarity.Epic));
            Assert.LessOrEqual(cfg.RefundFor(SummonRarity.Epic), cfg.RefundFor(SummonRarity.Legendary));
        }

        [Test]
        public void CostForPicksSingleOrTen()
        {
            var cfg = SummonConfig.Default;
            Assert.AreEqual(cfg.SingleCost, cfg.CostFor(false));
            Assert.AreEqual(cfg.TenCost, cfg.CostFor(true));
        }
    }
}
