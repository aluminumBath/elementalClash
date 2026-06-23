using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// Backs a moderation screen. The current user is the actor; the underlying service enforces roles (only an
    /// admin bans globally; an admin or session-admin bans/allows within a session). Session actions target
    /// <see cref="SocialHub.CurrentSessionId"/>. Wires onto <see cref="SocialHub"/>.
    /// </summary>
    public sealed class ModerationController : MonoBehaviour
    {
        public event System.Action Changed;

        public bool BanGlobally(string targetId, string reason) =>
            Do(h => h.Moderation.BanGlobally(h.CurrentUser.Id, targetId, reason));

        public bool LiftGlobalBan(string targetId) =>
            Do(h => h.Moderation.LiftGlobalBan(h.CurrentUser.Id, targetId));

        public bool BanFromSession(string targetId, string reason) =>
            Do(h => h.Moderation.BanFromSession(h.CurrentUser.Id, targetId, h.CurrentSessionId, reason));

        public bool AllowInSession(string targetId) =>
            Do(h => h.Moderation.AllowInSession(h.CurrentUser.Id, targetId, h.CurrentSessionId));

        public bool CanJoin(string userId, string sessionId)
        {
            var hub = SocialHub.Instance;
            return hub == null || hub.Moderation.CanJoin(userId, sessionId);
        }

        private bool Do(System.Func<SocialHub, bool> action)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return false;
            bool ok = action(hub);
            if (ok) Changed?.Invoke();
            return ok;
        }
    }
}
