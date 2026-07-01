using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>Shared underwater tuning constants (pure, so tests and systems agree).</summary>
    public static class UnderwaterTuning
    {
        public const float FireThawRadius = 3.5f;
        public const float FireThawAmount = 3f; // seconds of ice life a fire cast removes

        public const float IceTrapRange = 4f;   // how far ahead a water user freezes
        public const float IceTrapRadius = 2.5f;
        public const float IceTrapLife = 6f;

        public const float BubbleRadius = 3f;
        public const float BubbleLife = 8f;
    }

    /// <summary>
    /// A breath meter. It drains while you can't breathe (submerged with no air source) and refills while you
    /// can (at the surface, inside an air bubble, or as a water user). When it empties you're drowning — the
    /// owner applies damage. Pure, so it unit-tests directly.
    /// </summary>
    public sealed class OxygenModel
    {
        public float Max { get; }
        public float Current { get; private set; }
        public float DrainPerSecond { get; set; }
        public float RefillPerSecond { get; set; }

        public float Current01 => Max > 0f ? Mathf.Clamp01(Current / Max) : 0f;
        public bool IsEmpty => Current <= 0f;

        public OxygenModel(float max = 12f, float drainPerSecond = 1f, float refillPerSecond = 4f)
        {
            Max = Mathf.Max(0.01f, max);
            DrainPerSecond = Mathf.Max(0f, drainPerSecond);
            RefillPerSecond = Mathf.Max(0f, refillPerSecond);
            Current = Max;
        }

        public void Tick(float deltaTime, bool breathing)
        {
            if (deltaTime <= 0f) return;
            Current = breathing
                ? Mathf.Min(Max, Current + RefillPerSecond * deltaTime)
                : Mathf.Max(0f, Current - DrainPerSecond * deltaTime);
        }

        public void Refill() => Current = Max;
    }

    /// <summary>
    /// A timed water-breathing boon — e.g. from drinking a Tideglass Draught at the Tidecaller Village. While
    /// active it lets a non-water Channeler breathe below the surface; it ticks down and expires. Granting again
    /// keeps the longer of the current and new remaining time. Pure, so it unit-tests directly.
    /// </summary>
    public sealed class WaterBreathingBoon
    {
        public float Remaining { get; private set; }
        public bool IsActive => Remaining > 0f;

        /// <summary>Fraction remaining against a nominal full duration (for a HUD meter).</summary>
        public float Remaining01(float fullDuration) =>
            fullDuration > 0f ? Mathf.Clamp01(Remaining / fullDuration) : 0f;

        /// <summary>Grant (or extend) the boon; keeps the longer of the current and new remaining time.</summary>
        public void Grant(float seconds)
        {
            if (seconds > Remaining) Remaining = Mathf.Max(0f, seconds);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime > 0f && Remaining > 0f) Remaining = Mathf.Max(0f, Remaining - deltaTime);
        }

        public void Clear() => Remaining = 0f;
    }

    /// <summary>
    /// What an element can do below the surface. Water is in its element — but only its <em>ice</em> offense
    /// works; barriers (ice shells) and movement (swimming/dashing) always pass. Fire, earth, and air offense
    /// fizzles underwater — fire's real job down there is to thaw ice (handled separately), and air's is the
    /// bubble. Pure lookup.
    /// </summary>
    public static class UnderwaterAbilityRules
    {
        public static bool AllowsCast(bool submerged, Element element, AbilityVariant variant, OutcomeKind kind)
        {
            if (!submerged) return true;
            if (kind == OutcomeKind.Barrier || kind == OutcomeKind.Movement) return true;
            if (element == Element.Water) return variant == AbilityVariant.Ice;
            return false;
        }
    }
}
