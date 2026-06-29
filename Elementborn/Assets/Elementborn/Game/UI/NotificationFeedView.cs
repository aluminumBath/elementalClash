using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal notification view. Assign a Text field and it will show the latest active notification.
    /// </summary>
    public sealed class NotificationFeedView : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private NotificationFeed feed;

        private void Reset()
        {
            text = GetComponentInChildren<Text>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (feed == null)
            {
                feed = NotificationFeed.Ensure();
            }
        }

        private void Update()
        {
            if (feed == null || feed.Notifications.Count == 0)
            {
                Hide();
                return;
            }

            var latest = feed.Notifications[feed.Notifications.Count - 1];
            if (text != null)
            {
                text.text = latest.Message;
            }

            Show();
        }

        private void Show()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}
