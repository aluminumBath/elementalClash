using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    public enum InviteStatus { Pending, Accepted, Declined, Cancelled }

    /// <summary>An invitation to join a host's game session. <see cref="Status"/> is mutated through the service.</summary>
    public sealed class GameInvite
    {
        public string Id { get; }
        public string FromUserId { get; }
        public string ToUserId { get; }
        public string SessionId { get; }
        public long CreatedAtUtcTicks { get; }
        public InviteStatus Status { get; internal set; }

        public GameInvite(string id, string fromUserId, string toUserId, string sessionId, long createdAtUtcTicks)
        {
            Id = id; FromUserId = fromUserId; ToUserId = toUserId; SessionId = sessionId;
            CreatedAtUtcTicks = createdAtUtcTicks; Status = InviteStatus.Pending;
        }
    }

    public interface IInviteStore
    {
        void Save(GameInvite invite);
        GameInvite Get(string inviteId);
        IReadOnlyList<GameInvite> PendingFor(string userId);
    }

    public sealed class InMemoryInviteStore : IInviteStore
    {
        private readonly List<GameInvite> _invites = new List<GameInvite>();
        public void Save(GameInvite invite) { if (Get(invite.Id) == null) _invites.Add(invite); }
        public GameInvite Get(string inviteId) => _invites.Find(i => i.Id == inviteId);
        public IReadOnlyList<GameInvite> PendingFor(string userId) =>
            _invites.FindAll(i => i.ToUserId == userId && i.Status == InviteStatus.Pending);
    }

    /// <summary>Invite a friend to your session. Inviting requires friendship; every transition posts a
    /// notification. <see cref="Accept"/> returns the session id to join — the actual join is the netcode
    /// layer's job (a Nakama party/match), wired off the controller's event.</summary>
    public sealed class InviteService
    {
        private readonly IInviteStore _store;
        private readonly NotificationCenter _notifications;
        private readonly IUserDirectory _directory;
        private readonly IFriendGraph _friends;

        public InviteService(IInviteStore store, NotificationCenter notifications, IUserDirectory directory, IFriendGraph friends)
        {
            _store = store; _notifications = notifications; _directory = directory; _friends = friends;
        }

        private string Name(string id) => _directory.TryGet(id, out var u) ? u.DisplayName : "Someone";

        public GameInvite Invite(string fromId, string toId, string sessionId)
        {
            if (fromId == toId || !_friends.AreFriends(fromId, toId)) return null; // only friends
            var invite = new GameInvite(System.Guid.NewGuid().ToString("N"), fromId, toId, sessionId, System.DateTime.UtcNow.Ticks);
            _store.Save(invite);
            _notifications.Post(NotificationKind.Invite, "Game invite", Name(fromId) + " invited you to play.", toId, fromId);
            return invite;
        }

        /// <summary>Accept an invite. Returns the session id to join, or null if it's not a valid pending invite
        /// for this user. Notifies the host.</summary>
        public string Accept(string inviteId, string userId)
        {
            var inv = _store.Get(inviteId);
            if (inv == null || inv.ToUserId != userId || inv.Status != InviteStatus.Pending) return null;
            inv.Status = InviteStatus.Accepted;
            _notifications.Post(NotificationKind.Invite, "Invite accepted", Name(userId) + " is joining your game.", inv.FromUserId, userId);
            return inv.SessionId;
        }

        public bool Decline(string inviteId, string userId)
        {
            var inv = _store.Get(inviteId);
            if (inv == null || inv.ToUserId != userId || inv.Status != InviteStatus.Pending) return false;
            inv.Status = InviteStatus.Declined;
            _notifications.Post(NotificationKind.Invite, "Invite declined", Name(userId) + " declined your invite.", inv.FromUserId, userId);
            return true;
        }

        public bool Cancel(string inviteId, string fromId)
        {
            var inv = _store.Get(inviteId);
            if (inv == null || inv.FromUserId != fromId || inv.Status != InviteStatus.Pending) return false;
            inv.Status = InviteStatus.Cancelled;
            return true;
        }

        public IReadOnlyList<GameInvite> PendingFor(string userId) => _store.PendingFor(userId);
    }
}
