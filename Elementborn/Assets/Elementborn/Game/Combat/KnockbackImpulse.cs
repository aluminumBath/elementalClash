using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Builds a knockback impulse from a hit direction (or a source point), flattening to the ground plane and
    /// adding a small upward pop via the unit-tested <see cref="KnockbackForce"/>. Used by every attack so shoves
    /// read consistently with the hit reactions, instead of inheriting the caster's aim pitch.
    /// </summary>
    public static class KnockbackImpulse
    {
        public static Vector3 Directional(Vector3 direction, float force)
        {
            direction.y = 0f;
            Vector3 dir = direction.sqrMagnitude > 1e-4f ? direction.normalized : Vector3.forward;
            KnockbackForce.Resolve(force, out float h, out float v);
            return dir * h + Vector3.up * v;
        }

        public static Vector3 Radial(Vector3 targetPosition, Vector3 sourcePosition, float force)
            => Directional(targetPosition - sourcePosition, force);
    }
}
