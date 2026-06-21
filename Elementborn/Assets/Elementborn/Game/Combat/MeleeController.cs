using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Turns Melee outcomes (weapon swings) into a short forward hit: an overlap sphere ahead of
    /// the cast origin that applies damage, status, and knockback to the first IDamageable caught,
    /// skipping the attacker's own colliders.
    /// </summary>
    public sealed class MeleeController : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Transform origin;
        [SerializeField] private float reach = 1.6f;
        [SerializeField] private float radius = 1.2f;

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
            if (outcome.Kind != OutcomeKind.Melee) return;

            Vector3 dir = outcome.Direction.sqrMagnitude > 0.0001f ? outcome.Direction.normalized : transform.forward;
            Vector3 center = (origin ? origin.position : originPos) + dir * reach;

            foreach (var col in Physics.OverlapSphere(center, radius))
            {
                if (col.transform.root == transform.root) continue; // don't hit self
                var target = col.GetComponentInParent<IDamageable>();
                if (target == null) continue;

                target.Apply(new DamageInfo(outcome.Damage, outcome.Element, outcome.Variant));
                if (!outcome.Status.IsEmpty) target.ApplyStatus(outcome.Status);
                if (outcome.Knockback > 0f) target.ApplyKnockback(dir * outcome.Knockback);
                FactionMember.RegisterHit(col.gameObject, gameObject);
                break;
            }
        }
    }
}
