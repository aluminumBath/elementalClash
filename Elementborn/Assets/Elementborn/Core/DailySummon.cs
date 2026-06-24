using System;

namespace Elementborn.Core
{
    /// <summary>
    /// Pure rules for the once-per-day free summon. The "day" is the UTC calendar day, so the free pull refreshes
    /// at UTC midnight rather than 24h after the last claim. No Unity, no ambient clock — the caller passes the
    /// time — so it's unit-tested; <c>SummonController</c> owns the timestamp and persists it.
    /// </summary>
    public static class DailySummon
    {
        /// <summary>True if a free summon is available: never claimed, or last claimed on an earlier UTC day.</summary>
        public static bool IsAvailable(DateTime lastClaimUtc, DateTime nowUtc, bool everClaimed)
            => !everClaimed || nowUtc.Date > lastClaimUtc.Date;

        /// <summary>Time until the next UTC-midnight refresh (clamped to zero; never negative).</summary>
        public static TimeSpan UntilReset(DateTime nowUtc)
        {
            TimeSpan span = nowUtc.Date.AddDays(1) - nowUtc;
            return span < TimeSpan.Zero ? TimeSpan.Zero : span;
        }
    }
}
