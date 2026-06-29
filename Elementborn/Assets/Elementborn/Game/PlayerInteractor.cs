using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Canonical v50 player interaction detector.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private Transform sourceTransform;
        [SerializeField] private float range = 3f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField] private bool useRaycast = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Prompt")]
        [SerializeField] private InteractablePromptView promptView;

        private IInteractable current;
        private GameObject currentObject;
        private float holdTimer;

        private void Reset()
        {
            sourceCamera = Camera.main;
            sourceTransform = sourceCamera != null ? sourceCamera.transform : transform;
        }

        private void Awake()
        {
            if (sourceCamera == null)
            {
                sourceCamera = Camera.main;
            }

            if (sourceTransform == null)
            {
                sourceTransform = sourceCamera != null ? sourceCamera.transform : transform;
            }
        }

        private void Update()
        {
            RefreshCurrentInteractable();
            RefreshPrompt();
            HandleInput();
        }

        private void RefreshCurrentInteractable()
        {
            current = null;
            currentObject = null;

            if (useRaycast)
            {
                Vector3 origin = sourceTransform != null ? sourceTransform.position : transform.position;
                Vector3 direction = sourceTransform != null ? sourceTransform.forward : transform.forward;

                if (Physics.Raycast(origin, direction, out RaycastHit hit, range, interactableLayers, QueryTriggerInteraction.Collide))
                {
                    TrySetCurrent(hit.collider);
                }

                return;
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, range, interactableLayers, QueryTriggerInteraction.Collide);
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit == null)
                {
                    continue;
                }

                IInteractable candidate = hit.GetComponentInParent<IInteractable>();
                if (candidate == null || !InteractableCompatibility.CanInteract(candidate, gameObject))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    current = candidate;
                    currentObject = hit.gameObject;
                }
            }
        }

        private void TrySetCurrent(Collider col)
        {
            IInteractable candidate = col != null ? col.GetComponentInParent<IInteractable>() : null;
            if (candidate == null || !InteractableCompatibility.CanInteract(candidate, gameObject))
            {
                return;
            }

            current = candidate;
            currentObject = col.gameObject;
        }

        private void RefreshPrompt()
        {
            if (promptView == null)
            {
                return;
            }

            if (current == null)
            {
                promptView.Hide();
                return;
            }

            promptView.Show(InteractableCompatibility.GetPrompt(current, gameObject));
        }

        private void HandleInput()
        {
            if (current == null)
            {
                holdTimer = 0f;
                return;
            }

            InteractionPromptData prompt = InteractableCompatibility.GetPrompt(current, gameObject);

            if (!prompt.RequiresHold)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    InteractableCompatibility.Interact(current, gameObject);
                }

                holdTimer = 0f;
                return;
            }

            if (Input.GetKey(interactKey))
            {
                holdTimer += Time.deltaTime;
                promptView?.SetHoldProgress(Mathf.Clamp01(holdTimer / Mathf.Max(0.01f, prompt.HoldSeconds)));

                if (holdTimer >= prompt.HoldSeconds)
                {
                    InteractableCompatibility.Interact(current, gameObject);
                    holdTimer = 0f;
                }
            }
            else
            {
                holdTimer = 0f;
                promptView?.SetHoldProgress(0f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (useRaycast)
            {
                Transform src = sourceTransform != null ? sourceTransform : transform;
                Gizmos.DrawLine(src.position, src.position + src.forward * range);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, range);
            }
        }
    }
}
