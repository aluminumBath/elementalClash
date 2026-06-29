using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ElementbornPrototypePlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 7f;
        public float jumpSpeed = 5f;
        public float gravity = -18f;
        public float turnSpeed = 12f;

        [Header("Interaction")]
        public KeyCode interactKey = KeyCode.E;
        public float interactRange = 5.5f;
        public LayerMask interactionMask = ~0;

        private CharacterController controller;
        private Camera playCamera;
        private ElementbornPrototypePlayerStats stats;
        private float verticalVelocity;
        private ElementbornPrototypeInteractable currentInteractable;
        private ElementbornPrototypeInteractable lastPromptedInteractable;
        private float nextPromptTime;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stats = GetComponent<ElementbornPrototypePlayerStats>();

            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = Vector3.zero;
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 55f;
            controller.skinWidth = 0.04f;
        }

        private void Update()
        {
            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null && !manager.HasStarted)
            {
                return;
            }

            if (stats != null && stats.IsDead)
            {
                return;
            }

            playCamera = playCamera != null ? playCamera : Camera.main;

            Move();
            ScanForInteractable();

            if (Input.GetKeyDown(interactKey))
            {
                TryInteract();
            }
        }

        public void Teleport(Vector3 position)
        {
            bool wasEnabled = controller == null || controller.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }

            transform.position = position;

            if (controller != null)
            {
                controller.enabled = wasEnabled;
            }

            verticalVelocity = -0.5f;
        }

        private void Move()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Approximately(horizontal, 0f))
            {
                horizontal = KeyAxis(KeyCode.A, KeyCode.D) + KeyAxis(KeyCode.LeftArrow, KeyCode.RightArrow);
            }

            if (Mathf.Approximately(vertical, 0f))
            {
                vertical = KeyAxis(KeyCode.S, KeyCode.W) + KeyAxis(KeyCode.DownArrow, KeyCode.UpArrow);
            }

            Vector3 forward = playCamera != null ? playCamera.transform.forward : Vector3.forward;
            Vector3 right = playCamera != null ? playCamera.transform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 move = forward * vertical + right * horizontal;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            bool wantsSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool canSprint = wantsSprint && move.sqrMagnitude > 0.001f && (stats == null || stats.CanSprint());
            float speed = canSprint ? sprintSpeed : walkSpeed;

            if (canSprint && stats != null)
            {
                stats.ConsumeSprint(Time.deltaTime);
            }

            if (controller.isGrounded || transform.position.y <= 1.02f)
            {
                verticalVelocity = -0.5f;
                if (Input.GetKeyDown(KeyCode.Space) && (stats == null || stats.TrySpendStamina(stats.jumpCost)))
                {
                    verticalVelocity = jumpSpeed;
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = move * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            if (transform.position.y < 1f)
            {
                Teleport(new Vector3(transform.position.x, 1f, transform.position.z));
            }

            if (move.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(move, Vector3.up),
                    Time.deltaTime * turnSpeed);
            }
        }

        private float KeyAxis(KeyCode negative, KeyCode positive)
        {
            float value = 0f;
            if (Input.GetKey(negative))
            {
                value -= 1f;
            }

            if (Input.GetKey(positive))
            {
                value += 1f;
            }

            return value;
        }

        private void ScanForInteractable()
        {
            currentInteractable = FindBestInteractable();

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null && currentInteractable != null)
            {
                if (lastPromptedInteractable != currentInteractable || Time.time >= nextPromptTime)
                {
                    manager.ShowMessage("Press " + interactKey + ": " + currentInteractable.GetPrompt());
                    lastPromptedInteractable = currentInteractable;
                    nextPromptTime = Time.time + 1.25f;
                }
            }
        }

        private ElementbornPrototypeInteractable FindBestInteractable()
        {
            ElementbornPrototypeInteractable[] interactables =
                FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            ElementbornPrototypeInteractable best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable == null || !interactable.isActiveAndEnabled)
                {
                    continue;
                }

                float allowedRange = Mathf.Max(interactRange, interactable.activationRadius);
                float distance = Vector3.Distance(transform.position, interactable.transform.position);
                if (distance > allowedRange)
                {
                    continue;
                }

                Vector3 toTarget = interactable.transform.position - transform.position;
                toTarget.y = 0f;

                float facingBonus = 0f;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    Vector3 forward = playCamera != null ? playCamera.transform.forward : transform.forward;
                    forward.y = 0f;
                    forward.Normalize();
                    facingBonus = Mathf.Clamp01(Vector3.Dot(forward, toTarget.normalized));
                }

                float score = distance - facingBonus * 0.75f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = interactable;
                }
            }

            Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.up, interactRange, interactionMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null)
                {
                    continue;
                }

                ElementbornPrototypeInteractable interactable = hit.GetComponentInParent<ElementbornPrototypeInteractable>();
                if (interactable == null || !interactable.isActiveAndEnabled)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, interactable.transform.position);
                if (distance < bestScore)
                {
                    bestScore = distance;
                    best = interactable;
                }
            }

            return best;
        }

        private void TryInteract()
        {
            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;

            if (currentInteractable == null)
            {
                currentInteractable = FindBestInteractable();
            }

            if (currentInteractable != null)
            {
                currentInteractable.Interact();
                return;
            }

            if (manager != null)
            {
                manager.ShowMessage("Nothing to interact with nearby. Move closer to Ember Guide, the shard, or the pedestal.");
            }
        }
    }
}
