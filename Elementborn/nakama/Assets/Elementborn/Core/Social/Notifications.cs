using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    /// <summary>What a notification is about. Covers this phase plus the kinds the later phases will post.</summary>
    public enum NotificationKind { System, Feedback, Message, FriendRequest, Invite, Moderation }

    /// <summary>One delivered notification. <see cref="Read"/> is mutated only through the store/center.</summary>
    public sealed class Notification
    {
        public string Id { get; }
        public NotificationKind Kind { get; }
        public string Title { get; }
        public string Body { get; }
        public string FromUserId { get; }   // null for system-originated
        public string ToUserId { get; }
        public long CreatedAtUtcTicks { get; }
        public bool Read { get; internal set; }

        /// <summary>Marks this notification read. Usable by any backend store (the setter is internal).</summary>
        public void MarkRead() { Read = true; }

        public Notification(string id, NotificationKind kind, string title, string body,
                            string toUserId, string fromUserId, long createdAtUtcTicks)
        {
            Id = id; Kind = kind; Title = title; Body = body;
            ToUserId = toUserId; FromUserId = fromUserId; CreatedAtUtcTicks = createdAtUtcTicks;
            Read = false;
        }
    }

    /// <summary>The seam over notification storage/transport. <see cref="InMemoryNotificationStore"/> is the
    /// offline implementation; a Nakama adapter implements this against server-side notifications.</summary>
    public interface INotificationStore
    {
        void Add(Notification notification);
        IReadOnlyList<Notification> ForUser(string userId);
        void MarkRead(string notificationId);
        int UnreadCount(string userId);
    }

    public sealed class InMemoryNotificationStore : INotificationStore
    {
        private readonly List<Notification> _all = new List<Notification>();

        public void Add(Notification notification) => _all.Add(notification);
        public IReadOnlyList<Notification> ForUser(string userId) => _all.FindAll(n => n.ToUserId == userId);

        public void MarkRead(string notificationId)
        {
            var n = _all.Find(x => x.Id == notificationId);
            if (n != null) n.Read = true;
        }

        public int UnreadCount(string userId) => _all.FindAll(n => n.ToUserId == userId && !n.Read).Count;
    }

    /// <summary>The app-facing notification API: post to a user, read inboxes, track unread, mark read. Raises
    /// <see cref="Posted"/> so UI (a HUD badge/panel) can react. Storage is delegated to an
    /// <see cref="INotificationStore"/> seam, so swapping local↔backend doesn't touch callers.</summary>
    public sealed class NotificationCenter
    {
        private readonly INotificationStore _store;

        /// <summary>Fired whenever a notification is posted (any recipient); UI filters by current user.</summary>
        public event System.Action<Notification> Posted;

        public NotificationCenter(INotificationStore store)
        {
            _store = store ?? new InMemoryNotificationStore();
        }

        public Notification Post(NotificationKind kind, string title, string body, string toUserId, string fromUserId = null)
        {
            var n = new Notification(System.Guid.NewGuid().ToString("N"), kind, title, body,
                                     toUserId, fromUserId, System.DateTime.UtcNow.Ticks);
            _store.Add(n);
            Posted?.Invoke(n);
            return n;
        }

        public IReadOnlyList<Notification> InboxFor(string userId) => _store.ForUser(userId);
        public int UnreadCountFor(string userId) => _store.UnreadCount(userId);
        public void MarkRead(string notificationId) => _store.MarkRead(notificationId);
    }
}
