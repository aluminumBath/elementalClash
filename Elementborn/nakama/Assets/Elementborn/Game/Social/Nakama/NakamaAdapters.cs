#if ELEMENTBORN_NAKAMA
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social.NakamaNet
{
    /// <summary>Binds the seven Core seams to Nakama-backed adapters built from one connection.</summary>
    public sealed class NakamaSocialBackend : ISocialBackend
    {
        public IUserDirectory Directory { get; }
        public INotificationStore Notifications { get; }
        public IFeedbackStore Feedback { get; }
        public IFriendGraph Friends { get; }
        public IInviteStore Invites { get; }
        public IMessageTransport Messages { get; }
        public IModerationStore Moderation { get; }

        public NakamaSocialBackend(NakamaConnection c)
        {
            Directory = new NakamaUserDirectory(c);
            Notifications = new NakamaNotificationStore(c);
            Feedback = new NakamaStorageFeedbackStore(c);
            Friends = new NakamaFriendGraph(c);
            Invites = new NakamaStorageInviteStore(c);
            Messages = new NakamaChatTransport(c);
            Moderation = new NakamaStorageModerationStore(c);
        }
    }

    // ====================================================================================================
    // The adapters bridge async Nakama to the synchronous Core seams by caching: writes are fire-and-forget
    // (eventually consistent) and reads return a cache kept fresh by socket events and an initial fetch. Where
    // a guarantee needs server authority (cross-user notifications, ban enforcement), that is called out and
    // belongs in a Nakama server module / RPC rather than the client.
    // ====================================================================================================

    /// <summary>Users via Nakama accounts. App roles are cached locally — roles are otherwise a server concern
    /// (e.g. group membership or a storage record checked by an RPC).</summary>
    public sealed class NakamaUserDirectory : IUserDirectory
    {
        private readonly NakamaConnection _c;
        private readonly Dictionary<string, UserRef> _cache = new Dictionary<string, UserRef>();

        public NakamaUserDirectory(NakamaConnection c) { _c = c; }

        public void Register(UserRef user) { _cache[user.Id] = user; }

        public bool TryGet(string userId, out UserRef user)
        {
            if (_cache.TryGetValue(userId, out user)) return true;
            user = default;
            _ = FetchAsync(userId);
            return false;
        }

        private async Task FetchAsync(string userId)
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                var users = await _c.Client.GetUsersAsync(_c.Session, new[] { userId });
                foreach (var u in users.Users)
                {
                    string name = string.IsNullOrEmpty(u.DisplayName) ? u.Username : u.DisplayName;
                    _cache[u.Id] = new UserRef(u.Id, name, UserRole.Player);
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] GetUsers failed: " + e.Message); }
        }

        public void SetRole(string userId, UserRole role)
        {
            _cache[userId] = _cache.TryGetValue(userId, out var u) ? u.WithRole(role) : new UserRef(userId, userId, role);
        }

        public IReadOnlyList<UserRef> All() => new List<UserRef>(_cache.Values);

        public IReadOnlyList<UserRef> Admins()
        {
            var list = new List<UserRef>();
            foreach (var u in _cache.Values) if (u.IsAdmin) list.Add(u);
            return list;
        }
    }

    /// <summary>Notifications via Nakama's notification system: live ones arrive on the socket, history is
    /// fetched once. NOTE: delivering a notification to <em>another</em> user is a server concern (a module /
    /// RPC); client-side <see cref="Add"/> only surfaces locally-originated ones.</summary>
    public sealed class NakamaNotificationStore : INotificationStore
    {
        private readonly NakamaConnection _c;
        private readonly List<Notification> _cache = new List<Notification>();

        public NakamaNotificationStore(NakamaConnection c)
        {
            _c = c;
            if (_c != null && _c.Socket != null) _c.Socket.ReceivedNotification += OnReceived;
            _ = LoadAsync();
        }

        private void OnReceived(IApiNotification n)
        {
            _cache.Add(new Notification(n.Id, NotificationKind.System, n.Subject, n.Content, n.SenderId, _c.UserId));
        }

        private async Task LoadAsync()
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                var list = await _c.Client.ListNotificationsAsync(_c.Session, 100);
                foreach (var n in list.Notifications)
                    _cache.Add(new Notification(n.Id, NotificationKind.System, n.Subject, n.Content, n.SenderId, _c.UserId));
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] ListNotifications failed: " + e.Message); }
        }

        public void Add(Notification notification) { _cache.Add(notification); }

        public IReadOnlyList<Notification> ForUser(string userId)
        {
            var list = new List<Notification>();
            foreach (var n in _cache) if (n.ToUserId == userId) list.Add(n);
            return list;
        }

        public void MarkRead(string notificationId)
        {
            foreach (var n in _cache) if (n.Id == notificationId) n.MarkRead();
        }

        public int UnreadCount(string userId)
        {
            int count = 0;
            foreach (var n in _cache) if (n.ToUserId == userId && !n.Read) count++;
            return count;
        }
    }

    /// <summary>Feedback persisted to a Nakama storage collection ("feedback").</summary>
    public sealed class NakamaStorageFeedbackStore : IFeedbackStore
    {
        private const string Collection = "feedback";
        private readonly NakamaConnection _c;
        private readonly List<FeedbackReport> _cache = new List<FeedbackReport>();

        public NakamaStorageFeedbackStore(NakamaConnection c) { _c = c; _ = LoadAsync(); }

        public void Save(FeedbackReport report) { _cache.Add(report); _ = WriteAsync(report); }

        private async Task WriteAsync(FeedbackReport r)
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                string json = JsonUtility.ToJson(FeedbackDto.From(r));
                await _c.Client.WriteStorageObjectsAsync(_c.Session, new IApiWriteStorageObject[]
                {
                    new WriteStorageObject { Collection = Collection, Key = r.Id, Value = json, PermissionRead = 2, PermissionWrite = 1 }
                });
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] feedback write failed: " + e.Message); }
        }

        private async Task LoadAsync()
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                var page = await _c.Client.ListStorageObjectsAsync(_c.Session, Collection, 100, null);
                foreach (var o in page.Objects)
                {
                    var dto = JsonUtility.FromJson<FeedbackDto>(o.Value);
                    if (dto != null) _cache.Add(dto.ToReport());
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] feedback list failed: " + e.Message); }
        }

        public IReadOnlyList<FeedbackReport> All() => new List<FeedbackReport>(_cache);
    }

    /// <summary>Friends via Nakama's friend system. The lists are cached from ListFriends (refreshed on demand);
    /// writes are fire-and-forget. Nakama friend state: 0 = friend, 1 = invite sent, 2 = invite received.</summary>
    public sealed class NakamaFriendGraph : IFriendGraph
    {
        private readonly NakamaConnection _c;
        private readonly List<string> _friends = new List<string>();
        private readonly List<string> _incoming = new List<string>();
        private readonly List<string> _outgoing = new List<string>();

        public NakamaFriendGraph(NakamaConnection c) { _c = c; _ = RefreshAsync(); }

        private async Task RefreshAsync()
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                _friends.Clear(); _incoming.Clear(); _outgoing.Clear();
                var list = await _c.Client.ListFriendsAsync(_c.Session, null, 100, null);
                foreach (var f in list.Friends)
                {
                    if (f.State == 0) _friends.Add(f.User.Id);
                    else if (f.State == 1) _outgoing.Add(f.User.Id);
                    else if (f.State == 2) _incoming.Add(f.User.Id);
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] ListFriends failed: " + e.Message); }
        }

        public FriendStatus SendRequest(string fromId, string toId)
        {
            _ = AddAndRefresh(toId);
            return FriendStatus.PendingOutgoing;
        }

        public bool Accept(string userId, string requesterId) { _ = AddAndRefresh(requesterId); return true; }
        public bool Decline(string userId, string requesterId) { _ = DeleteAndRefresh(requesterId); return true; }
        public bool Remove(string userId, string otherId) { _ = DeleteAndRefresh(otherId); return true; }

        private async Task AddAndRefresh(string id)
        {
            if (_c == null || !_c.Connected) return;
            try { await _c.Client.AddFriendsAsync(_c.Session, new[] { id }, new string[0]); }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] AddFriends failed: " + e.Message); }
            await RefreshAsync();
        }

        private async Task DeleteAndRefresh(string id)
        {
            if (_c == null || !_c.Connected) return;
            try { await _c.Client.DeleteFriendsAsync(_c.Session, new[] { id }, new string[0]); }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] DeleteFriends failed: " + e.Message); }
            await RefreshAsync();
        }

        public bool AreFriends(string a, string b) => _friends.Contains(b);
        public IReadOnlyList<string> FriendsOf(string userId) => new List<string>(_friends);
        public IReadOnlyList<string> PendingIncoming(string userId) => new List<string>(_incoming);
        public IReadOnlyList<string> PendingOutgoing(string userId) => new List<string>(_outgoing);

        public FriendStatus StatusBetween(string viewerId, string otherId)
        {
            if (_friends.Contains(otherId)) return FriendStatus.Friends;
            if (_outgoing.Contains(otherId)) return FriendStatus.PendingOutgoing;
            if (_incoming.Contains(otherId)) return FriendStatus.PendingIncoming;
            return FriendStatus.None;
        }
    }

    /// <summary>Invites kept in a Nakama storage collection ("invites"). NOTE: for real-time invites and joins,
    /// Nakama <b>parties</b> (socket.ReceivedPartyInvite + JoinPartyAsync) are the better fit — this storage shim
    /// keeps the seam working; swap to parties when wiring the netcode join.</summary>
    public sealed class NakamaStorageInviteStore : IInviteStore
    {
        private const string Collection = "invites";
        private readonly NakamaConnection _c;
        private readonly Dictionary<string, GameInvite> _cache = new Dictionary<string, GameInvite>();

        public NakamaStorageInviteStore(NakamaConnection c) { _c = c; _ = LoadAsync(); }

        public void Save(GameInvite invite) { _cache[invite.Id] = invite; _ = WriteAsync(invite); }

        private async Task WriteAsync(GameInvite i)
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                string json = JsonUtility.ToJson(InviteDto.From(i));
                await _c.Client.WriteStorageObjectsAsync(_c.Session, new IApiWriteStorageObject[]
                {
                    new WriteStorageObject { Collection = Collection, Key = i.Id, Value = json, PermissionRead = 2, PermissionWrite = 1 }
                });
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] invite write failed: " + e.Message); }
        }

        private async Task LoadAsync()
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                var page = await _c.Client.ListStorageObjectsAsync(_c.Session, Collection, 100, null);
                foreach (var o in page.Objects)
                {
                    var dto = JsonUtility.FromJson<InviteDto>(o.Value);
                    if (dto != null) _cache[dto.id] = dto.ToInvite();
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] invite list failed: " + e.Message); }
        }

        public GameInvite Get(string inviteId) => _cache.TryGetValue(inviteId, out var i) ? i : null;

        public IReadOnlyList<GameInvite> PendingFor(string userId)
        {
            var list = new List<GameInvite>();
            foreach (var i in _cache.Values)
                if (i.ToUserId == userId && i.Status == InviteStatus.Pending) list.Add(i);
            return list;
        }
    }

    /// <summary>Direct/session messaging over Nakama chat. Our channel id (a canonical DM pair, or a session)
    /// is used as the Nakama room name; the server echoes our own sends, so we cache and raise only on receive
    /// (no local loopback, to avoid duplicates).</summary>
    public sealed class NakamaChatTransport : IMessageTransport
    {
        private readonly NakamaConnection _c;
        private readonly Dictionary<string, string> _nakamaByOurs = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _oursByNakama = new Dictionary<string, string>();
        private readonly Dictionary<string, List<ChatMessage>> _history = new Dictionary<string, List<ChatMessage>>();

        public event System.Action<ChatMessage> Received;

        public NakamaChatTransport(NakamaConnection c)
        {
            _c = c;
            if (_c != null && _c.Socket != null) _c.Socket.ReceivedChannelMessage += OnChannelMessage;
        }

        public void Send(ChatMessage message) { _ = SendAsync(message); }

        private async Task SendAsync(ChatMessage m)
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                string nakamaId = await EnsureChannelAsync(m.ChannelId);
                if (nakamaId == null) return;
                string content = JsonUtility.ToJson(new ChatContentDto { text = m.Text });
                await _c.Socket.WriteChatMessageAsync(nakamaId, content);
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] chat send failed: " + e.Message); }
        }

        private async Task<string> EnsureChannelAsync(string ourChannelId)
        {
            if (_nakamaByOurs.TryGetValue(ourChannelId, out var existing)) return existing;
            var channel = await _c.Socket.JoinChatAsync(ourChannelId, ChannelType.Room, true, false);
            _nakamaByOurs[ourChannelId] = channel.Id;
            _oursByNakama[channel.Id] = ourChannelId;
            return channel.Id;
        }

        private void OnChannelMessage(IApiChannelMessage m)
        {
            if (!_oursByNakama.TryGetValue(m.ChannelId, out var ourId)) return;
            string text = m.Content;
            var dto = JsonUtility.FromJson<ChatContentDto>(m.Content);
            if (dto != null && dto.text != null) text = dto.text;
            var msg = new ChatMessage(m.MessageId, ourId, m.SenderId, text, System.DateTime.UtcNow.Ticks);
            if (!_history.TryGetValue(ourId, out var list)) { list = new List<ChatMessage>(); _history[ourId] = list; }
            list.Add(msg);
            Received?.Invoke(msg);
        }

        public IReadOnlyList<ChatMessage> History(string channelId, int max = 50)
        {
            if (!_nakamaByOurs.ContainsKey(channelId)) _ = EnsureChannelAsync(channelId); // join lazily for next time
            if (!_history.TryGetValue(channelId, out var list)) return new List<ChatMessage>();
            int start = list.Count > max ? list.Count - max : 0;
            return list.GetRange(start, list.Count - start);
        }
    }

    /// <summary>Bans kept in a Nakama storage collection ("bans"). NOTE: this is an advisory client cache —
    /// authoritative enforcement (refusing a banned user's match join) belongs in a Nakama before-hook/RPC.</summary>
    public sealed class NakamaStorageModerationStore : IModerationStore
    {
        private const string Collection = "bans";
        private readonly NakamaConnection _c;
        private readonly List<BanRecord> _cache = new List<BanRecord>();

        public NakamaStorageModerationStore(NakamaConnection c) { _c = c; _ = LoadAsync(); }

        public void Add(BanRecord ban)
        {
            _cache.Add(ban);
            _ = WriteAsync(ban);
        }

        private static string KeyFor(BanRecord b) =>
            (b.Scope == BanScope.Global ? "g" : "s_" + b.SessionId) + "_" + b.UserId;

        private async Task WriteAsync(BanRecord b)
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                string json = JsonUtility.ToJson(BanDto.From(b));
                await _c.Client.WriteStorageObjectsAsync(_c.Session, new IApiWriteStorageObject[]
                {
                    new WriteStorageObject { Collection = Collection, Key = KeyFor(b), Value = json, PermissionRead = 2, PermissionWrite = 1 }
                });
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] ban write failed: " + e.Message); }
        }

        private async Task LoadAsync()
        {
            if (_c == null || !_c.Connected) return;
            try
            {
                var page = await _c.Client.ListStorageObjectsAsync(_c.Session, Collection, 200, null);
                foreach (var o in page.Objects)
                {
                    var dto = JsonUtility.FromJson<BanDto>(o.Value);
                    if (dto != null) _cache.Add(dto.ToRecord());
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] ban list failed: " + e.Message); }
        }

        public bool Remove(string userId, BanScope scope, string sessionId)
        {
            int removed = _cache.RemoveAll(b => b.UserId == userId && b.Scope == scope &&
                                                (scope == BanScope.Global || b.SessionId == sessionId));
            return removed > 0;
        }

        public bool IsBannedGlobally(string userId) =>
            _cache.Exists(b => b.Scope == BanScope.Global && b.UserId == userId);

        public bool IsBannedFromSession(string userId, string sessionId) =>
            _cache.Exists(b => b.Scope == BanScope.Session && b.UserId == userId && b.SessionId == sessionId);

        public IReadOnlyList<BanRecord> All() => _cache;
    }

    // ---- serialization DTOs (JsonUtility) ----------------------------------------------------------------

    [System.Serializable]
    public sealed class ChatContentDto { public string text; }

    [System.Serializable]
    public sealed class FeedbackDto
    {
        public string id; public int kind; public string title; public string body; public string fromUserId; public long createdTicks;
        public static FeedbackDto From(FeedbackReport r) => new FeedbackDto
        { id = r.Id, kind = (int)r.Kind, title = r.Title, body = r.Body, fromUserId = r.FromUserId, createdTicks = r.CreatedAtUtcTicks };
        public FeedbackReport ToReport() => new FeedbackReport(id, (FeedbackKind)kind, title, body, fromUserId, createdTicks);
    }

    [System.Serializable]
    public sealed class InviteDto
    {
        public string id; public string fromUserId; public string toUserId; public string sessionId; public long createdTicks;
        public static InviteDto From(GameInvite i) => new InviteDto
        { id = i.Id, fromUserId = i.FromUserId, toUserId = i.ToUserId, sessionId = i.SessionId, createdTicks = i.CreatedAtUtcTicks };
        public GameInvite ToInvite() => new GameInvite(id, fromUserId, toUserId, sessionId, createdTicks);
    }

    [System.Serializable]
    public sealed class BanDto
    {
        public string userId; public int scope; public string sessionId; public string byUserId; public string reason; public long createdTicks;
        public static BanDto From(BanRecord b) => new BanDto
        { userId = b.UserId, scope = (int)b.Scope, sessionId = b.SessionId, byUserId = b.ByUserId, reason = b.Reason, createdTicks = b.CreatedAtUtcTicks };
        public BanRecord ToRecord() => new BanRecord(userId, (BanScope)scope, sessionId, byUserId, reason, createdTicks);
    }
}
#endif
