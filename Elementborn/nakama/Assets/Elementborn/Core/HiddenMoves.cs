using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>The four hidden "signature" moves, one per element.</summary>
    public enum HiddenMove { None, WaterDash, RockArmor, AirTornado, FireBreath }

    /// <summary>
    /// Maps each element to its signature move and holds the tuning. Pure, so the mapping round-trips in tests
    /// and the controller/provider share one source of numbers.
    /// </summary>
    public static class HiddenMoves
    {
        public static HiddenMove For(Element element) => element switch
        {
            Element.Water => HiddenMove.WaterDash,
            Element.Earth => HiddenMove.RockArmor,
            Element.Air => HiddenMove.AirTornado,
            Element.Fire => HiddenMove.FireBreath,
            _ => HiddenMove.None
        };

        public static Element ElementOf(HiddenMove move) => move switch
        {
            HiddenMove.WaterDash => Element.Water,
            HiddenMove.RockArmor => Element.Earth,
            HiddenMove.AirTornado => Element.Air,
            HiddenMove.FireBreath => Element.Fire,
            _ => Element.Water
        };

        // Water: a wrist-spin propels you forward (underwater).
        public const float WaterDashSpeed = 14f;
        public const float WaterDashDuration = 0.35f;

        // Earth: crossed arms encase you in rock — big defense, but slowed.
        public const float RockArmorReduction = 0.7f; // 70% of incoming damage cut
        public const float RockArmorSlow = 0.5f;       // 50% slower while armored
        public const float RockArmorDuration = 6f;

        // Air: a full-body spin throws a tornado that launches everyone near (you too).
        public const float TornadoRadius = 6f;
        public const float TornadoLaunch = 12f;        // upward impulse on others
        public const float TornadoDamage = 8f;
        public const float TornadoSelfLaunch = 8f;

        // Fire: a hand at the mouth + a button breathes a cone of fire.
        public const float FireBreathRange = 7f;
        public const float FireBreathConeDegrees = 45f;
        public const float FireBreathDamage = 10f;
        public const float FireBreathBurnDps = 6f;
        public const float FireBreathBurnDuration = 3f;
    }

    /// <summary>
    /// Pure predicates for recognising the special hidden-move gestures from raw pose/motion values. The VR
    /// provider feeds these device readings; keeping the maths here makes the thresholds testable.
    /// </summary>
    public static class HiddenGestures
    {
        /// <summary>A wrist twist: angular speed past a threshold (radians/second).</summary>
        public static bool WristSpinning(Vector3 angularVelocity, float minRadiansPerSecond)
            => angularVelocity.magnitude >= minRadiansPerSecond;

        /// <summary>A near-complete body turn (accumulated yaw degrees over a window).</summary>
        public static bool CompletedFullTurn(float accumulatedDegrees, float threshold = 330f)
            => Mathf.Abs(accumulatedDegrees) >= threshold;

        /// <summary>Both hands pulled in high and close — the crossed-arms armor pose.</summary>
        public static bool HandsCrossedHigh(Vector3 leftHand, Vector3 rightHand, Vector3 head,
            float maxDropBelowHead = 0.5f, float maxSeparation = 0.45f)
        {
            bool leftHigh = leftHand.y > head.y - maxDropBelowHead && leftHand.y <= head.y + 0.1f;
            bool rightHigh = rightHand.y > head.y - maxDropBelowHead && rightHand.y <= head.y + 0.1f;
            bool together = (leftHand - rightHand).magnitude <= maxSeparation;
            return leftHigh && rightHigh && together;
        }

        /// <summary>A hand brought up near the head/mouth.</summary>
        public static bool HandNearHead(Vector3 hand, Vector3 head, float maxDistance = 0.3f)
            => (hand - head).sqrMagnitude <= maxDistance * maxDistance;
    }
}
