namespace Elementborn.Core
{
    /// <summary>
    /// Pure math for a floating enemy health bar: the fill fraction, and a "show on damage, then retract after a
    /// lull" visibility curve. UnityEngine-free and unit-tested so the runtime component stays a thin shell.
    /// </summary>
    public static class HealthBarState
    {
        /// <summary>Clamped current/max in [0,1]; a non-positive max reads as empty.</summary>
        public static float Fraction(float current, float max)
        {
            if (max <= 0f) return 0f;
            float f = current / max;
            return f < 0f ? 0f : (f > 1f ? 1f : f);
        }

        /// <summary>
        /// Visibility in [0,1]: 1 right after damage, held for <paramref name="holdSeconds"/>, then ramping to 0
        /// across <paramref name="fadeSeconds"/> once the lull passes (used to retract the bar).
        /// </summary>
        public static float Alpha(float secondsSinceDamage, float holdSeconds, float fadeSeconds)
        {
            if (secondsSinceDamage <= holdSeconds) return 1f;
            if (fadeSeconds <= 0f) return 0f;
            float f = (secondsSinceDamage - holdSeconds) / fadeSeconds;
            return f >= 1f ? 0f : 1f - f;
        }
    }
}
