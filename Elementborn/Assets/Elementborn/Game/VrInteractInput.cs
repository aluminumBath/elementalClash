using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal VR/editor interaction bridge. Does not depend on XR packages, so it compiles in desktop-only projects.
    /// Wire TriggerInteract from XR input events later; keyboard fallback helps editor testing.
    /// </summary>
    public sealed class VrInteractInput : MonoBehaviour
    {
        [SerializeField] private Transform interactorTransform;
        [SerializeField] private KeyCode editorFallbackKey = KeyCode.E;
        [SerializeField] private bool enableEditorKeyboardFallback = true;

        private void Update()
        {
            if (enableEditorKeyboardFallback && Input.GetKeyDown(editorFallbackKey))
            {
                TriggerInteract();
            }
        }

        public void TriggerInteract()
        {
            if (interactorTransform != null)
            {
                InteractionArbiter.SignalInteract(interactorTransform);
                return;
            }

            InteractionArbiter.SignalInteract(transform);
        }

        public void SetInteractorTransform(Transform value)
        {
            interactorTransform = value;
        }
    }
}
