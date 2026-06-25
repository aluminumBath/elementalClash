using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    /// <summary>Who a user is in the social layer. Admins moderate globally; session-admins moderate the session
    /// they host; everyone else is a Player.</summary>
    public enum UserRole { Player, SessionAdmin, Admin }

    /// <summary>An immutable reference to a user. <see cref="WithRole"/> returns a copy with a new role.</summary>
    public readonly struct UserRef
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly UserRole Role;

        public UserRef(string id, string displayName, UserRole role)
        {
            Id = id; DisplayName = displayName; Role = role;
        }

        public UserRef WithRole(UserRole role) => new UserRef(Id, DisplayName, role);

        // Compatibility alias for older UI/controller code. The canonical field is DisplayName.
        public string Name => DisplayName;

        public bool IsAdmin => Role == UserRole.Admin;
        public bool CanModerateSessions => Role == UserRole.Admin || Role == UserRole.SessionAdmin;
    }

    /// <summary>The seam over "who exists and what role they hold". <see cref="LocalUserDirectory"/> is the
    /// offline implementation; a backend adapter (e.g. Nakama) implements the same interface for the networked
    /// build.</summary>
    public interface IUserDirectory
    {
        void Register(UserRef user);
        bool TryGet(string userId, out UserRef user);
        void SetRole(string userId, UserRole role);
        IReadOnlyList<UserRef> All();
        IReadOnlyList<UserRef> Admins();
    }

    /// <summary>In-memory user directory — needs no server, so it runs and unit-tests offline.</summary>
    public sealed class LocalUserDirectory : IUserDirectory
    {
        private readonly Dictionary<string, UserRef> _users = new Dictionary<string, UserRef>();

        public void Register(UserRef user) => _users[user.Id] = user;
        public bool TryGet(string userId, out UserRef user) => _users.TryGetValue(userId, out user);

        public void SetRole(string userId, UserRole role)
        {
            if (_users.TryGetValue(userId, out var u)) _users[userId] = u.WithRole(role);
        }

        public IReadOnlyList<UserRef> All() => new List<UserRef>(_users.Values);

        public IReadOnlyList<UserRef> Admins()
        {
            var result = new List<UserRef>();
            foreach (var u in _users.Values)
                if (u.Role == UserRole.Admin) result.Add(u);
            return result;
        }
    }
}
