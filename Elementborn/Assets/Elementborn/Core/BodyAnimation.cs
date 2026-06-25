namespace Elementborn.Core
{
    /// <summary>Pure procedural body-animation math for the cel-shaded creatures, enemies, and bosses: a breathing
    /// idle bob and sway, plus a lunge curve for attacks. Engine-free + testable; <c>ProceduralAnimator</c> applies
    /// these to a body's visual each frame and on its attack events, so things move instead of sliding as static
    /// meshes.</summary>
    public static class BodyAnimation
    {
        /// <summary>Idle vertical bob, bounded by <paramref name="amplitude"/>.</summary>
        public static float Bob(float time, float amplitude, float speed) => Sin(time * speed) * amplitude;

        /// <summary>Idle roll/sway in degrees, bounded by <paramref name="amplitudeDeg"/> and offset from the bob.</summary>
        public static float Sway(float time, float amplitudeDeg, float speed) =>
            Sin(time * speed * 0.7f + 1.3f) * amplitudeDeg;

        /// <summary>A quick thrust-and-recover shape for a lunge: 0 at both ends, 1 at the peak. <paramref name="t01"/>
        /// runs 0..1 over the lunge.</summary>
        public static float LungeCurve(float t01)
        {
            if (t01 <= 0f || t01 >= 1f) return 0f;
            return t01 < 0.35f ? (t01 / 0.35f) : (1f - (t01 - 0.35f) / 0.65f);
        }

        private static float Sin(float x) => (float)System.Math.Sin(x);
    }
}
