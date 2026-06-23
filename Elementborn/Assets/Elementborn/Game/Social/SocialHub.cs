using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// The single access point for the social layer. Phase 1 wires the <b>local in-memory backend</b> — no server
    /// required, so everything runs and unit-tests offline. For the networked build, a Nakama (or other) adapter
    /// implementing the same Core seams (<see cref="IUserDirectory"/> / <see cref="INotificationStore"/> /
    /// <see cref="IFeedbackStore"/>) is constructed here instead; nothing else changes. See docs/SOCIAL.md.
    /// Put one on a bootstrap object.
    /// </summary>
    public sealed class SocialHub : MonoBehaviour
    {
        public static SocialHub Instance { get; private set; }

        [SerializeField] private string displayName = "Player";

        public IUserDirectory Directory { get; private set; }
        public NotificationCenter Notifications { get; private set; }
        public FeedbackService Feedback { get; private set; }
        public FriendService Friends { get; private set; }
        public InviteService Invites { get; private set; }
        public ChatService Chat { get; private set; }
        public ModerationService Moderation { get; private set; }
        public UserRef CurrentUser { get; private set; }

        /// <summary>The local player's session/lobby id — friends are invited to this. In the networked build a
        /// Nakama party/match id stands in here.</summary>
        public string CurrentSessionId { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Seams come from the active backend: the offline LocalSocialBackend by default, or a Nakama-backed
            // one under the ELEMENTBORN_NAKAMA define (see docs/NETCODE.md). The services below are unchanged.
            BuildServices(SocialBackends.Create());
            CurrentSessionId = System.Guid.NewGuid().ToString("N");

            // Local dev identity. A real sign-in (Nakama device/auth) provides this via SetIdentity in the
            // networked build.
            CurrentUser = new UserRef(System.Guid.NewGuid().ToString("N"), displayName, UserRole.Player);
            Directory.Register(CurrentUser);
        }

        private void BuildServices(ISocialBackend backend)
        {
            Directory = backend.Directory;
            Notifications = new NotificationCenter(backend.Notifications);
            Feedback = new FeedbackService(backend.Feedback, Notifications, Directory);
            Friends = new FriendService(backend.Friends, Notifications, Directory);
            Invites = new InviteService(backend.Invites, Notifications, Directory, backend.Friends);
            Chat = new ChatService(backend.Messages);
            Moderation = new ModerationService(backend.Moderation, Notifications, Directory);
        }

        /// <summary>Replace the local dev identity with a real one — e.g. after Nakama device auth resolves the
        /// user id, display name, and the party/match that stands in for the session.</summary>
        public void SetIdentity(string userId, string name, UserRole role, string sessionId)
        {
            if (!string.IsNullOrEmpty(sessionId)) CurrentSessionId = sessionId;
            CurrentUser = new UserRef(userId, name, role);
            Directory.Register(CurrentUser);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Dev/test helper: make the local user an admin (the networked build assigns roles server-side),
        /// so feedback-to-admin and moderation can be exercised offline.</summary>
        public void GrantSelfAdmin()
        {
            Directory.SetRole(CurrentUser.Id, UserRole.Admin);
            if (Directory.TryGet(CurrentUser.Id, out var u)) CurrentUser = u;
        }
    }
}
