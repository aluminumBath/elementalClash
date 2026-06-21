using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Handles the Sanguine Grip (a Control outcome). Raycasts from the cast origin along
    /// the aim, and on the first <see cref="IDamageable"/> hit applies the immobilizing Control
    /// status plus an upward fling. Continuous puppeteering (walking the target around while
    /// held) is an intentional future extension; the immobilize-and-fling grip is complete.
    /// </summary>
    public sealed class SanguineGripController : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Transform castOrigin;
        [SerializeField] private float range = 15f;

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

        private void HandleOutcome(AbilityOutcome outcome, Vector3 origin)
        {
            if (outcome.Kind != OutcomeKind.Control) return;

            Vector3 o = castOrigin ? castOrigin.position : origin;
            Vector3 dir = outcome.Direction.sqrMagnitude > 0.0001f ? outcome.Direction.normalized : transform.forward;

            if (!Physics.Raycast(o, dir, out RaycastHit hit, range)) return;

            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target == null) return;

            if (outcome.Damage > 0f) target.Apply(new DamageInfo(outcome.Damage, outcome.Element, outcome.Variant));
            if (!outcome.Status.IsEmpty) target.ApplyStatus(outcome.Status); // Control = immobilize
            if (outcome.Knockback > 0f) target.ApplyKnockback((dir + Vector3.up) * outcome.Knockback);
        }
    }
}
