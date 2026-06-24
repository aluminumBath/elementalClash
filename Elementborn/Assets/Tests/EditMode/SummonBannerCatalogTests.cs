using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SummonBannerCatalogTests
    {
        private static IEnumerable<SummonBanner> Banners => SummonBannerCatalog.All;

        [Test]
        public void EveryPooledCreatureIsUsable()
        {
            // A summoned creature must be either a combat companion or a rideable mount, or owning it is pointless.
            foreach (var b in Banners)
                foreach (var k in b.AllKinds())
                {
                    var info = CreatureCatalog.For(k);
                    Assert.IsTrue(info.IsCompanion || info.Rideable,
                        $"{k} is neither a companion nor rideable, so it shouldn't be summonable");
                }
        }

        [Test]
        public void TiersAreDisjoint()
        {
            foreach (var b in Banners)
            {
                var all = b.Legendary.Concat(b.Epic).Concat(b.Rare).ToList();
                Assert.AreEqual(all.Count, all.Distinct().Count(),
                    "a creature must appear in exactly one rarity tier");
            }
        }

        [Test]
        public void RarityOfMatchesThePoolMembership()
        {
            var b = SummonBannerCatalog.Standard;
            foreach (var k in b.Legendary) Assert.AreEqual(SummonRarity.Legendary, SummonBannerCatalog.RarityOf(k));
            foreach (var k in b.Epic) Assert.AreEqual(SummonRarity.Epic, SummonBannerCatalog.RarityOf(k));
            foreach (var k in b.Rare) Assert.AreEqual(SummonRarity.Rare, SummonBannerCatalog.RarityOf(k));
        }

        [Test]
        public void FeaturedBannerRateUpsALegendary()
        {
            var f = SummonBannerCatalog.Featured;
            Assert.IsTrue(f.HasFeature);
            Assert.Contains(f.Featured, f.Legendary.ToList(), "the featured creature must be in the Legendary pool");

            Assert.IsFalse(SummonBannerCatalog.Standard.HasFeature, "the standard banner has no rate-up");
        }

        [Test]
        public void ByIdRoundTripsAndFallsBackToStandard()
        {
            Assert.AreEqual(SummonBannerCatalog.Standard.Id, SummonBannerCatalog.ById("standard").Id);
            Assert.AreEqual(SummonBannerCatalog.FeaturedSlotId, SummonBannerCatalog.ById(SummonBannerCatalog.FeaturedSlotId).Id);
            Assert.AreEqual(SummonBannerCatalog.Standard.Id, SummonBannerCatalog.ById("does-not-exist").Id);
        }

        [Test]
        public void PoolsAreNonEmpty()
        {
            foreach (var b in Banners)
            {
                Assert.Greater(b.Legendary.Count, 0);
                Assert.Greater(b.Epic.Count, 0);
                Assert.Greater(b.Rare.Count, 0);
            }
        }
    }
}
