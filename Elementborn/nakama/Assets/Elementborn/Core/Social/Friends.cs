using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    /// <summary>The relationship between two users, from one user's point of view.</summary>
    public enum FriendStatus { None, PendingOutgoing, PendingIncoming, Friends }

    /// <summary>The seam over the friend graph. <see cref="LocalFriendGraph"/> is offline; a Nakama adapter
    /// implements this against Nakama's friends API.</summary>
    public interface IFriendGraph
    {
        FriendStatus SendRequest(string fromId, string toId);
        bool Accept(string userId, string requesterId);
        bool Decline(string userId, string requesterId);
        bool Remove(string userId, string otherId);
        bool AreFriends(string a, string b);
        IReadOnlyList<string> FriendsOf(string userId);
        IReadOnlyList<string> PendingIncoming(string userId);
        IReadOnlyList<string> PendingOutgoing(string userId);
        FriendStatus StatusBetween(string viewerId, string otherId);
    }

    /// <summary>In-memory friend graph. Friendships are symmetric; requests are directed. A request that meets a
    /// reciprocal pending request auto-accepts into a friendship.</summary>
    public sealed class LocalFriendGraph : IFriendGraph
    {
        private const char Sep = '\u0001';
        private readonly Dictionary<string, HashSet<string>> _friends = new Dictionary<string, HashSet<string>>();
        private readonly HashSet<string> _pending = new HashSet<string>(); // directed key "from\u0001to"

        private static string Key(string from, string to) => from + Sep + to;

        public FriendStatus SendRequest(string fromId, string toId)
        {
            if (string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(toId) || fromId == toId) return FriendStatus.None;
            if (AreFriends(fromId, toId)) return FriendStatus.Friends;

            if (_pending.Contains(Key(toId, fromId)))   // they already asked us -> become friends
            {
                _pending.Remove(Key(toId, fromId));
                _pending.Remove(Key(fromId, toId));
                AddFriendship(fromId, toId);
                return FriendStatus.Friends;
            }

            _pending.Add(Key(fromId, toId));
            return FriendStatus.PendingOutgoing;
        }

        public bool Accept(string userId, string requesterId)
        {
            if (!_pending.Remove(Key(requesterId, userId))) return false; // no incoming request
            _pending.Remove(Key(userId, requesterId));
            AddFriendship(userId, requesterId);
            return true;
        }

        public bool Decline(string userId, string requesterId) => _pending.Remove(Key(requesterId, userId));

        public bool Remove(string userId, string otherId)
        {
            bool removed = false;
            if (_friends.TryGetValue(userId, out var a)) removed = a.Remove(otherId);
            if (_friends.TryGetValue(otherId, out var b)) b.Remove(userId);
            return removed;
        }

        public bool AreFriends(string a, string b) => _friends.TryGetValue(a, out var set) && set.Contains(b);

        public IReadOnlyList<string> FriendsOf(string userId) =>
            _friends.TryGetValue(userId, out var set) ? new List<string>(set) : new List<string>();

        public IReadOnlyList<string> PendingIncoming(string userId)
        {
            var r = new List<string>();
            foreach (var k in _pending)
            {
                int i = k.IndexOf(Sep);
                if (k.Substring(i + 1) == userId) r.Add(k.Substring(0, i));
            }
            return r;
        }

        public IReadOnlyList<string> PendingOutgoing(string userId)
        {
            var r = new List<string>();
            foreach (var k in _pending)
            {
                int i = k.IndexOf(Sep);
                if (k.Substring(0, i) == userId) r.Add(k.Substring(i + 1));
            }
            return r;
        }

        public FriendStatus StatusBetween(string viewerId, string otherId)
        {
            if (AreFriends(viewerId, otherId)) return FriendStatus.Friends;
            if (_pending.Contains(Key(viewerId, otherId))) return FriendStatus.PendingOutgoing;
            if (_pending.Contains(Key(otherId, viewerId))) return FriendStatus.PendingIncoming;
            return FriendStatus.None;
        }

        private void AddFriendship(string a, string b)
        {
            if (!_friends.TryGetValue(a, out var sa)) { sa = new HashSet<string>(); _friends[a] = sa; }
            if (!_friends.TryGetValue(b, out var sb)) { sb = new HashSet<string>(); _friends[b] = sb; }
            sa.Add(b); sb.Add(a);
        }
    }

    /// <summary>Friend actions plus the notification side-effects: a request notifies the target; an accept
    /// notifies the requester. Wraps an <see cref="IFriendGraph"/> and the <see cref="NotificationCenter"/>.</summary>
    public sealed class FriendService
    {
        private readonly IFriendGraph _graph;
        private readonly NotificationCenter _notifications;
        private readonly IUserDirectory _directory;

        public FriendService(IFriendGraph graph, NotificationCenter notifications, IUserDirectory directory)
        {
            _graph = graph; _notifications = notifications; _directory = directory;
        }

        public IFriendGraph Graph => _graph;
        private string Name(string id) => _directory.TryGet(id, out var u) ? u.DisplayName : "Someone";

        public FriendStatus SendRequest(string fromId, string toId)
        {
            var status = _graph.SendRequest(fromId, toId);
            if (status == FriendStatus.PendingOutgoing)
                _notifications.Post(NotificationKind.FriendRequest, "Friend request",
                    Name(fromId) + " wants to be friends.", toId, fromId);
            else if (status == FriendStatus.Friends) // reciprocal request -> instant friends
                _notifications.Post(NotificationKind.FriendRequest, "New friend",
                    "You and " + Name(fromId) + " are now friends.", toId, fromId);
            return status;
        }

        public bool Accept(string userId, string requesterId)
        {
            if (!_graph.Accept(userId, requesterId)) return false;
            _notifications.Post(NotificationKind.FriendRequest, "Friend request accepted",
                Name(userId) + " accepted your friend request.", requesterId, userId);
            return true;
        }

        public bool Decline(string userId, string requesterId) => _graph.Decline(userId, requesterId);
        public bool Unfriend(string userId, string otherId) => _graph.Remove(userId, otherId);

        public IReadOnlyList<string> FriendsOf(string userId) => _graph.FriendsOf(userId);
        public IReadOnlyList<string> PendingIncoming(string userId) => _graph.PendingIncoming(userId);
    }
}
