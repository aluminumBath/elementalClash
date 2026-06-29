using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SummonRollerTests
    {
        private static SummonBanner Standard => SummonBannerCatalog.Standard;
        private static SummonBanner Featured => SummonBannerCatalog.Featured;

        [Test]
        public void TierThresholdsRespectChances()
        {
            var cfg = SummonConfig.Default; // Legendary 0.006, Epic 0.051

            // tier draw below LegendaryChance -> Legendary (then a pool-pick draw).
            var leg = SummonRoller.Pull(Standard, new SummonState(), cfg, new ScriptedRandomSource(0.0, 0.001, 0.0));
            Assert.AreEqual(SummonRarity.Legendary, leg.Rarity);

            // tier draw inside the Epic band -> Epic.
            var epic = SummonRoller.Pull(Standard, new SummonState(), cfg, new ScriptedRandomSource(0.0, 0.02, 0.0));
            Assert.AreEqual(SummonRarity.Epic, epic.Rarity);

            // tier draw above both bands -> Rare.
            var rare = SummonRoller.Pull(Standard, new SummonState(), cfg, new ScriptedRandomSource(0.0, 0.5, 0.0));
            Assert.AreEqual(SummonRarity.Rare, rare.Rarity);
        }

        [Test]
        public void HardPityForcesLegendaryAndResets()
        {
            var cfg = new SummonConfig { HardPity = 3 };
            var state = new SummonState();
            var rng = new ScriptedRandomSource(0.9); // every draw is Rare-tier

            var a = SummonRoller.Pull(Standard, state, cfg, rng);
            var b = SummonRoller.Pull(Standard, state, cfg, rng);
            Assert.AreEqual(SummonRarity.Rare, a.Rarity);
            Assert.AreEqual(SummonRarity.Rare, b.Rarity);
            Assert.AreEqual(2, state.PityCounter);

            var c = SummonRoller.Pull(Standard, state, cfg, rng);
            Assert.AreEqual(SummonRarity.Legendary, c.Rarity);
            Assert.IsTrue(c.PityForced);
            Assert.AreEqual(0, state.PityCounter, "pity resets after a Legendary");
        }

        [Test]
        public void LostFiftyFiftyArmsTheNextGuarantee()
        {
            var cfg = SummonConfig.Default;
            var state = new SummonState();

            // Legendary (0.0), lose the 50/50 (0.99 >= 0.5), then pick an off-banner Legendary (0.0).
            var lost = SummonRoller.Pull(Featured, state, cfg, new ScriptedRandomSource(0.0, 0.0, 0.99, 0.0));
            Assert.AreEqual(SummonRarity.Legendary, lost.Rarity);
            Assert.IsFalse(lost.WonFeatured);
            Assert.AreNotEqual(Featured.Featured, lost.Kind);
            Assert.IsTrue(state.GuaranteedFeatured);

            // Next Legendary is the guaranteed featured (no 50/50 draw consumed).
            var won = SummonRoller.Pull(Featured, state, cfg, new ScriptedRandomSource(0.0, 0.0));
            Assert.AreEqual(Featured.Featured, won.Kind);
            Assert.IsTrue(won.WonFeatured);
            Assert.IsFalse(state.GuaranteedFeatured, "the guarantee is consumed");
        }

        [Test]
        public void WonFiftyFiftyGivesFeaturedWithoutArming()
        {
            var cfg = SummonConfig.Default;
            var state = new SummonState();
            // Legendary (0.0), win the 50/50 (0.0 < 0.5) -> featured, no further draws.
            var won = SummonRoller.Pull(Featured, state, cfg, new ScriptedRandomSource(0.0, 0.0, 0.0));
            Assert.AreEqual(Featured.Featured, won.Kind);
            Assert.IsTrue(won.WonFeatured);
            Assert.IsFalse(state.GuaranteedFeatured);
        }

        [Test]
        public void TenPullGuaranteesAtLeastOneEpic()
        {
            var cfg = SummonConfig.Default;
            var state = new SummonState();
            var rng = new ScriptedRandomSource(0.9); // all draws would otherwise be Rare

            var results = SummonRoller.PullMany(Standard, state, cfg, rng, cfg.TenPullSize);
            Assert.AreEqual(10, results.Count);
            Assert.IsTrue(results.Any(r => (int)r.Rarity >= (int)SummonRarity.Epic),
                "the floor must bump the batch to at least one Epic");
            Assert.AreEqual(10, state.TotalPulls);
        }

        [Test]
        public void SinglePullDoesNotApplyTheTenPullFloor()
        {
            var cfg = SummonConfig.Default;
            var state = new SummonState();
            var one = SummonRoller.PullMany(Standard, state, cfg, new ScriptedRandomSource(0.9), 1);
            Assert.AreEqual(1, one.Count);
            Assert.AreEqual(SummonRarity.Rare, one[0].Rarity);
        }
    }
}
