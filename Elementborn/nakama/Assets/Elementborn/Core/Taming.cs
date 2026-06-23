namespace Elementborn.Core
{
    /// <summary>Result of a taming attempt.</summary>
    public readonly struct TameOutcome
    {
        public readonly bool Success;
        public readonly bool LureConsumed;
        public readonly string Reason;

        public TameOutcome(bool success, bool lureConsumed, string reason)
        {
            Success = success;
            LureConsumed = lureConsumed;
            Reason = reason;
        }

        public static TameOutcome Fail(string reason) => new TameOutcome(false, false, reason);
    }

    /// <summary>
    /// Pure taming rules. To tame a creature you need its specific lure AND it must be weakened (health
    /// at or below <see cref="WeakenThreshold"/>); then success is a chance from the creature's
    /// <see cref="CreatureInfo.TameChance"/>. The lure is consumed on any genuine attempt. No Unity
    /// dependency, so it unit-tests directly.
    /// </summary>
    public static class TamingRules
    {
        public const float WeakenThreshold = 0.25f;

        public static bool CanAttempt(CreatureInfo info, float healthFraction, bool hasLure, out string reason)
        {
            if (!hasLure) { reason = "Need the right lure"; return false; }
            if (healthFraction > WeakenThreshold) { reason = "Weaken it first"; return false; }
            reason = "Ready";
            return true;
        }

        public static TameOutcome Resolve(CreatureInfo info, float healthFraction, bool hasLure, IRandomSource rng)
        {
            if (!CanAttempt(info, healthFraction, hasLure, out string reason))
                return TameOutcome.Fail(reason);

            bool success = rng.NextUnit() < info.TameChance;
            return new TameOutcome(success, true, success ? "Tamed!" : "It broke free");
        }
    }
}
