using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    public enum MapMarkerKind { Self, Friend, LeylineRift, Checkpoint, City, Quest }

    /// <summary>A thing drawn on the map / minimap at a world position.</summary>
    public readonly struct MapMarker
    {
        public readonly string Id;
        public readonly MapMarkerKind Kind;
        public readonly Vector3 World;
        public readonly string Label;
        public MapMarker(string id, MapMarkerKind kind, Vector3 world, string label)
        { Id = id; Kind = kind; World = world; Label = label; }
    }

    /// <summary>Portal tiers: capital hubs (the elemental capital pools you portal <i>from</i>) versus the city
    /// portals they route <i>to</i>.</summary>
    public enum PortalTier { City, Capital }

    /// <summary>A leyline rift / elemental portal: a fast-travel node you can warp between once discovered. A portal
    /// carries an element (null for the elementless Confluence gate and the neutral crossings) and a tier, so a
    /// capital pool can list the discovered <b>city</b> portals of its own element.</summary>
    public readonly struct LeylineRift
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Vector3 World;
        public readonly Element? Elem;   // the portal's element (null = elementless gate/crossing)
        public readonly PortalTier Tier; // capital hub vs city portal

        public LeylineRift(string id, string name, Vector3 world)
            : this(id, name, world, null, PortalTier.City) { }

        public LeylineRift(string id, string name, Vector3 world, Element? elem, PortalTier tier)
        { Id = id; Name = name; World = world; Elem = elem; Tier = tier; }
    }

    /// <summary>How an elemental portal reads on the themed map and in-world: a glow colour and a look hint. Water
    /// portals are glassy ponds that glow teal; fire a molten rift; earth a mossy stone arch; air a shimmering
    /// updraft; the elementless Confluence gate a prismatic ring.</summary>
    public readonly struct PortalStyle
    {
        public readonly Color Glow;
        public readonly string Look;
        public PortalStyle(Color glow, string look) { Glow = glow; Look = look; }
    }

    public static class PortalTheme
    {
        public static PortalStyle For(Element? element)
        {
            if (element == null) return new PortalStyle(new Color(0.80f, 0.72f, 1f), "a prismatic Confluence ring");
            switch (element.Value)
            {
                case Element.Water: return new PortalStyle(new Color(0.16f, 0.85f, 0.78f), "a glassy pond that glows teal");
                case Element.Fire:  return new PortalStyle(new Color(1f, 0.50f, 0.16f), "a molten rift ringed in embers");
                case Element.Earth: return new PortalStyle(new Color(0.50f, 0.80f, 0.35f), "a mossy stone arch");
                case Element.Air:   return new PortalStyle(new Color(0.72f, 0.95f, 1f), "a shimmering updraft");
                default:            return new PortalStyle(Color.white, "a portal");
            }
        }
    }

    /// <summary>
    /// The fast-travel network of leyline rifts. All rifts are registered up front; you may only travel to ones
    /// you've discovered (stepped on / activated). Savable, and discovery never un-sets.
    /// </summary>
    public sealed class FastTravelNetwork
    {
        private readonly Dictionary<string, LeylineRift> _rifts = new Dictionary<string, LeylineRift>();
        private readonly HashSet<string> _discovered = new HashSet<string>();

        public void Register(LeylineRift rift) { if (!string.IsNullOrEmpty(rift.Id)) _rifts[rift.Id] = rift; }

        /// <summary>Mark a rift discovered. Returns true if it was newly discovered.</summary>
        public bool Discover(string riftId) => _rifts.ContainsKey(riftId) && _discovered.Add(riftId);

        public bool IsDiscovered(string riftId) => _discovered.Contains(riftId);

        /// <summary>You can only warp to a rift that exists and has been discovered.</summary>
        public bool CanTravelTo(string riftId) => _rifts.ContainsKey(riftId) && _discovered.Contains(riftId);

        /// <summary>The discovered <b>city</b> portals of an element — the destinations a player may pick from that
        /// element's capital pool (capitals themselves are not travel targets from the pool). Ordered by name.</summary>
        public List<LeylineRift> DiscoveredCitiesOfElement(Element element)
        {
            var list = new List<LeylineRift>();
            foreach (var r in _rifts.Values)
                if (r.Tier == PortalTier.City && r.Elem == element && _discovered.Contains(r.Id)) list.Add(r);
            list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return list;
        }

        /// <summary>Whether a capital pool of <paramref name="element"/> may portal to <paramref name="destId"/>:
        /// the destination must be a discovered city portal of the same element.</summary>
        public bool CanRouteFromCapital(Element element, string destId)
            => _rifts.TryGetValue(destId, out var r) && r.Tier == PortalTier.City && r.Elem == element && _discovered.Contains(destId);

        public IReadOnlyCollection<LeylineRift> All => _rifts.Values;

        public List<LeylineRift> Discovered()
        {
            var list = new List<LeylineRift>();
            foreach (var r in _rifts.Values) if (_discovered.Contains(r.Id)) list.Add(r);
            return list;
        }

        /// <summary>The nearest discovered rift to a position, or null if none discovered.</summary>
        public LeylineRift? NearestDiscovered(Vector3 world)
        {
            LeylineRift? best = null;
            float bestSq = float.MaxValue;
            foreach (var r in _rifts.Values)
            {
                if (!_discovered.Contains(r.Id)) continue;
                float sq = (r.World - world).sqrMagnitude;
                if (sq < bestSq) { bestSq = sq; best = r; }
            }
            return best;
        }

        public IReadOnlyCollection<string> ToSave() => new List<string>(_discovered);
        public void LoadDiscovered(IEnumerable<string> ids)
        {
            if (ids == null) return;
            foreach (var id in ids) if (_rifts.ContainsKey(id)) _discovered.Add(id);
        }
    }

    /// <summary>Per-user opt-in to share live location with friends. Off by default — privacy is the default.</summary>
    public sealed class LocationSharing
    {
        private readonly HashSet<string> _sharing = new HashSet<string>();

        public void SetSharing(string userId, bool on)
        {
            if (string.IsNullOrEmpty(userId)) return;
            if (on) _sharing.Add(userId); else _sharing.Remove(userId);
        }

        public bool IsSharing(string userId) => _sharing.Contains(userId);

        public IReadOnlyCollection<string> ToSave() => new List<string>(_sharing);
        public void Load(IEnumerable<string> ids) { if (ids != null) foreach (var id in ids) _sharing.Add(id); }
    }

    /// <summary>Builds the map markers to show. You are always located; friends only if they've opted in to share.</summary>
    public static class Locator
    {
        /// <summary>The local player's own marker — always available.</summary>
        public static MapMarker Self(string localId, Vector3 world) =>
            new MapMarker(localId, MapMarkerKind.Self, world, "You");

        /// <summary>
        /// Markers for friends who have opted in to share AND whose position is known. Pass the local player's
        /// friend ids (from the friend graph); consent is gated by <paramref name="sharing"/>.
        /// </summary>
        public static List<MapMarker> VisibleFriends(IReadOnlyList<string> friendIds, LocationSharing sharing,
            IReadOnlyDictionary<string, Vector3> friendPositions)
        {
            var markers = new List<MapMarker>();
            if (friendIds == null || sharing == null || friendPositions == null) return markers;
            foreach (var fid in friendIds)
                if (sharing.IsSharing(fid) && friendPositions.TryGetValue(fid, out var pos))
                    markers.Add(new MapMarker(fid, MapMarkerKind.Friend, pos, fid));
            return markers;
        }
    }

    /// <summary>Pure helpers to place world positions onto a 2D map / minimap and filter by proximity.</summary>
    public static class Minimap
    {
        /// <summary>World XZ → normalized [0,1] map coordinates within world bounds (degenerate spans map to centre).</summary>
        public static Vector2 WorldToNormalized(Vector3 world, Vector3 worldMin, Vector3 worldMax)
        {
            float u = Approx(worldMax.x - worldMin.x) ? 0.5f : Mathf.InverseLerp(worldMin.x, worldMax.x, world.x);
            float v = Approx(worldMax.z - worldMin.z) ? 0.5f : Mathf.InverseLerp(worldMin.z, worldMax.z, world.z);
            return new Vector2(u, v);
        }

        /// <summary>True if <paramref name="world"/> is within <paramref name="radius"/> of <paramref name="center"/> on the XZ plane (the minimap's nearby ring).</summary>
        public static bool WithinRange(Vector3 center, float radius, Vector3 world)
        {
            float dx = world.x - center.x, dz = world.z - center.z;
            return dx * dx + dz * dz <= radius * radius;
        }

        private static bool Approx(float span) => span < 0.0001f && span > -0.0001f;
    }
}
