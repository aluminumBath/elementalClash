using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// Surfaces the current user's notifications for a HUD badge / panel. Raises <see cref="Changed"/> when a new
    /// one arrives for this user or one is marked read, so UI can refresh an unread count. The panel itself is a
    /// follow-up; this is the wiring onto <see cref="SocialHub"/>.
    /// </summary>
    public sealed class NotificationController : MonoBehaviour
    {
        public event System.Action Changed;

        private void OnEnable()
        {
            if (SocialHub.Instance != null) SocialHub.Instance.Notifications.Posted += OnPosted;
        }

        private void OnDisable()
        {
            if (SocialHub.Instance != null) SocialHub.Instance.Notifications.Posted -= OnPosted;
        }

        private void OnPosted(Notification n)
        {
            var hub = SocialHub.Instance;
            if (hub != null && n.ToUserId == hub.CurrentUser.Id) Changed?.Invoke();
        }

        public int UnreadCount
        {
            get
            {
                var hub = SocialHub.Instance;
                return hub != null ? hub.Notifications.UnreadCountFor(hub.CurrentUser.Id) : 0;
            }
        }

        public IReadOnlyList<Notification> Inbox
        {
            get
            {
                var hub = SocialHub.Instance;
                return hub != null ? hub.Notifications.InboxFor(hub.CurrentUser.Id) : new List<Notification>();
            }
        }

        public void MarkRead(string notificationId)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return;
            hub.Notifications.MarkRead(notificationId);
            Changed?.Invoke();
        }
    }
}
