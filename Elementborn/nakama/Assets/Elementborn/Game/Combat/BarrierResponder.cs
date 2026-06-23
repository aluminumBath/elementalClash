using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Turns a Defend (<see cref="OutcomeKind.Barrier"/>) outcome into a brief defensive shield on the
    /// player: while it's up, incoming direct damage is reduced via <see cref="Damageable.Shield"/>.
    /// Optionally spawns a barrier effect prefab for its duration. Put this on the player rig alongside
    /// the other ability responders (dash, Sanguine Grip).
    /// </summary>
    public sealed class BarrierResponder : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Damageable target;
        [SerializeField] private float duration = 3f;
        [SerializeField, Range(0f, 1f)] private float damageReduction = 0.5f;
        [SerializeField] private GameObject barrierEffectPrefab;

        private void Reset() => combat = GetComponent<PlayerCombatController>();

        private void Awake()
        {
            if (combat == null) combat = GetComponent<PlayerCombatController>();
            if (target == null) target = GetComponentInParent<Damageable>();
        }

        private void OnEnable()
        {
            if (combat != null) combat.OutcomeReady += HandleOutcome;
        }

        private void OnDisable()
        {
            if (combat != null) combat.OutcomeReady -= HandleOutcome;
        }

        private void HandleOutcome(AbilityOutcome outcome, Vector3 origin)
        {
            if (outcome.Kind != OutcomeKind.Barrier || target == null) return;

            target.Shield(duration, damageReduction);

            if (barrierEffectPrefab != null)
            {
                var fx = Instantiate(barrierEffectPrefab, transform.position, transform.rotation, transform);
                Destroy(fx, duration);
            }
        }
    }
}
