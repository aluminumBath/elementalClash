using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    public enum FeedbackKind { Bug, Suggestion }

    /// <summary>A bug report or feature suggestion submitted by a user.</summary>
    public sealed class FeedbackReport
    {
        public string Id { get; }
        public FeedbackKind Kind { get; }
        public string Title { get; }
        public string Body { get; }
        public string FromUserId { get; }
        public long CreatedAtUtcTicks { get; }

        public FeedbackReport(string id, FeedbackKind kind, string title, string body,
                              string fromUserId, long createdAtUtcTicks)
        {
            Id = id; Kind = kind; Title = title; Body = body;
            FromUserId = fromUserId; CreatedAtUtcTicks = createdAtUtcTicks;
        }
    }

    /// <summary>The seam over feedback storage. <see cref="InMemoryFeedbackStore"/> is offline; a backend
    /// adapter persists reports server-side for an admin dashboard.</summary>
    public interface IFeedbackStore
    {
        void Save(FeedbackReport report);
        IReadOnlyList<FeedbackReport> All();
    }

    public sealed class InMemoryFeedbackStore : IFeedbackStore
    {
        private readonly List<FeedbackReport> _reports = new List<FeedbackReport>();
        public void Save(FeedbackReport report) => _reports.Add(report);
        public IReadOnlyList<FeedbackReport> All() => _reports;
    }

    /// <summary>Backs the in-game "report a bug / suggest a feature" section open to all users: it records the
    /// report and posts a <see cref="NotificationKind.Feedback"/> notification to every admin in the directory.</summary>
    public sealed class FeedbackService
    {
        private readonly IFeedbackStore _store;
        private readonly NotificationCenter _notifications;
        private readonly IUserDirectory _directory;

        public FeedbackService(IFeedbackStore store, NotificationCenter notifications, IUserDirectory directory)
        {
            _store = store; _notifications = notifications; _directory = directory;
        }

        public FeedbackReport Submit(FeedbackKind kind, string title, string body, string fromUserId)
        {
            var report = new FeedbackReport(System.Guid.NewGuid().ToString("N"), kind, title, body,
                                            fromUserId, System.DateTime.UtcNow.Ticks);
            _store.Save(report);

            string label = kind == FeedbackKind.Bug ? "Bug report" : "Feature suggestion";
            foreach (var admin in _directory.Admins())
                _notifications.Post(NotificationKind.Feedback, label + ": " + title, body, admin.Id, fromUserId);

            return report;
        }

        public IReadOnlyList<FeedbackReport> AllReports() => _store.All();
    }
}
