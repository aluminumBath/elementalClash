using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Turns Heavy outcomes into a committed, TELEGRAPHED impact zone. On cast it fixes where the strike will land
    /// (<see cref="HeavyStrike.ImpactPoint"/>, captured at cast time so it's fair to dodge), shows a ground ring
    /// for <see cref="HeavyStrike.TelegraphSeconds"/>, then drops the blast: every <see cref="IDamageable"/> still
    /// within the (charge-scaled) radius is hit and knocked outward. Charge grows both the damage (from
    /// <see cref="AbilitySystem"/>) and the blast radius / ring. Distinct from the wide near-cone Sweep and the
    /// single-target Melee. The blast math is the pure <see cref="HeavyStrike"/>; visuals are <see cref="AbilityFx"/>.
    /// </summary>
    public sealed class HeavyController : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Transform origin;

        private void Reset() => combat = GetComponent<PlayerCombatController>();

        private void OnEnable()
        {
            if (combat == null) combat = GetComponent<PlayerCombatController>();
            if (combat != null) combat.OutcomeReady += HandleOutcome;
        }

        private void OnDisable()
        {
            if (combat != null) combat.OutcomeReady -= HandleOutcome;
        }

        private void HandleOutcome(AbilityOutcome outcome, Vector3 originPos)
        {
            if (outcome.Kind != OutcomeKind.Heavy || !isActiveAndEnabled) return;

            Vector3 center = origin ? origin.position : originPos;
            Vector3 dir = outcome.Direction.sqrMagnitude > 0.0001f ? outcome.Direction.normalized : transform.forward;
            Vector3 impact = HeavyStrike.ImpactPoint(center, dir);
            float radius = HeavyStrike.RadiusForCharge(outcome.Charge);

            StartCoroutine(TelegraphAndStrike(outcome, center, impact, dir, radius));
        }

        private IEnumerator TelegraphAndStrike(AbilityOutcome outcome, Vector3 start, Vector3 impact, Vector3 dir, float radius)
        {
            GameObject ring = AbilityFx.SpawnTelegraphRing(impact, outcome.Element);
            GameObject travel = AbilityFx.SpawnHeavyTravel(start, outcome.Element, outcome.Variant);

            // The strike arcs in over the flight while the ring fills to telegraph the landing. Impact point is fixed.
            float flight = HeavyStrike.TelegraphSeconds;
            for (float t = 0f; t < flight; t += Time.deltaTime)
            {
                float u = Mathf.Clamp01(t / flight);
                AbilityFx.SetRingFill(ring, radius, u);
                if (travel != null) travel.transform.position = HeavyStrike.ArcPoint(start, impact, HeavyStrike.ArcHeight, u);
                yield return null;
            }

            if (ring != null) Destroy(ring);
            if (travel != null) Destroy(travel);
            AbilityFx.SpawnImpactBurst(impact, radius, outcome.Element, outcome.Variant);

            // Resolve the hit now, on landing — anyone who stepped out of the ring during the arc is spared.
            var seen = new HashSet<IDamageable>();
            foreach (var col in Physics.OverlapSphere(impact, radius))
            {
                if (col.transform.root == transform.root) continue;            // never hit self
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || !seen.Add(target)) continue;             // one hit per target
                if (!HeavyStrike.Covers(impact, col.transform.position, radius)) continue;

                target.Apply(new DamageInfo(outcome.Damage, outcome.Element, outcome.Variant));
                if (!outcome.Status.IsEmpty) target.ApplyStatus(outcome.Status);
                if (outcome.Knockback > 0f)
                {
                    Vector3 away = col.transform.position - impact;
                    away.y = 0f;
                    Vector3 push = away.sqrMagnitude > 0.0001f ? away.normalized : dir;
                    target.ApplyKnockback(push * outcome.Knockback);
                }
                FactionMember.RegisterHit(col.gameObject, gameObject);
            }
        }
    }
}
