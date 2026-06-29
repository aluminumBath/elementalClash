using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Optional scene diagnostic component. It does not prevent every null reference, but it reports
    /// the most common missing scene objects before play-test interactions happen.
    /// </summary>
    public sealed class RuntimeNullReferenceGuard : MonoBehaviour
    {
        [SerializeField] private bool checkOnStart = true;
        [SerializeField] private bool warnIfNoPlayer = true;
        [SerializeField] private bool warnIfNoCamera = true;
        [SerializeField] private bool warnIfNoCanvas = true;
        [SerializeField] private bool warnIfNoEventSystem = true;
        [SerializeField] private bool warnIfNoRuntimeBootstrap = true;

        private void Start()
        {
            if (checkOnStart)
            {
                RunChecks();
            }
        }

        [ContextMenu("Run Null Guard Checks")]
        public void RunChecks()
        {
            if (warnIfNoPlayer && GameObject.FindGameObjectWithTag("Player") == null)
            {
                Debug.LogWarning("[Elementborn] No active object with Player tag found.");
            }

            if (warnIfNoCamera && Camera.main == null && ElementbornFindUtility.FindFirst<Camera>() == null)
            {
                Debug.LogWarning("[Elementborn] No camera found.");
            }

            if (warnIfNoCanvas && ElementbornFindUtility.FindFirst<Canvas>() == null)
            {
                Debug.LogWarning("[Elementborn] No UI Canvas found.");
            }

            if (warnIfNoEventSystem && ElementbornFindUtility.FindFirst<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Debug.LogWarning("[Elementborn] No EventSystem found.");
            }

            if (warnIfNoRuntimeBootstrap && ElementbornFindUtility.FindFirst<ElementbornRuntimeBootstrap>() == null)
            {
                Debug.LogWarning("[Elementborn] No ElementbornRuntimeBootstrap found.");
            }
        }
    }
}
