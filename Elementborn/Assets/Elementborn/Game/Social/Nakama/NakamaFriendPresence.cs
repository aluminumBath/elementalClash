#if ELEMENTBORN_NAKAMA
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Game.Social.NakamaNet
{
    /// <summary>
    /// The live friend-position producer (<see cref="IFriendPresence"/>), backed by Nakama status presence and
    /// built the same way as the adapters in NakamaAdapters: from the shared <see cref="NakamaConnection"/>, it
    /// subscribes to a socket event, caches what arrives, and writes are fire-and-forget and tolerate a
    /// not-yet-connected state. The local position is broadcast as the player's status string (packed by
    /// <see cref="PresenceCodec"/>) only while they opt in, and the producer follows the current friends to
    /// receive theirs. Registered by <c>NakamaSocialInstaller</c> after connect.
    /// NOTE: like the rest of the Nakama layer, this is verified against a live server — not the offline gates.
    /// </summary>
    public sealed class NakamaFriendPresence : IFriendPresence
    {
        private readonly NakamaConnection _c;
        private readonly Dictionary<string, Vector3> _positions = new Dictionary<string, Vector3>();
        private readonly HashSet<string> _following = new HashSet<string>();
        private bool _sharingLast;

        public NakamaFriendPresence(NakamaConnection c)
        {
            _c = c;
            if (_c != null && _c.Socket != null) _c.Socket.ReceivedStatusPresence += OnStatusPresence;
        }

        public void PublishSelf(string localId, Vector3 position, bool sharing, float now)
        {
            if (_c == null || !_c.Connected) return;
            // Broadcast our position as a status while sharing; clear it the moment we stop.
            if (sharing) _ = Safe(_c.Socket.UpdateStatusAsync(PresenceCodec.Encode(position)));
            else if (_sharingLast) _ = Safe(_c.Socket.UpdateStatusAsync(""));
            _sharingLast = sharing;
        }

        public void Poll(PresenceRegistry into, float now)
        {
            if (_c == null || !_c.Connected) return;
            var hub = SocialHub.Instance;
            if (hub == null) return;

            var friends = hub.Friends.FriendsOf(hub.CurrentUser.Id);
            EnsureFollowing(friends);

            for (int i = 0; i < friends.Count; i++)
                if (_positions.TryGetValue(friends[i], out var p)) into.Report(friends[i], p, now);
        }

        // Follow any friends we aren't following yet (so their status presences start arriving).
        private void EnsureFollowing(IReadOnlyList<string> friends)
        {
            List<string> add = null;
            for (int i = 0; i < friends.Count; i++)
                if (_following.Add(friends[i]))
                {
                    if (add == null) add = new List<string>();
                    add.Add(friends[i]);
                }
            if (add != null) _ = FollowAsync(add);
        }

        private async Task FollowAsync(List<string> userIds)
        {
            try
            {
                var status = await _c.Socket.FollowUsersAsync(userIds);
                if (status != null && status.Presences != null)
                    foreach (var pr in status.Presences) Absorb(pr);
            }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] follow failed: " + e.Message); }
        }

        private void OnStatusPresence(IStatusPresenceEvent e)
        {
            if (e == null) return;
            if (e.Leaves != null) foreach (var pr in e.Leaves) _positions.Remove(pr.UserId);
            if (e.Joins != null) foreach (var pr in e.Joins) Absorb(pr);
        }

        // A presence whose status decodes is a sharing friend at that position; anything else means "not here".
        private void Absorb(IUserPresence pr)
        {
            if (pr == null) return;
            if (PresenceCodec.TryDecode(pr.Status, out var pos)) _positions[pr.UserId] = pos;
            else _positions.Remove(pr.UserId);
        }

        private static async Task Safe(Task t)
        {
            try { await t; }
            catch (System.Exception e) { Debug.LogWarning("[Nakama] status update failed: " + e.Message); }
        }
    }
}
#endif
