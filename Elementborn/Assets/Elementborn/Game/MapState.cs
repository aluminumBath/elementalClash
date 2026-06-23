using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game.Social;

namespace Elementborn.Game
{
    /// <summary>
    /// Runtime owner of the map's navigation state: the leyline <see cref="FastTravelNetwork"/> (seeded from
    /// <see cref="WorldMap"/>), the friend <see cref="LocationSharing"/> set, and the local player's own broadcast
    /// opt-in. Rift objects discover through it, the minimap and viewer read from it, and it persists through
    /// <see cref="PlayerInventory"/>. The bootstrap scene adds one.
    /// </summary>
    public sealed class MapState : MonoBehaviour
    {
        public static MapState Instance { get; private set; }

        private FastTravelNetwork _network;
        private readonly LocationSharing _sharing = new LocationSharing();
        // No live friend-position feed yet (that would come from the Nakama presence layer). An empty set keeps
        // VisibleFriends correct — consent-gated, with nothing to show — until that feed exists.
        private readonly Dictionary<string, Vector3> _friendPositions = new Dictionary<string, Vector3>();

        /// <summary>The local player's opt-in to broadcast their own location to friends. Off by default (privacy).</summary>
        public bool ShareMyLocation { get; set; }

        public FastTravelNetwork Network => _network;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _network = WorldMap.BuildNetwork();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public string LocalId => SocialHub.Instance != null ? SocialHub.Instance.CurrentUser.Id : "local";

        private IReadOnlyList<string> FriendIds =>
            SocialHub.Instance != null
                ? SocialHub.Instance.Friends.FriendsOf(SocialHub.Instance.CurrentUser.Id)
                : new List<string>();

        /// <summary>Mark a rift discovered (idempotent). True if it was newly discovered.</summary>
        public bool Discover(string riftId) => _network != null && _network.Discover(riftId);

        /// <summary>Warp to a discovered rift, landing on the terrain. False if it can't be travelled to / no rig.</summary>
        public bool WarpToRift(string riftId)
        {
            if (_network == null || !_network.CanTravelTo(riftId)) return false;
            foreach (var r in _network.All)
                if (r.Id == riftId)
                {
                    Vector3 pos = r.World;
                    pos.y = TerrainHeight.Sample(pos) + 1f; // settle on the ground at the destination
                    return RigTeleporter.WarpTo(pos);
                }
            return false;
        }

        public MapMarker SelfMarker(Vector3 world) => Locator.Self(LocalId, world);

        public List<MapMarker> FriendMarkers() => Locator.VisibleFriends(FriendIds, _sharing, _friendPositions);

        // --- persistence (folded into PlayerInventory.ToSave / LoadFrom) ---
        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.discoveredRifts.Clear();
            if (_network != null) foreach (var id in _network.ToSave()) d.discoveredRifts.Add(id);
            d.shareLocation = ShareMyLocation;
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            _network?.LoadDiscovered(d.discoveredRifts);
            ShareMyLocation = d.shareLocation;
        }
    }
}
