using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal prompt UI. Works with legacy Unity UI Text/Image.
    /// If you later use TextMeshPro, either swap the Text fields or create a TMP version.
    /// </summary>
    public sealed class InteractablePromptView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text actionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image holdFillImage;

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Show(InteractionPromptData prompt)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = false;
            }

            if (titleText != null)
            {
                titleText.text = prompt.Title;
            }

            if (actionText != null)
            {
                actionText.text = prompt.RequiresHold
                    ? $"Hold E - {prompt.ActionText}"
                    : $"E - {prompt.ActionText}";
            }

            if (iconImage != null)
            {
                iconImage.sprite = prompt.Icon;
                iconImage.enabled = prompt.Icon != null;
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            SetHoldProgress(0f);
        }

        public void SetHoldProgress(float progress)
        {
            if (holdFillImage != null)
            {
                holdFillImage.fillAmount = Mathf.Clamp01(progress);
            }
        }
    }
}
