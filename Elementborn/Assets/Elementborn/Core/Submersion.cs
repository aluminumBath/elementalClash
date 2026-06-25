namespace Elementborn.Core
{
    /// <summary>
    /// Pure submersion math: how far a point sits below a flat water surface. Engine-free so the
    /// pressure term of <c>EnvironmentHazards</c> is unit-testable. Only the vertical relationship
    /// lives here; the XZ footprint test (is the point actually *in* this body of water?) belongs to
    /// the Game-layer <c>WaterBody</c> that calls this.
    /// </summary>
    public static class Submersion
    {
        /// <summary>Depth of <paramref name="pointY"/> below <paramref name="surfaceY"/>: a positive
        /// value when submerged, exactly 0 at or above the surface (never negative, so it feeds the
        /// pressure ramp cleanly).</summary>
        public static float Depth(float surfaceY, float pointY)
            => pointY < surfaceY ? surfaceY - pointY : 0f;

        /// <summary>True when <paramref name="pointY"/> is below <paramref name="surfaceY"/>.</summary>
        public static bool IsSubmerged(float surfaceY, float pointY) => pointY < surfaceY;
    }
}
