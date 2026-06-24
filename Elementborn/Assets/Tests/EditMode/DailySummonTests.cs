using System;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class DailySummonTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

        [Test]
        public void NeverClaimedIsAlwaysAvailable()
        {
            Assert.IsTrue(DailySummon.IsAvailable(default, Noon, everClaimed: false));
            // even if the (ignored) timestamp is "today"
            Assert.IsTrue(DailySummon.IsAvailable(Noon, Noon, everClaimed: false));
        }

        [Test]
        public void NotAvailableAgainTheSameUtcDay()
        {
            var later = Noon.AddHours(6); // same calendar day
            Assert.IsFalse(DailySummon.IsAvailable(Noon, later, everClaimed: true));
        }

        [Test]
        public void AvailableOnceTheUtcDayRolls()
        {
            var nextDay = Noon.AddDays(1);
            Assert.IsTrue(DailySummon.IsAvailable(Noon, nextDay, everClaimed: true));
        }

        [Test]
        public void ResetIsMidnightBoundaryNotA24hCooldown()
        {
            var lateClaim = new DateTime(2026, 6, 24, 23, 59, 0, DateTimeKind.Utc);
            var justAfterMidnight = new DateTime(2026, 6, 25, 0, 1, 0, DateTimeKind.Utc);
            // Only ~2 minutes later, but it's a new UTC day, so it's available.
            Assert.IsTrue(DailySummon.IsAvailable(lateClaim, justAfterMidnight, everClaimed: true));
        }

        [Test]
        public void UntilResetIsTimeToNextMidnight()
        {
            Assert.AreEqual(TimeSpan.FromHours(12), DailySummon.UntilReset(Noon));
            var nearMidnight = new DateTime(2026, 6, 24, 23, 30, 0, DateTimeKind.Utc);
            Assert.AreEqual(TimeSpan.FromMinutes(30), DailySummon.UntilReset(nearMidnight));
        }

        [Test]
        public void UntilResetIsNeverNegative()
        {
            // Exactly midnight -> a full day until the next one, never negative.
            var midnight = new DateTime(2026, 6, 24, 0, 0, 0, DateTimeKind.Utc);
            Assert.GreaterOrEqual(DailySummon.UntilReset(midnight).Ticks, 0);
            Assert.AreEqual(TimeSpan.FromDays(1), DailySummon.UntilReset(midnight));
        }
    }
}
