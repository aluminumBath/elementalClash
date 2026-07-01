namespace Elementborn.Core
{
    /// <summary>
    /// Area-burst damage falloff: full damage at the center, fading linearly to zero at the radius edge. Used by the
    /// phoenix death blast (and reusable by any radial burst). UnityEngine-free and unit-tested.
    /// </summary>
    public static class Blast
    {
        /// <summary>Damage at <paramref name="distance"/> from a burst of <paramref name="maxDamage"/> over
        /// <paramref name="radius"/>: full at the center, 0 at/beyond the edge, linear between.</summary>
        public static float DamageAt(float maxDamage, float distance, float radius)
        {
            if (maxDamage <= 0f || radius <= 0f) return 0f;
            if (distance <= 0f) return maxDamage;
            if (distance >= radius) return 0f;
            return maxDamage * (1f - distance / radius);
        }
    }
}
