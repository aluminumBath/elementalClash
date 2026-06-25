using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Pure geometry for a Heavy: a committed strike that lands at a point a short way in front of the caster and
    /// hits everything in a blast radius around that point (a meteor / geyser / ground slam / updraft). Distinct
    /// from a Sweep (a wide cone AT the caster) and a Primary (a single travelling projectile): Heavy is a
    /// focused, high-damage impact AT RANGE. No Unity scene needed, so HeavyController stays a thin shell.
    /// </summary>
    public static class HeavyStrike
    {
        /// <summary>How far in front of the caster the strike lands, in metres.</summary>
        public const float ImpactDistance = 3f;

        /// <summary>Blast radius of the impact at zero charge, in metres.</summary>
        public const float ImpactRadius = 2f;

        /// <summary>Seconds between the cast and the impact landing — the telegraph / dodge window.</summary>
        public const float TelegraphSeconds = 0.5f;

        /// <summary>Extra blast radius at full charge, as a fraction of <see cref="ImpactRadius"/>.</summary>
        public const float ChargeRadiusBonus = 0.6f;

        /// <summary>The blast radius for a given cast charge (0..1): the ring and hit grow as you charge.</summary>
        public static float RadiusForCharge(float charge) =>
            ImpactRadius * (1f + ChargeRadiusBonus * Mathf.Clamp01(charge));

        /// <summary>Peak height of the strike's travel arc, in metres (presentation tuning).</summary>
        public const float ArcHeight = 3.5f;

        /// <summary>
        /// A point on the strike's travel arc from <paramref name="start"/> to <paramref name="end"/> at progress
        /// <paramref name="u"/> (0..1): a straight lerp lifted by a parabola that peaks at the midpoint. Pure, so
        /// the projectile's flight path is unit-tested without a scene. u=0 is the start, u=1 is the landing.
        /// </summary>
        public static Vector3 ArcPoint(Vector3 start, Vector3 end, float height, float u)
        {
            u = Mathf.Clamp01(u);
            if (u <= 0f) return start;
            if (u >= 1f) return end;

            Vector3 p = Vector3.Lerp(start, end, u);
            p.y += height * Mathf.Sin(Mathf.PI * u);
            return p;
        }

        /// <summary>Where the strike lands: <paramref name="distance"/> ahead of the origin along the facing.</summary>
        public static Vector3 ImpactPoint(Vector3 origin, Vector3 forward, float distance = ImpactDistance)
        {
            Vector3 facing = forward;
            facing.y = 0f;
            if (facing.sqrMagnitude < 0.0001f) return origin;
            return origin + facing.normalized * distance;
        }

        /// <summary>
        /// True if <paramref name="targetPosition"/> is within <paramref name="radius"/> of
        /// <paramref name="impactPoint"/>, measured on the horizontal (XZ) plane so height doesn't matter.
        /// </summary>
        public static bool Covers(Vector3 impactPoint, Vector3 targetPosition, float radius = ImpactRadius)
        {
            Vector3 delta = targetPosition - impactPoint;
            delta.y = 0f;
            return delta.sqrMagnitude <= radius * radius;
        }
    }
}
