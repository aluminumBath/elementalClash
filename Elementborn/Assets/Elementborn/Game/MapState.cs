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
        // Friend positions refreshed each poll from the registered presence source (Nakama online; a dev simulator
        // offline). With no source the set stays empty, so VisibleFriends stays correct — consent-gated, nothing to
        // show. Self is always located directly and never needs the feed.
        private readonly Dictionary<string, Vector3> _friendPositions = new Dictionary<string, Vector3>();
        private readonly PresenceRegistry _presence = new PresenceRegistry();
        private static IFriendPresence _provider;
        private const float PresenceTtl = 6f;     // a friend silent longer than this drops off the map
        private const float PollInterval = 0.5f;  // how often we publish/pull presence
        private float _pollTimer;

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

        /// <summary>Register the live presence source — the networked build's Nakama feed, or a dev simulator. Pass
        /// the same instance to <see cref="ClearPresence"/> on teardown so a destroyed source can't linger.</summary>
        public static void SetPresence(IFriendPresence provider) => _provider = provider;
        public static void ClearPresence(IFriendPresence provider) { if (_provider == provider) _provider = null; }

        private void Update()
        {
            _pollTimer -= Time.deltaTime;
            if (_pollTimer > 0f) return;
            _pollTimer = PollInterval;

            if (_provider == null)
            {
                if (_friendPositions.Count > 0) _friendPositions.Clear(); // source gone -> don't leave stale markers
                return;
            }

            float now = Time.time;
            var rig = RigTeleporter.Rig;
            _provider.PublishSelf(LocalId, rig != null ? rig.position : Vector3.zero, ShareMyLocation, now);
            _provider.Poll(_presence, now);

            _friendPositions.Clear();
            var fresh = _presence.Fresh(now, PresenceTtl);
            foreach (var fid in FriendIds)
            {
                bool present = fresh.TryGetValue(fid, out var pos); // present == broadcasting == sharing with us
                _sharing.SetSharing(fid, present);
                if (present) _friendPositions[fid] = pos;
            }
        }

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
                    bool ok = RigTeleporter.WarpTo(pos);
                    if (ok) AudioController.Instance?.Play(SfxKind.WhooshShort); // fast-travel whoosh
                    return ok;
                }
            return false;
        }

        public MapMarker SelfMarker(Vector3 world) => Locator.Self(LocalId, world);

        public List<MapMarker> FriendMarkers() => Locator.VisibleFriends(FriendIds, _sharing, _friendPositions);

        /// <summary>The consent-gated, freshness-filtered world positions of friends currently sharing with us —
        /// the same set the map draws, exposed read-only so the world can render them as visible co-op ally figures.</summary>
        public IReadOnlyDictionary<string, Vector3> VisibleFriendWorldPositions => _friendPositions;

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
