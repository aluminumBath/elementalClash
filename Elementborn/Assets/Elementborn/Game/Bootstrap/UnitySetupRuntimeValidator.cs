using UnityEngine;
using UnityEngine.EventSystems;

namespace Elementborn.Game
{
    public sealed class UnitySetupRuntimeValidator : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool warnOnly = true;

        private void Start()
        {
            if (runOnStart)
            {
                ValidateScene();
            }
        }

        [ContextMenu("Validate Scene")]
        public void ValidateScene()
        {
            if (GameObject.FindGameObjectWithTag("Player") == null)
            {
                Debug.LogWarning("Elementborn setup: no GameObject tagged Player was found.");
            }

            if (ElementbornFindUtility.FindFirst<EventSystem>() == null)
            {
                Debug.LogWarning("Elementborn setup: no EventSystem found. UI input may not work.");
            }

            if (ElementbornFindUtility.FindFirst<Canvas>() == null)
            {
                Debug.LogWarning("Elementborn setup: no Canvas found. HUD/UI views will not display.");
            }

            if (ElementbornFindUtility.FindFirst<ElementbornRuntimeBootstrap>() == null)
            {
                Debug.LogWarning("Elementborn setup: no ElementbornRuntimeBootstrap found.");
            }

            if (ElementbornFindUtility.FindFirst<QuestUiTracker>() == null)
            {
                Debug.LogWarning("Elementborn setup: no QuestUiTracker found. Quest UI will auto-create only if code calls Ensure().");
            }
        }
    }
}
