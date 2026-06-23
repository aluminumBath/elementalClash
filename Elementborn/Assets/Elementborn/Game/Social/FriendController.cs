using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core.Social;

namespace Elementborn.Game.Social
{
    /// <summary>
    /// Backs a friends-list UI: send/accept/decline requests, unfriend, and read the current user's friends and
    /// incoming requests. Raises <see cref="Changed"/> so the list can refresh. Wires onto <see cref="SocialHub"/>.
    /// </summary>
    public sealed class FriendController : MonoBehaviour
    {
        public event System.Action Changed;

        public FriendStatus SendRequest(string otherUserId)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return FriendStatus.None;
            var status = hub.Friends.SendRequest(hub.CurrentUser.Id, otherUserId);
            Changed?.Invoke();
            return status;
        }

        public bool Accept(string requesterId) => Do(hub => hub.Friends.Accept(hub.CurrentUser.Id, requesterId));
        public bool Decline(string requesterId) => Do(hub => hub.Friends.Decline(hub.CurrentUser.Id, requesterId));
        public bool Unfriend(string otherId) => Do(hub => hub.Friends.Unfriend(hub.CurrentUser.Id, otherId));

        private bool Do(System.Func<SocialHub, bool> action)
        {
            var hub = SocialHub.Instance;
            if (hub == null) return false;
            bool ok = action(hub);
            if (ok) Changed?.Invoke();
            return ok;
        }

        public IReadOnlyList<string> FriendIds =>
            SocialHub.Instance != null ? SocialHub.Instance.Friends.FriendsOf(SocialHub.Instance.CurrentUser.Id) : new List<string>();

        public IReadOnlyList<string> IncomingRequests =>
            SocialHub.Instance != null ? SocialHub.Instance.Friends.PendingIncoming(SocialHub.Instance.CurrentUser.Id) : new List<string>();
    }
}
