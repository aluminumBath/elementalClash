namespace Elementborn.Core
{
    /// <summary>Pure model for environmental attrition in the vertical world and the deep water:
    /// <b>altitude cold</b> (climb too high and the cold drains health) and <b>underwater pressure</b> (dive too
    /// deep and the pressure does). Each ramps from a safe threshold up to a maximum rate, and each has
    /// element/gear exemptions (the <c>Immune*</c> methods). The Game layer samples the player's altitude and
    /// depth and applies the returned per-second rates as health loss on a tick. This layer is engine-free and
    /// fully testable.</summary>
    public static class EnvironmentHazards
    {
        // --- altitude cold ---
        public const float ColdSafeAltitude = 60f;    // at/below this height above sea level: no cold
        public const float ColdMaxAltitude  = 260f;   // at/above this height: cold is at its worst
        public const float ColdMaxDps       = 6f;     // health lost per second at ColdMaxAltitude

        // --- underwater pressure ---
        public const float PressureSafeDepth = 8f;    // at/above this depth below the surface: no pressure
        public const float PressureMaxDepth  = 120f;  // at/below this depth: pressure is at its worst
        public const float PressureMaxDps    = 7f;    // health lost per second at PressureMaxDepth

        /// <summary>Health lost per second to cold at the given altitude above sea level (0 below the safe ceiling).</summary>
        public static float ColdDamagePerSecond(float altitude) =>
            Ramp(altitude, ColdSafeAltitude, ColdMaxAltitude, ColdMaxDps);

        /// <summary>Health lost per second to pressure at the given depth below the surface (0 above the safe depth,
        /// and 0 when not submerged — i.e. a negative depth).</summary>
        public static float PressureDamagePerSecond(float depth) =>
            Ramp(depth, PressureSafeDepth, PressureMaxDepth, PressureMaxDps);

        /// <summary>Air and Fire channelers shrug off the cold; so does anyone wearing a Fire-enchanted chest piece.</summary>
        public static bool ImmuneToCold(bool isChanneler, Element primary, Element? chestEnchant)
        {
            if (isChanneler && (primary == Element.Air || primary == Element.Fire)) return true;
            return chestEnchant == Element.Fire;
        }

        /// <summary>Water and Earth channelers shrug off the pressure; so does anyone wearing a Water- or
        /// Earth-enchanted chest piece together with an Air- or Water-enchanted helmet.</summary>
        public static bool ImmuneToPressure(bool isChanneler, Element primary, Element? chestEnchant, Element? helmetEnchant)
        {
            if (isChanneler && (primary == Element.Water || primary == Element.Earth)) return true;
            bool chestOk = chestEnchant == Element.Water || chestEnchant == Element.Earth;
            bool helmetOk = helmetEnchant == Element.Air || helmetEnchant == Element.Water;
            return chestOk && helmetOk;
        }

        // Linear ramp: 0 at/below safe, maxRate at/above max, proportional between.
        private static float Ramp(float value, float safe, float max, float maxRate)
        {
            if (value <= safe) return 0f;
            if (value >= max) return maxRate;
            return (value - safe) / (max - safe) * maxRate;
        }
    }
}
