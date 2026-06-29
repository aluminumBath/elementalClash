using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Add to creature enemies to periodically pounce toward the current target.
    /// </summary>
    public sealed class CreaturePounceBehavior : MonoBehaviour
    {
        [SerializeField] private EnemyCombatBrain brain;
        [SerializeField] private EnemyMeleeAttackDriver meleeAttack;
        [SerializeField] private float pounceRange = 6f;
        [SerializeField] private float pounceCooldown = 4f;
        [SerializeField] private float pounceSpeed = 8f;

        private float lastPounceAt = -999f;

        private void Awake()
        {
            if (brain == null) brain = GetComponent<EnemyCombatBrain>();
            if (meleeAttack == null) meleeAttack = GetComponent<EnemyMeleeAttackDriver>();
        }

        private void Update()
        {
            if (brain == null || brain.Target == null)
            {
                return;
            }

            if (Time.unscaledTime < lastPounceAt + pounceCooldown)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, brain.Target.position);
            if (distance > pounceRange)
            {
                return;
            }

            lastPounceAt = Time.unscaledTime;
            Vector3 direction = Vector3.ProjectOnPlane(brain.Target.position - transform.position, Vector3.up).normalized;
            transform.position += direction * pounceSpeed * Time.deltaTime;
            if (meleeAttack != null)
            {
                meleeAttack.Attack(brain.Target.gameObject, pounceCooldown);
            }
        }
    }
}
