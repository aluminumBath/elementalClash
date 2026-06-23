using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Turns Sweep outcomes into a wide, multi-target arc in front of the caster: an overlap sphere of
    /// <see cref="SweepArc.Range"/> filtered to the <see cref="SweepArc"/> cone, applying damage, status, and
    /// knockback to EVERY <see cref="IDamageable"/> caught (unlike the single-target <see cref="MeleeController"/>).
    /// The cone test is the pure <see cref="SweepArc"/>, so this stays a thin shell. Each element's rider (Fire
    /// burn, Water slow, Earth stagger, Air pure knockback) rides in on the outcome from <see cref="AbilitySystem"/>.
    /// </summary>
    public sealed class SweepController : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Transform origin;

        private readonly HashSet<IDamageable> _seen = new HashSet<IDamageable>();

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
            if (outcome.Kind != OutcomeKind.Sweep) return;

            Vector3 center = origin ? origin.position : originPos;
            Vector3 dir = outcome.Direction.sqrMagnitude > 0.0001f ? outcome.Direction.normalized : transform.forward;

            AbilityFx.SpawnSweepFan(center, dir, outcome.Element, outcome.Variant); // fast forward fan

            _seen.Clear();
            foreach (var col in Physics.OverlapSphere(center, SweepArc.Range))
            {
                if (col.transform.root == transform.root) continue;            // never hit self
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null || !_seen.Add(target)) continue;            // one hit per target
                if (!SweepArc.Covers(center, dir, col.transform.position)) continue; // outside the arc

                target.Apply(new DamageInfo(outcome.Damage, outcome.Element, outcome.Variant));
                if (!outcome.Status.IsEmpty) target.ApplyStatus(outcome.Status);
                if (outcome.Knockback > 0f) target.ApplyKnockback(dir * outcome.Knockback);
                FactionMember.RegisterHit(col.gameObject, gameObject);
                // no break — a sweep catches everyone in the arc
            }
        }
    }
}
