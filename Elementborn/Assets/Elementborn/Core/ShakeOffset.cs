namespace Elementborn.Core
{
    /// <summary>
    /// Deterministic decaying camera-shake offset — pure and unit-tested so the feel is predictable and the
    /// Game-layer <c>CameraShaker</c> stays a thin driver. Produces a 2D offset that oscillates via seeded sine
    /// waves and eases to exactly zero at <c>duration</c>. No UnityEngine, so it runs in EditMode tests.
    /// </summary>
    public static class ShakeOffset
    {
        /// <summary>
        /// Offset at <paramref name="elapsed"/> seconds into a shake of total <paramref name="duration"/>. X and Y
        /// use different frequencies/seeds so the motion isn't a straight line. Neither axis ever exceeds
        /// <paramref name="amplitude"/> in magnitude; returns (0,0) at or past the end and for non-positive inputs.
        /// </summary>
        public static void Evaluate(float elapsed, float duration, float amplitude, float frequency,
                                    float seedX, float seedY, out float x, out float y)
        {
            x = 0f; y = 0f;
            if (duration <= 0f || amplitude <= 0f || elapsed < 0f || elapsed >= duration) return;
            float fade = 1f - (elapsed / duration); // linear 1 -> 0
            fade *= fade;                           // quadratic ease-out
            float a = amplitude * fade;
            x = a * Sin((elapsed * frequency) + seedX);
            y = a * Sin((elapsed * frequency * 1.3f) + seedY);
        }

        // Local sine keeps Core free of UnityEngine; System.Math is fine here.
        private static float Sin(float t) => (float)System.Math.Sin(t);
    }
}
