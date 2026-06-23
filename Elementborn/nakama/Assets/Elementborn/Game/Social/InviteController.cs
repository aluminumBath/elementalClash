using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// Backs invite UI: invite a friend to your session, and accept/decline incoming invites. Accepting raises
    /// <see cref="JoinSession"/> with the host's session id — the netcode layer (a Nakama party/match join)
    /// hooks that event to actually connect. Wires onto <see cref="SocialHub"/>.
    /// </summary>
    public sealed class InviteController : MonoBehaviour
    {
        public event System.Action Changed;
        public event System.Action<string> JoinSession;

        public GameInvite InviteFriend(string friendUserId)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return null;
            var invite = hub.Invites.Invite(hub.CurrentUser.Id, friendUserId, hub.CurrentSessionId);
            if (invite != null) Debug.Log($"[Invite] Invited {friendUserId} to session {hub.CurrentSessionId}.");
            return invite;
        }

        public void Accept(string inviteId)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return;
            string session = hub.Invites.Accept(inviteId, hub.CurrentUser.Id);
            Changed?.Invoke();
            if (session == null) return;
            if (!hub.Moderation.CanJoin(hub.CurrentUser.Id, session))   // moderation consulted at join
            {
                Debug.Log("[Invite] Join blocked — you're banned from that session.");
                return;
            }
            JoinSession?.Invoke(session); // netcode layer joins here
        }

        public bool Decline(string inviteId)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return false;
            bool ok = hub.Invites.Decline(inviteId, hub.CurrentUser.Id);
            if (ok) Changed?.Invoke();
            return ok;
        }

        public IReadOnlyList<GameInvite> Pending =>
            SocialHub.Instance != null ? SocialHub.Instance.Invites.PendingFor(SocialHub.Instance.CurrentUser.Id) : new List<GameInvite>();
    }
}
