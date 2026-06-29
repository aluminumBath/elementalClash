using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyFleeBehavior : MonoBehaviour
    {
        [SerializeField] private SimpleCombatHealth health;
        [SerializeField] private float fleeHealthPercent = 0.2f;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<SimpleCombatHealth>();
            }
        }

        public bool ShouldFlee(float overridePercent = -1f)
        {
            if (health == null || health.MaxHealth <= 0f)
            {
                return false;
            }

            float threshold = overridePercent >= 0f ? overridePercent : fleeHealthPercent;
            return health.CurrentHealth / health.MaxHealth <= Mathf.Clamp01(threshold);
        }

        public Vector3 GetFleeDirection(Transform threat)
        {
            if (threat == null)
            {
                return -transform.forward;
            }

            Vector3 direction = Vector3.ProjectOnPlane(transform.position - threat.position, Vector3.up);
            return direction.sqrMagnitude > 0.001f ? direction.normalized : -transform.forward;
        }
    }
}
