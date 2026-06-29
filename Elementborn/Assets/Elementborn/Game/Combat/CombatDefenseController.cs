using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    [RequireComponent(typeof(StaminaResource))]
    public sealed class CombatDefenseController : MonoBehaviour
    {
        [SerializeField] private CombatStaminaTuning tuning;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Rigidbody rigidbodyTarget;
        [SerializeField] private Transform movementReference;
        [SerializeField] private UnityEvent onBlockStarted;
        [SerializeField] private UnityEvent onBlockEnded;
        [SerializeField] private UnityEvent onPerfectBlock;
        [SerializeField] private UnityEvent onDodgeStarted;
        [SerializeField] private UnityEvent onDodgeEnded;

        private StaminaResource stamina;
        private CombatDefenseState state = CombatDefenseState.Normal;
        private float blockStartedAt = -999f;
        private float dodgeStartedAt = -999f;
        private float lastDodgeEndedAt = -999f;
        private Vector3 dodgeDirection = Vector3.forward;
        private bool dodgeEndingInvoked;
        private CombatStaminaTuning runtimeFallbackTuning;

        public CombatDefenseState State => state;
        public bool IsBlocking => state == CombatDefenseState.Blocking;
        public bool IsDodging => state == CombatDefenseState.Dodging;
        public bool IsInvulnerable => IsDodging && Time.unscaledTime <= dodgeStartedAt + GetTuning().DodgeIFrameSeconds;
        public StaminaResource Stamina => stamina;

        private void Awake()
        {
            stamina = GetComponent<StaminaResource>();
            if (tuning != null)
            {
                stamina.SetTuning(tuning);
            }

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            if (rigidbodyTarget == null)
            {
                rigidbodyTarget = GetComponent<Rigidbody>();
            }

            if (movementReference == null)
            {
                movementReference = transform;
            }
        }

        private void Update()
        {
            if (state == CombatDefenseState.Blocking)
            {
                float drain = GetTuning().BlockDrainPerSecond * Time.deltaTime;
                if (drain > 0f && !stamina.TrySpend(drain))
                {
                    EndBlock();
                }
            }

            if (state == CombatDefenseState.Dodging)
            {
                ContinueDodge();
            }
        }

        public bool BeginBlock()
        {
            if (state == CombatDefenseState.Dodging)
            {
                return false;
            }

            if (state == CombatDefenseState.Blocking)
            {
                return true;
            }

            if (!stamina.TrySpend(GetTuning().BlockStartCost))
            {
                return false;
            }

            state = CombatDefenseState.Blocking;
            blockStartedAt = Time.unscaledTime;
            onBlockStarted?.Invoke();
            return true;
        }

        public void EndBlock()
        {
            if (state != CombatDefenseState.Blocking)
            {
                return;
            }

            state = CombatDefenseState.Normal;
            onBlockEnded?.Invoke();
        }

        public bool TryDodge(Vector3 direction)
        {
            if (state == CombatDefenseState.Dodging)
            {
                return false;
            }

            if (Time.unscaledTime < lastDodgeEndedAt + GetTuning().DodgeCooldownSeconds)
            {
                return false;
            }

            if (!stamina.TrySpend(GetTuning().DodgeCost))
            {
                return false;
            }

            EndBlock();

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = movementReference != null ? movementReference.forward : transform.forward;
            }

            dodgeDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            if (dodgeDirection.sqrMagnitude < 0.001f)
            {
                dodgeDirection = transform.forward;
            }

            state = CombatDefenseState.Dodging;
            dodgeStartedAt = Time.unscaledTime;
            dodgeEndingInvoked = false;
            onDodgeStarted?.Invoke();
            return true;
        }

        public float ModifyIncomingDamage(float incomingDamage, AbilityElementType element, GameObject source, out bool perfectBlocked, out bool dodged)
        {
            perfectBlocked = false;
            dodged = false;

            if (incomingDamage <= 0f)
            {
                return 0f;
            }

            if (IsInvulnerable)
            {
                dodged = true;
                return 0f;
            }

            if (!IsBlocking)
            {
                return incomingDamage;
            }

            bool perfect = Time.unscaledTime <= blockStartedAt + GetTuning().PerfectBlockWindowSeconds;
            perfectBlocked = perfect;

            float reduction = perfect ? GetTuning().PerfectBlockReductionPercent : GetTuning().BlockDamageReductionPercent;
            float finalDamage = incomingDamage * (1f - Mathf.Clamp01(reduction / 100f));

            if (perfect)
            {
                stamina.Restore(GetTuning().PerfectBlockStaminaRefund);
                onPerfectBlock?.Invoke();
                NotificationFeed.Post("Perfect block!", NotificationType.Combat);
            }
            else
            {
                NotificationFeed.Post("Blocked.", NotificationType.Combat);
            }

            return Mathf.Max(0f, finalDamage);
        }

        private void ContinueDodge()
        {
            CombatStaminaTuning activeTuning = GetTuning();
            float elapsed = Time.unscaledTime - dodgeStartedAt;
            float duration = activeTuning.DodgeDurationSeconds;

            if (elapsed >= duration)
            {
                state = CombatDefenseState.Normal;
                lastDodgeEndedAt = Time.unscaledTime;
                if (!dodgeEndingInvoked)
                {
                    dodgeEndingInvoked = true;
                    onDodgeEnded?.Invoke();
                }
                return;
            }

            float speed = activeTuning.DodgeDistance / Mathf.Max(0.05f, duration);
            Vector3 motion = dodgeDirection * speed * Time.deltaTime;

            if (characterController != null && characterController.enabled)
            {
                characterController.Move(motion);
            }
            else if (rigidbodyTarget != null && !rigidbodyTarget.isKinematic)
            {
                rigidbodyTarget.MovePosition(rigidbodyTarget.position + motion);
            }
            else
            {
                transform.position += motion;
            }
        }

        private CombatStaminaTuning GetTuning()
        {
            if (tuning != null)
            {
                return tuning;
            }

            if (stamina != null && stamina.Tuning != null)
            {
                return stamina.Tuning;
            }

            if (runtimeFallbackTuning == null)
            {
                runtimeFallbackTuning = ScriptableObject.CreateInstance<CombatStaminaTuning>();
            }

            return runtimeFallbackTuning;
        }
    }
}
