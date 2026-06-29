using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Add to sea raiders or boat-mounted enemies. It keeps a distance band and uses ranged boat attacks.
    /// </summary>
    public sealed class BoatEnemyCombatBehavior : MonoBehaviour
    {
        [SerializeField] private EnemyCombatBrain brain;
        [SerializeField] private EnemyRangedAttackDriver rangedAttack;
        [SerializeField] private float desiredDistance = 10f;
        [SerializeField] private float broadsideCooldown = 3f;

        private float lastBroadsideAt = -999f;

        private void Awake()
        {
            if (brain == null) brain = GetComponent<EnemyCombatBrain>();
            if (rangedAttack == null) rangedAttack = GetComponent<EnemyRangedAttackDriver>();
        }

        private void Update()
        {
            if (brain == null || rangedAttack == null || brain.Target == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, brain.Target.position);
            if (distance <= desiredDistance && Time.unscaledTime >= lastBroadsideAt + broadsideCooldown)
            {
                lastBroadsideAt = Time.unscaledTime;
                rangedAttack.Attack(brain.Target, broadsideCooldown);
            }
        }
    }
}
