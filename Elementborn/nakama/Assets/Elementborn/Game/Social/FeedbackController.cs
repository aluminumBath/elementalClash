using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// Backs the in-game "report a bug / suggest a feature" section open to all users. A simple form calls
    /// <see cref="SubmitBug"/> or <see cref="SubmitSuggestion"/>; the report is recorded and a notification goes
    /// to every admin (via <see cref="SocialHub"/>). The form UI is a follow-up; this is the wiring.
    /// </summary>
    public sealed class FeedbackController : MonoBehaviour
    {
        public FeedbackReport SubmitBug(string title, string body) => Submit(FeedbackKind.Bug, title, body);
        public FeedbackReport SubmitSuggestion(string title, string body) => Submit(FeedbackKind.Suggestion, title, body);

        private FeedbackReport Submit(FeedbackKind kind, string title, string body)
        {
            var hub = SocialHub.Instance;
            if (hub == null || string.IsNullOrWhiteSpace(title)) return null;

            var report = hub.Feedback.Submit(kind, title, body, hub.CurrentUser.Id);
            Debug.Log($"[Feedback] {kind}: '{title}' submitted — {hub.Directory.Admins().Count} admin(s) notified.");
            return report;
        }
    }
}
