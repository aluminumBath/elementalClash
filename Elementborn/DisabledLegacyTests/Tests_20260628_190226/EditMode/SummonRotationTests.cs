using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class SummonRotationTests
    {
        [Test]
        public void FeaturedForPeriodCyclesAndWraps()
        {
            int n = SummonBannerCatalog.FeaturedRotationLength;
            Assert.Greater(n, 1, "a rotation needs more than one entry to be a rotation");

            // The cycle repeats every n periods.
            Assert.AreEqual(SummonBannerCatalog.FeaturedForPeriod(0).Featured,
                            SummonBannerCatalog.FeaturedForPeriod(n).Featured);

            // Negative periods wrap to the same slot as their positive counterpart.
            Assert.AreEqual(SummonBannerCatalog.FeaturedForPeriod(n - 1).Featured,
                            SummonBannerCatalog.FeaturedForPeriod(-1).Featured);
        }

        [Test]
        public void EveryRotationEntryIsALegendaryUnderTheStableSlotId()
        {
            int n = SummonBannerCatalog.FeaturedRotationLength;
            for (int p = 0; p < n; p++)
            {
                var b = SummonBannerCatalog.FeaturedForPeriod(p);
                Assert.IsTrue(b.HasFeature, $"period {p} should be a featured banner");
                Assert.AreEqual(SummonBannerCatalog.FeaturedSlotId, b.Id, "the slot id must stay stable for pity carry-over");
                Assert.AreEqual(SummonRarity.Legendary, SummonBannerCatalog.RarityOf(b.Featured),
                    $"the featured creature for period {p} must be a Legendary");
                Assert.Contains(b.Featured, b.Legendary.ToList(), "the featured creature must be in the Legendary pool");
            }
        }

        [Test]
        public void RotationCoversDistinctLegendaries()
        {
            int n = SummonBannerCatalog.FeaturedRotationLength;
            var featured = new List<CreatureKind>();
            for (int p = 0; p < n; p++) featured.Add(SummonBannerCatalog.FeaturedForPeriod(p).Featured);
            Assert.AreEqual(n, featured.Distinct().Count(), "each rotation slot should feature a different creature");
        }

        [Test]
        public void DefaultFeaturedIsRotationPeriodZero()
        {
            Assert.AreEqual(SummonBannerCatalog.FeaturedForPeriod(0).Featured, SummonBannerCatalog.Featured.Featured);
            Assert.AreEqual(CreatureKind.Phoenix, SummonBannerCatalog.Featured.Featured, "period 0 is the Flamecaller (Phoenix) banner");
        }

        [Test]
        public void PeriodForCountsWholeWindowsSinceEpoch()
        {
            var epoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.AreEqual(0, SummonBannerCatalog.PeriodFor(epoch, epoch, 7));
            Assert.AreEqual(0, SummonBannerCatalog.PeriodFor(epoch.AddDays(6.9), epoch, 7), "still inside the first window");
            Assert.AreEqual(1, SummonBannerCatalog.PeriodFor(epoch.AddDays(7), epoch, 7), "the second window begins at +7 days");
            Assert.AreEqual(3, SummonBannerCatalog.PeriodFor(epoch.AddDays(7 * 3 + 2), epoch, 7));
        }

        [Test]
        public void PeriodForClampsBeforeEpochAndGuardsBadLength()
        {
            var epoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(0, SummonBannerCatalog.PeriodFor(epoch.AddDays(-100), epoch, 7), "instants before the epoch clamp to 0");
            // A non-positive period length is treated as 1 day, not a divide-by-zero.
            Assert.AreEqual(5, SummonBannerCatalog.PeriodFor(epoch.AddDays(5), epoch, 0));
        }
    }
}
