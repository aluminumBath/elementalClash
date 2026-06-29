using UnityEngine;
using UnityEngine.AI;

namespace Elementborn.Game
{
    public sealed class EnemyMovementMotor : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Rigidbody rigidbodyTarget;
        [SerializeField] private float turnSpeed = 8f;

        private void Awake()
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (rigidbodyTarget == null)
            {
                rigidbodyTarget = GetComponent<Rigidbody>();
            }
        }

        public void MoveTo(Vector3 destination, float speed)
        {
            speed = Mathf.Max(0f, speed);

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.speed = speed;
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(destination);
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(destination - transform.position, Vector3.up);
            MoveDirection(direction, speed);
        }

        public void MoveDirection(Vector3 direction, float speed)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude < 0.001f)
            {
                Stop();
                return;
            }

            direction.Normalize();

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(direction * Mathf.Max(0f, speed) * Time.deltaTime);
            }
            else if (rigidbodyTarget != null && !rigidbodyTarget.isKinematic)
            {
                rigidbodyTarget.MovePosition(rigidbodyTarget.position + direction * Mathf.Max(0f, speed) * Time.deltaTime);
            }
            else
            {
                transform.position += direction * Mathf.Max(0f, speed) * Time.deltaTime;
            }

            FaceDirection(direction);
        }

        public void FaceTarget(Transform target)
        {
            if (target == null)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up);
            FaceDirection(direction);
        }

        public void FaceDirection(Vector3 direction)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * Mathf.Max(0.1f, turnSpeed));
        }

        public void Stop()
        {
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
            }
        }
    }
}
