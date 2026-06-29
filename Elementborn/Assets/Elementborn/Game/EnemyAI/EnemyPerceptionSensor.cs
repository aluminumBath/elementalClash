using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyPerceptionSensor : MonoBehaviour
    {
        [SerializeField] private EnemyCombatProfile profile;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool requireLineOfSight = false;
        [SerializeField] private Transform eyePoint;

        public Transform CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null;

        private void Awake()
        {
            if (eyePoint == null)
            {
                eyePoint = transform;
            }
        }

        public Transform Scan()
        {
            float range = profile != null ? profile.SightRange : 14f;
            Collider[] hits = Physics.OverlapSphere(transform.position, range, targetMask, QueryTriggerInteraction.Ignore);

            Transform best = null;
            float bestDistance = float.PositiveInfinity;

            foreach (var hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                Transform candidate = hit.attachedRigidbody != null ? hit.attachedRigidbody.transform : hit.transform;
                if (candidate == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(playerTag) && !candidate.CompareTag(playerTag) && !candidate.root.CompareTag(playerTag))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, candidate.position);
                if (distance >= bestDistance)
                {
                    continue;
                }

                if (requireLineOfSight && !CanSee(candidate))
                {
                    continue;
                }

                best = candidate;
                bestDistance = distance;
            }

            CurrentTarget = best;
            return CurrentTarget;
        }

        public bool TargetStillValid()
        {
            if (CurrentTarget == null)
            {
                return false;
            }

            float loseRange = profile != null ? profile.LoseTargetRange : 24f;
            if (Vector3.Distance(transform.position, CurrentTarget.position) > loseRange)
            {
                CurrentTarget = null;
                return false;
            }

            if (requireLineOfSight && !CanSee(CurrentTarget))
            {
                return false;
            }

            return true;
        }

        public void SetProfile(EnemyCombatProfile value)
        {
            profile = value;
        }

        public void ForceTarget(Transform target)
        {
            CurrentTarget = target;
        }

        private bool CanSee(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = eyePoint != null ? eyePoint.position : transform.position;
            Vector3 direction = target.position - origin;
            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.transform == target || hit.transform.root == target.root;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            float range = profile != null ? profile.SightRange : 14f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
