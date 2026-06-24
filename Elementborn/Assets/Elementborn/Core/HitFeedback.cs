namespace Elementborn.Core
{
    /// <summary>
    /// Pure math for combat-hit presentation: how strong a hit "reads" (for scaling shake / flash / hit-stop) and
    /// the squash-and-recover curve a struck target plays. UnityEngine-free and unit-tested so the feel layer can
    /// stay deterministic.
    /// </summary>
    public static class HitFeedback
    {
        /// <summary>Normalised hit strength in [0,1]: <paramref name="amount"/> at or above
        /// <paramref name="reference"/> reads as a full-strength hit.</summary>
        public static float Intensity01(float amount, float reference)
        {
            if (reference <= 0f) return amount > 0f ? 1f : 0f;
            float v = amount / reference;
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }

        /// <summary>A squash-and-recover scale multiplier over a hit reaction: 1 at the start and end, dipping to
        /// <c>1 - strength</c> in the middle. <paramref name="elapsed"/> outside [0,duration] returns 1.</summary>
        public static float SquashScale(float elapsed, float duration, float strength)
        {
            if (duration <= 0f || elapsed <= 0f || elapsed >= duration) return 1f;
            float s = strength < 0f ? 0f : (strength > 0.9f ? 0.9f : strength);
            float k = (float)System.Math.Sin((elapsed / duration) * System.Math.PI); // 0 → 1 → 0
            return 1f - s * k;
        }
    }
}
