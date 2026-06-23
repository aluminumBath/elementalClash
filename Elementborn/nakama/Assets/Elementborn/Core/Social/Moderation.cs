using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    public enum BanScope { Global, Session }

    /// <summary>A ban: a user barred globally (by an admin) or from one session (by a session-admin/admin).</summary>
    public sealed class BanRecord
    {
        public string UserId { get; }
        public BanScope Scope { get; }
        public string SessionId { get; }   // null for a global ban
        public string ByUserId { get; }
        public string Reason { get; }
        public long CreatedAtUtcTicks { get; }

        public BanRecord(string userId, BanScope scope, string sessionId, string byUserId, string reason, long createdAtUtcTicks)
        {
            UserId = userId; Scope = scope; SessionId = sessionId;
            ByUserId = byUserId; Reason = reason; CreatedAtUtcTicks = createdAtUtcTicks;
        }
    }

    /// <summary>The seam over ban records. <see cref="InMemoryModerationStore"/> is offline; a backend adapter
    /// persists bans and maps to Nakama group/permission checks.</summary>
    public interface IModerationStore
    {
        void Add(BanRecord ban);
        bool Remove(string userId, BanScope scope, string sessionId);
        bool IsBannedGlobally(string userId);
        bool IsBannedFromSession(string userId, string sessionId);
        IReadOnlyList<BanRecord> All();
    }

    public sealed class InMemoryModerationStore : IModerationStore
    {
        private readonly List<BanRecord> _bans = new List<BanRecord>();

        public void Add(BanRecord ban) => _bans.Add(ban);

        public bool Remove(string userId, BanScope scope, string sessionId) =>
            _bans.RemoveAll(b => b.UserId == userId && b.Scope == scope &&
                                 (scope == BanScope.Global || b.SessionId == sessionId)) > 0;

        public bool IsBannedGlobally(string userId) =>
            _bans.Exists(b => b.Scope == BanScope.Global && b.UserId == userId);

        public bool IsBannedFromSession(string userId, string sessionId) =>
            _bans.Exists(b => b.Scope == BanScope.Session && b.UserId == userId && b.SessionId == sessionId);

        public IReadOnlyList<BanRecord> All() => _bans;
    }

    /// <summary>
    /// Ban / allow users, with role enforcement: only an <see cref="UserRole.Admin"/> may ban globally; an admin
    /// or <see cref="UserRole.SessionAdmin"/> may ban from (or allow back into) a session. <see cref="CanJoin"/>
    /// is the check the join flow consults. Every action notifies the affected user.
    /// </summary>
    public sealed class ModerationService
    {
        private readonly IModerationStore _store;
        private readonly NotificationCenter _notifications;
        private readonly IUserDirectory _directory;

        public ModerationService(IModerationStore store, NotificationCenter notifications, IUserDirectory directory)
        {
            _store = store; _notifications = notifications; _directory = directory;
        }

        private bool IsAdmin(string id) => _directory.TryGet(id, out var u) && u.IsAdmin;
        private bool CanModerateSessions(string id) => _directory.TryGet(id, out var u) && u.CanModerateSessions;
        private static long Now => System.DateTime.UtcNow.Ticks;

        public bool BanGlobally(string adminId, string targetId, string reason)
        {
            if (!IsAdmin(adminId) || adminId == targetId) return false;
            _store.Add(new BanRecord(targetId, BanScope.Global, null, adminId, reason, Now));
            _notifications.Post(NotificationKind.Moderation, "Account banned",
                string.IsNullOrEmpty(reason) ? "You have been banned." : reason, targetId, adminId);
            return true;
        }

        public bool LiftGlobalBan(string adminId, string targetId)
        {
            if (!IsAdmin(adminId)) return false;
            return _store.Remove(targetId, BanScope.Global, null);
        }

        public bool BanFromSession(string moderatorId, string targetId, string sessionId, string reason)
        {
            if (!CanModerateSessions(moderatorId) || moderatorId == targetId || string.IsNullOrEmpty(sessionId)) return false;
            _store.Add(new BanRecord(targetId, BanScope.Session, sessionId, moderatorId, reason, Now));
            _notifications.Post(NotificationKind.Moderation, "Removed from session",
                string.IsNullOrEmpty(reason) ? "You were removed from a session." : reason, targetId, moderatorId);
            return true;
        }

        public bool AllowInSession(string moderatorId, string targetId, string sessionId)
        {
            if (!CanModerateSessions(moderatorId)) return false;
            return _store.Remove(targetId, BanScope.Session, sessionId);
        }

        /// <summary>The gate the join flow calls: false if the user is banned globally or from this session.</summary>
        public bool CanJoin(string userId, string sessionId) =>
            !_store.IsBannedGlobally(userId) && !_store.IsBannedFromSession(userId, sessionId);

        public bool IsBannedGlobally(string userId) => _store.IsBannedGlobally(userId);
        public bool IsBannedFromSession(string userId, string sessionId) => _store.IsBannedFromSession(userId, sessionId);
        public IReadOnlyList<BanRecord> Bans => _store.All();
    }
}
