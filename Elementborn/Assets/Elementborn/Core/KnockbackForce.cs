namespace Elementborn.Core
{
    /// <summary>
    /// Pure resolver for a knockback impulse's magnitudes: clamps an ability's designed force to a sane ceiling and
    /// splits it into a horizontal shove plus a small upward "pop", so knocked-back targets read clearly instead of
    /// sliding flatly. UnityEngine-free and unit-tested; the Game layer turns these scalars into a world impulse.
    /// </summary>
    public static class KnockbackForce
    {
        public const float MaxHorizontal = 20f;
        public const float PopRatio = 0.22f;

        public static void Resolve(float baseForce, out float horizontal, out float vertical)
        {
            float f = baseForce < 0f ? 0f : (baseForce > MaxHorizontal ? MaxHorizontal : baseForce);
            horizontal = f;
            vertical = f * PopRatio;
        }
    }
}
