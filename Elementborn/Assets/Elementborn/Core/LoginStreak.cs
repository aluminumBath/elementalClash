using System;

namespace Elementborn.Core
{
    /// <summary>
    /// Consecutive-day login streak, sharing the daily summon's UTC-day clock. Claiming on the next UTC day
    /// continues the streak; skipping a day resets it to 1. Rewards run on a weekly cycle that escalates to a
    /// milestone on every 7th day, plus a small loyalty bonus that grows with each completed week (capped). Pure
    /// (caller supplies the time) — unit-tested; <c>SummonController</c> drives it from the daily claim.
    /// </summary>
    public static class LoginStreak
    {
        public const int CycleLength = 7;

        // Sigil reward by day-of-cycle (1..7); day 7 is the weekly milestone.
        private static readonly int[] CycleRewards = { 40, 40, 60, 60, 80, 100, 200 };

        private const int LoyaltyPerWeek = 10;   // extra Sigils per fully-completed week...
        private const int LoyaltyCap = 100;      // ...up to this ceiling.

        /// <summary>
        /// The streak after a claim: 1 if never claimed or a day was skipped; +1 if it's the next UTC day;
        /// unchanged if somehow claimed again the same UTC day.
        /// </summary>
        public static int NextStreak(DateTime lastClaimUtc, DateTime nowUtc, int currentStreak, bool everClaimed)
        {
            if (!everClaimed) return 1;
            int safeCurrent = currentStreak < 1 ? 1 : currentStreak;
            int dayGap = (nowUtc.Date - lastClaimUtc.Date).Days;
            if (dayGap <= 0) return safeCurrent;        // same day (already claimed today)
            if (dayGap == 1) return safeCurrent + 1;    // consecutive day
            return 1;                                    // a day was missed — streak broken
        }

        /// <summary>Sigils granted for reaching <paramref name="streak"/> (1-based).</summary>
        public static int RewardFor(int streak)
        {
            if (streak < 1) streak = 1;
            int cycleReward = CycleRewards[(streak - 1) % CycleLength];
            int weeksCompleted = (streak - 1) / CycleLength;
            int loyalty = Math.Min(weeksCompleted * LoyaltyPerWeek, LoyaltyCap);
            return cycleReward + loyalty;
        }

        /// <summary>The 1-based day within the current weekly cycle (1..7).</summary>
        public static int DayInCycle(int streak)
        {
            if (streak < 1) streak = 1;
            return ((streak - 1) % CycleLength) + 1;
        }
    }
}
