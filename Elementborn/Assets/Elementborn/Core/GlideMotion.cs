namespace Elementborn.Core
{
    /// <summary>Pure helpers for the glide. Tapping jump leaps; holding it while falling deploys a glide that
    /// caps how fast you drop (a slow, controlled descent) and gives a little forward push — so you can ride down
    /// from a peak or cross a gap. Engine-free and testable; the rig feeds it the live vertical velocity.</summary>
    public static class GlideMotion
    {
        public const float DefaultGlideFallSpeed = 2.2f;   // metres/second descent while gliding
        public const float GlideForwardMultiplier = 1.35f; // horizontal-speed boost while gliding

        /// <summary>A glide is active only when airborne, descending, and the glide button is held. At the top of a
        /// jump (still rising) it stays off, so the leap plays out before the glide takes over.</summary>
        public static bool IsGliding(bool grounded, float verticalVelocity, bool glideHeld)
            => !grounded && glideHeld && verticalVelocity < 0f;

        /// <summary>Clamp a downward velocity to the glide descent rate. A non-negative (rising/level) velocity is
        /// returned unchanged.</summary>
        public static float ClampDescent(float verticalVelocity, float glideFallSpeed)
        {
            float floor = -System.Math.Abs(glideFallSpeed);
            return verticalVelocity < floor ? floor : verticalVelocity;
        }
    }
}
