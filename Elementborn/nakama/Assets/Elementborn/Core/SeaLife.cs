using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Steering for ambient decorative life (fish). Gently wanders by blending the current heading with a
    /// random nudge, but turns back toward home once it strays past a radius — so a school stays in its area.
    /// Pure: the caller supplies the random nudge, so it tests deterministically.
    /// </summary>
    public static class AmbientWander
    {
        public static Vector3 Steer(Vector3 position, Vector3 heading, Vector3 home, float radius,
            Vector3 randomNudge, float turn = 0.5f)
        {
            Vector3 dir = heading.sqrMagnitude > 0.0001f ? heading.normalized : Vector3.forward;
            dir = Vector3.Slerp(dir, (dir + randomNudge).normalized, Mathf.Clamp01(turn));

            Vector3 toHome = home - position;
            if (toHome.magnitude > radius && toHome.sqrMagnitude > 0.0001f)
                dir = Vector3.Slerp(dir, toHome.normalized, 0.6f); // pull back inside the home radius

            return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
        }
    }

    public enum BossPhase { Calm, Aggressive, Frenzy }

    /// <summary>
    /// A boss's phase by remaining health, and how fast it attacks in each. Pure, so the thresholds are tested
    /// once and the boss controller just reads them.
    /// </summary>
    public static class BossPhases
    {
        public static BossPhase For(float healthFraction)
        {
            if (healthFraction > 0.66f) return BossPhase.Calm;
            if (healthFraction > 0.33f) return BossPhase.Aggressive;
            return BossPhase.Frenzy;
        }

        public static float AttackInterval(BossPhase phase, float baseInterval)
        {
            switch (phase)
            {
                case BossPhase.Aggressive: return baseInterval * 0.7f;
                case BossPhase.Frenzy: return baseInterval * 0.45f;
                default: return baseInterval;
            }
        }
    }
}
