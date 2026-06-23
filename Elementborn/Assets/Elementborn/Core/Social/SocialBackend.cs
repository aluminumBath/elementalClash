namespace Elementborn.Core.Social
{
    /// <summary>
    /// The set of seams the social <em>services</em> run on. The whole social layer is built on these seven
    /// interfaces, so swapping backends is swapping one object. The default is the offline
    /// <see cref="LocalSocialBackend"/>; a networked build supplies a Nakama-backed implementation with the same
    /// seams (see docs/NETCODE.md) and nothing in the services or UI changes.
    /// </summary>
    public interface ISocialBackend
    {
        IUserDirectory Directory { get; }
        INotificationStore Notifications { get; }
        IFeedbackStore Feedback { get; }
        IFriendGraph Friends { get; }
        IInviteStore Invites { get; }
        IMessageTransport Messages { get; }
        IModerationStore Moderation { get; }
    }

    /// <summary>The offline, in-memory backend — runs and unit-tests with no server.</summary>
    public sealed class LocalSocialBackend : ISocialBackend
    {
        public IUserDirectory Directory { get; } = new LocalUserDirectory();
        public INotificationStore Notifications { get; } = new InMemoryNotificationStore();
        public IFeedbackStore Feedback { get; } = new InMemoryFeedbackStore();
        public IFriendGraph Friends { get; } = new LocalFriendGraph();
        public IInviteStore Invites { get; } = new InMemoryInviteStore();
        public IMessageTransport Messages { get; } = new InMemoryMessageTransport();
        public IModerationStore Moderation { get; } = new InMemoryModerationStore();
    }

    /// <summary>
    /// Chooses the active social backend. Defaults to <see cref="LocalSocialBackend"/>; under the
    /// <c>ELEMENTBORN_NAKAMA</c> define the Nakama installer replaces <see cref="Factory"/> before
    /// <c>SocialHub</c> builds, so the networked backend is used instead. Resetting <see cref="Factory"/> to null
    /// falls back to local.
    /// </summary>
    public static class SocialBackends
    {
        public static System.Func<ISocialBackend> Factory = () => new LocalSocialBackend();

        public static ISocialBackend Create() => Factory?.Invoke() ?? new LocalSocialBackend();
    }
}
