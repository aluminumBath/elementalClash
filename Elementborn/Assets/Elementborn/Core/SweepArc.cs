using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Pure geometry for a Sweep: a wide, short-range arc in front of the caster. Decides which targets a sweep
    /// covers — those within <see cref="Range"/> and inside the <see cref="HalfAngleDegrees"/> cone around the
    /// facing direction, judged on the horizontal plane. No Unity scene needed, so the presentation
    /// (SweepController) is a thin shell over this and the math is unit-tested directly.
    /// </summary>
    public static class SweepArc
    {
        /// <summary>Half the arc's width, in degrees. 60 gives a 120-degree fan.</summary>
        public const float HalfAngleDegrees = 60f;

        /// <summary>How far the sweep reaches, in metres.</summary>
        public const float Range = 3.5f;

        /// <summary>
        /// True if <paramref name="targetPosition"/> lies within <paramref name="range"/> of
        /// <paramref name="origin"/> and inside the cone of half-angle <paramref name="halfAngleDegrees"/> around
        /// <paramref name="forward"/>. Distance and angle are measured on the horizontal (XZ) plane, so height
        /// differences don't matter. A target at the origin counts as covered (point-blank).
        /// </summary>
        public static bool Covers(Vector3 origin, Vector3 forward, Vector3 targetPosition,
            float range = Range, float halfAngleDegrees = HalfAngleDegrees)
        {
            Vector3 toTarget = targetPosition - origin;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            if (distance > range) return false;
            if (distance < 0.0001f) return true; // point-blank

            Vector3 facing = forward;
            facing.y = 0f;
            if (facing.sqrMagnitude < 0.0001f) return true; // no facing -> omni at close range

            return Vector3.Angle(facing, toTarget) <= halfAngleDegrees + 0.001f;
        }
    }
}
