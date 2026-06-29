using System;
using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class LoginStreakTests
    {
        private static readonly DateTime Day1 = new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

        [Test]
        public void FirstClaimStartsAtOne()
        {
            Assert.AreEqual(1, LoginStreak.NextStreak(default, Day1, 0, everClaimed: false));
        }

        [Test]
        public void ConsecutiveDayIncrements()
        {
            Assert.AreEqual(4, LoginStreak.NextStreak(Day1, Day1.AddDays(1), 3, everClaimed: true));
        }

        [Test]
        public void SameDayDoesNotChange()
        {
            Assert.AreEqual(3, LoginStreak.NextStreak(Day1, Day1.AddHours(5), 3, everClaimed: true));
        }

        [Test]
        public void MissingADayResetsToOne()
        {
            Assert.AreEqual(1, LoginStreak.NextStreak(Day1, Day1.AddDays(2), 9, everClaimed: true));
            Assert.AreEqual(1, LoginStreak.NextStreak(Day1, Day1.AddDays(5), 40, everClaimed: true));
        }

        [Test]
        public void ResetIsMidnightBased()
        {
            var lateClaim = new DateTime(2026, 6, 24, 23, 59, 0, DateTimeKind.Utc);
            var nextMorning = new DateTime(2026, 6, 25, 0, 5, 0, DateTimeKind.Utc);
            // ~6 minutes later but a new UTC day -> the streak continues, not breaks.
            Assert.AreEqual(6, LoginStreak.NextStreak(lateClaim, nextMorning, 5, everClaimed: true));
        }

        [Test]
        public void RewardFollowsTheWeeklyCycle()
        {
            Assert.AreEqual(40, LoginStreak.RewardFor(1));
            Assert.AreEqual(100, LoginStreak.RewardFor(6));
            Assert.AreEqual(200, LoginStreak.RewardFor(7));   // weekly milestone
            Assert.AreEqual(50, LoginStreak.RewardFor(8));    // day 1 of week 2: 40 + 10 loyalty
            Assert.AreEqual(210, LoginStreak.RewardFor(14));  // milestone + 10 loyalty
            Assert.AreEqual(60, LoginStreak.RewardFor(15));   // 40 + 20 loyalty
        }

        [Test]
        public void LoyaltyBonusIsCapped()
        {
            // Very long streak: loyalty maxes at 100, on top of that day's cycle reward.
            int day = 7 + 7 * 20;                  // a milestone day, 20 weeks in
            Assert.AreEqual(200 + 100, LoginStreak.RewardFor(day));
        }

        [Test]
        public void DayInCycleWrapsEverySeven()
        {
            Assert.AreEqual(1, LoginStreak.DayInCycle(1));
            Assert.AreEqual(7, LoginStreak.DayInCycle(7));
            Assert.AreEqual(1, LoginStreak.DayInCycle(8));
            Assert.AreEqual(2, LoginStreak.DayInCycle(9));
        }

        [Test]
        public void RewardClampsNonPositiveStreak()
        {
            Assert.AreEqual(LoginStreak.RewardFor(1), LoginStreak.RewardFor(0));
        }
    }
}
