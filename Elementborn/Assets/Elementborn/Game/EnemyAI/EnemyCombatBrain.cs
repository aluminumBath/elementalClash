using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(EnemyMovementMotor))]
    [RequireComponent(typeof(EnemyPerceptionSensor))]
    public sealed class EnemyCombatBrain : MonoBehaviour
    {
        [SerializeField] private EnemyCombatProfile profile;
        [SerializeField] private EnemyAiState state = EnemyAiState.Idle;
        [SerializeField] private EnemyPatrolRoute patrolRoute;
        [SerializeField] private EnemyMovementMotor movement;
        [SerializeField] private EnemyPerceptionSensor perception;
        [SerializeField] private EnemyMeleeAttackDriver meleeAttack;
        [SerializeField] private EnemyRangedAttackDriver rangedAttack;
        [SerializeField] private EnemyFleeBehavior fleeBehavior;
        [SerializeField] private EnemyWeaknessAwareSelector weaknessSelector;
        [SerializeField] private SimpleCombatHealth health;
        [SerializeField] private Transform homePoint;
        [SerializeField] private float homeReturnDistance = 2f;

        public EnemyAiState State => state;
        public EnemyCombatProfile Profile => profile;
        public Transform Target => perception != null ? perception.CurrentTarget : null;

        private void Awake()
        {
            if (movement == null) movement = GetComponent<EnemyMovementMotor>();
            if (perception == null) perception = GetComponent<EnemyPerceptionSensor>();
            if (patrolRoute == null) patrolRoute = GetComponent<EnemyPatrolRoute>();
            if (meleeAttack == null) meleeAttack = GetComponent<EnemyMeleeAttackDriver>();
            if (rangedAttack == null) rangedAttack = GetComponent<EnemyRangedAttackDriver>();
            if (fleeBehavior == null) fleeBehavior = GetComponent<EnemyFleeBehavior>();
            if (weaknessSelector == null) weaknessSelector = GetComponent<EnemyWeaknessAwareSelector>();
            if (health == null) health = GetComponent<SimpleCombatHealth>();
            if (perception != null) perception.SetProfile(profile);
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void Update()
        {
            if (state == EnemyAiState.Dead || state == EnemyAiState.Stunned)
            {
                return;
            }

            TickState();
        }

        public void SetProfile(EnemyCombatProfile value)
        {
            profile = value;
            if (perception != null)
            {
                perception.SetProfile(profile);
            }
        }

        public void ForceTarget(Transform target)
        {
            if (perception != null)
            {
                perception.ForceTarget(target);
            }

            state = target != null ? EnemyAiState.Chase : EnemyAiState.Idle;
        }

        public void ForceState(EnemyAiState newState)
        {
            state = newState;
        }

        private void TickState()
        {
            EnemyCombatProfile p = profile;
            if (p == null)
            {
                if (perception != null && perception.Scan() != null)
                {
                    state = EnemyAiState.Chase;
                }

                return;
            }

            Transform target = perception != null && perception.TargetStillValid() ? perception.CurrentTarget : perception != null ? perception.Scan() : null;

            if (target != null && p.FleeWhenLowHealth && fleeBehavior != null && fleeBehavior.ShouldFlee(p.LowHealthFleePercent))
            {
                state = EnemyAiState.Flee;
            }
            else if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= p.AttackRange)
                {
                    state = EnemyAiState.Attack;
                }
                else if (p.CanStrafe && distance <= p.RangedAttackRange)
                {
                    state = EnemyAiState.Strafe;
                }
                else
                {
                    state = EnemyAiState.Chase;
                }
            }
            else if (patrolRoute != null && patrolRoute.HasRoute)
            {
                state = EnemyAiState.Patrol;
            }
            else
            {
                state = EnemyAiState.Idle;
            }

            switch (state)
            {
                case EnemyAiState.Patrol:
                    Patrol(p);
                    break;
                case EnemyAiState.Chase:
                    Chase(p, target);
                    break;
                case EnemyAiState.Strafe:
                    Strafe(p, target);
                    TryRangedAttack(p, target);
                    break;
                case EnemyAiState.Attack:
                    Attack(p, target);
                    break;
                case EnemyAiState.Flee:
                    Flee(p, target);
                    break;
                case EnemyAiState.ReturnHome:
                    ReturnHome(p);
                    break;
                default:
                    movement.Stop();
                    break;
            }
        }

        private void Patrol(EnemyCombatProfile p)
        {
            if (patrolRoute == null || !patrolRoute.HasRoute)
            {
                movement.Stop();
                return;
            }

            movement.MoveTo(patrolRoute.CurrentWaypoint, p.PatrolSpeed);
            patrolRoute.AdvanceIfReached(transform.position);
        }

        private void Chase(EnemyCombatProfile p, Transform target)
        {
            if (target == null)
            {
                movement.Stop();
                return;
            }

            movement.MoveTo(target.position, p.ChaseSpeed);
        }

        private void Strafe(EnemyCombatProfile p, Transform target)
        {
            if (target == null)
            {
                movement.Stop();
                return;
            }

            Vector3 toTarget = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up).normalized;
            Vector3 strafe = Vector3.Cross(Vector3.up, toTarget);
            float wobble = Mathf.Sin(Time.time * 2f) >= 0f ? 1f : -1f;
            movement.MoveDirection(strafe * wobble, p.StrafeSpeed);
            movement.FaceTarget(target);
        }

        private void Attack(EnemyCombatProfile p, Transform target)
        {
            if (target == null)
            {
                movement.Stop();
                return;
            }

            movement.Stop();
            movement.FaceTarget(target);

            if (Vector3.Distance(transform.position, target.position) <= p.AttackRange)
            {
                TryMeleeAttack(p, target);
            }
            else
            {
                TryRangedAttack(p, target);
            }
        }

        private void Flee(EnemyCombatProfile p, Transform target)
        {
            if (fleeBehavior == null)
            {
                movement.Stop();
                return;
            }

            movement.MoveDirection(fleeBehavior.GetFleeDirection(target), p.FleeSpeed);
        }

        private void ReturnHome(EnemyCombatProfile p)
        {
            if (homePoint == null)
            {
                state = EnemyAiState.Idle;
                return;
            }

            movement.MoveTo(homePoint.position, p.PatrolSpeed);
            if (Vector3.Distance(transform.position, homePoint.position) <= homeReturnDistance)
            {
                state = EnemyAiState.Idle;
            }
        }

        private void TryMeleeAttack(EnemyCombatProfile p, Transform target)
        {
            if (meleeAttack == null || target == null || !meleeAttack.CanAttack(p.AttackCooldownSeconds))
            {
                return;
            }

            CombatAttackDefinition selected = weaknessSelector != null && p.UseWeaknesses
                ? weaknessSelector.SelectBestAttack(target.gameObject, p.PreferredElement)
                : null;

            if (selected != null)
            {
                meleeAttack.SetAttackDefinition(selected);
            }

            meleeAttack.Attack(target.gameObject, p.AttackCooldownSeconds);
        }

        private void TryRangedAttack(EnemyCombatProfile p, Transform target)
        {
            if (rangedAttack == null || target == null || !rangedAttack.CanAttack(p.RangedCooldownSeconds))
            {
                return;
            }

            CombatAttackDefinition selected = weaknessSelector != null && p.UseWeaknesses
                ? weaknessSelector.SelectBestAttack(target.gameObject, p.PreferredElement)
                : null;

            if (selected != null)
            {
                rangedAttack.SetAttackDefinition(selected);
            }

            rangedAttack.Attack(target, p.RangedCooldownSeconds);
        }

        private void HandleDeath()
        {
            state = EnemyAiState.Dead;
            if (movement != null)
            {
                movement.Stop();
            }
        }
    }
}
