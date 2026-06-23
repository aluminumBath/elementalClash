using UnityEngine;
using Elementborn.Core;
using Elementborn.Core.Social;
using Elementborn.Game.Social;

namespace Elementborn.Game
{
    /// <summary>
    /// DEV / DEMO ONLY — a stand-in for the Nakama presence feed. Seeds one demo ally as a friend and reports it
    /// slowly orbiting the player, so the map's friend markers (and the consent path) can be seen without a live
    /// server or a second client. Registers itself as <see cref="MapState"/>'s presence source. Not for a shipping
    /// build — the bootstrap sandbox scene adds it as demo content; delete the object (or this component) and the
    /// networked build's real Nakama source takes over.
    /// </summary>
    public sealed class SimulatedFriendPresence : MonoBehaviour, IFriendPresence
    {
        [SerializeField] private string allyId = "demo-ally";
        [SerializeField] private string allyName = "Aria (demo)";
        [SerializeField] private float orbitRadius = 40f;  // world units from the player
        [SerializeField] private float orbitSpeed = 0.3f;  // radians / second

        private bool _seeded;

        private void OnEnable() => MapState.SetPresence(this);
        private void OnDisable() => MapState.ClearPresence(this);

        // Dev source: nothing is sent upstream.
        public void PublishSelf(string localId, Vector3 position, bool sharing, float now) { }

        public void Poll(PresenceRegistry into, float now)
        {
            SeedFriendship();
            var rig = RigTeleporter.Rig;
            Vector3 c = rig != null ? rig.position : Vector3.zero;
            float a = now * orbitSpeed;
            into.Report(allyId, c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * orbitRadius, now);
        }

        // Register the ally and befriend it once, so it passes the map's friend filter (and exercises the same
        // consent gate a real sharing friend would).
        private void SeedFriendship()
        {
            if (_seeded) return;
            var hub = SocialHub.Instance;
            if (hub == null) return;

            if (!hub.Directory.TryGet(allyId, out _))
                hub.Directory.Register(new UserRef(allyId, allyName, UserRole.Player));

            string me = hub.CurrentUser.Id;
            if (!hub.Friends.Graph.AreFriends(me, allyId))
            {
                hub.Friends.SendRequest(me, allyId);
                hub.Friends.Accept(allyId, me); // the ally accepts our request
            }
            _seeded = true;
        }
    }
}
