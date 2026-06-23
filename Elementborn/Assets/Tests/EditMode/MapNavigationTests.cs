using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Tests.EditMode
{
    public class MapNavigationTests
    {
        // ---- fast travel ----
        [Test]
        public void CannotTravelToAnUndiscoveredRift()
        {
            var net = new FastTravelNetwork();
            net.Register(new LeylineRift("rift_a", "Ashfall Rift", new Vector3(10, 0, 0)));
            Assert.IsFalse(net.IsDiscovered("rift_a"));
            Assert.IsFalse(net.CanTravelTo("rift_a"));
        }

        [Test]
        public void DiscoveringARiftUnlocksTravel()
        {
            var net = new FastTravelNetwork();
            net.Register(new LeylineRift("rift_a", "Ashfall Rift", Vector3.zero));
            Assert.IsTrue(net.Discover("rift_a"));
            Assert.IsFalse(net.Discover("rift_a"));   // already discovered
            Assert.IsTrue(net.CanTravelTo("rift_a"));
            Assert.AreEqual(1, net.Discovered().Count);
        }

        [Test]
        public void CannotDiscoverAnUnknownRift()
        {
            var net = new FastTravelNetwork();
            Assert.IsFalse(net.Discover("nope"));
        }

        [Test]
        public void NearestDiscoveredPicksTheClosestAndIgnoresUndiscovered()
        {
            var net = new FastTravelNetwork();
            net.Register(new LeylineRift("near", "Near", new Vector3(1, 0, 0)));
            net.Register(new LeylineRift("far", "Far", new Vector3(100, 0, 0)));
            net.Discover("far"); // only far discovered yet
            Assert.AreEqual("far", net.NearestDiscovered(Vector3.zero).Value.Id);
            net.Discover("near");
            Assert.AreEqual("near", net.NearestDiscovered(Vector3.zero).Value.Id);
        }

        [Test]
        public void DiscoveredRiftsSaveAndLoad()
        {
            var net = new FastTravelNetwork();
            net.Register(new LeylineRift("a", "A", Vector3.zero));
            net.Register(new LeylineRift("b", "B", Vector3.one));
            net.Discover("a");

            var reloaded = new FastTravelNetwork();
            reloaded.Register(new LeylineRift("a", "A", Vector3.zero));
            reloaded.Register(new LeylineRift("b", "B", Vector3.one));
            reloaded.LoadDiscovered(net.ToSave());
            Assert.IsTrue(reloaded.CanTravelTo("a"));
            Assert.IsFalse(reloaded.CanTravelTo("b"));
        }

        // ---- location sharing & locate ----
        [Test]
        public void LocationSharingIsOffByDefault()
        {
            var s = new LocationSharing();
            Assert.IsFalse(s.IsSharing("u1"));
            s.SetSharing("u1", true);
            Assert.IsTrue(s.IsSharing("u1"));
            s.SetSharing("u1", false);
            Assert.IsFalse(s.IsSharing("u1"));
        }

        [Test]
        public void SelfIsAlwaysLocated()
        {
            var m = Locator.Self("me", new Vector3(3, 0, 4));
            Assert.AreEqual(MapMarkerKind.Self, m.Kind);
            Assert.AreEqual("me", m.Id);
        }

        [Test]
        public void OnlyConsentingFriendsWithKnownPositionsAreShown()
        {
            var sharing = new LocationSharing();
            sharing.SetSharing("alice", true);   // consents, has a position
            sharing.SetSharing("carol", true);   // consents, but no position
            // bob does not consent
            var friends = new List<string> { "alice", "bob", "carol" };
            var positions = new Dictionary<string, Vector3>
            {
                { "alice", new Vector3(1, 0, 1) },
                { "bob", new Vector3(2, 0, 2) }
            };
            var markers = Locator.VisibleFriends(friends, sharing, positions);
            Assert.AreEqual(1, markers.Count);
            Assert.AreEqual("alice", markers[0].Id);
            Assert.AreEqual(MapMarkerKind.Friend, markers[0].Kind);
        }

        // ---- minimap math ----
        [Test]
        public void WorldMapsToNormalizedCorners()
        {
            var min = new Vector3(-50, 0, -50);
            var max = new Vector3(50, 0, 50);
            var atMin = Minimap.WorldToNormalized(min, min, max);
            var atMax = Minimap.WorldToNormalized(max, min, max);
            var atMid = Minimap.WorldToNormalized(Vector3.zero, min, max);
            Assert.AreEqual(0f, atMin.x, 0.0001f); Assert.AreEqual(0f, atMin.y, 0.0001f);
            Assert.AreEqual(1f, atMax.x, 0.0001f); Assert.AreEqual(1f, atMax.y, 0.0001f);
            Assert.AreEqual(0.5f, atMid.x, 0.0001f); Assert.AreEqual(0.5f, atMid.y, 0.0001f);
        }

        [Test]
        public void WithinRangeRingFiltersAndIgnoresHeight()
        {
            Assert.IsTrue(Minimap.WithinRange(Vector3.zero, 5f, new Vector3(3, 99, 4)));  // dist 5 on XZ, height ignored
            Assert.IsFalse(Minimap.WithinRange(Vector3.zero, 5f, new Vector3(10, 0, 0)));
        }

        // ---- canonical overworld (WorldMap) ----
        [Test]
        public void WorldMapRiftsAreUniqueAndIncludeCrystal()
        {
            Assert.Greater(WorldMap.Rifts.Count, 0);
            var ids = new HashSet<string>();
            foreach (var r in WorldMap.Rifts)
            {
                Assert.IsFalse(string.IsNullOrEmpty(r.Id));
                Assert.IsTrue(ids.Add(r.Id), "duplicate rift id: " + r.Id);
            }
            Assert.IsTrue(ids.Contains("crystal"));
        }

        [Test]
        public void BuildNetworkRegistersAllAndDiscoversNone()
        {
            var net = WorldMap.BuildNetwork();
            Assert.AreEqual(WorldMap.Rifts.Count, net.All.Count);
            foreach (var r in WorldMap.Rifts) Assert.IsFalse(net.IsDiscovered(r.Id));
            Assert.IsFalse(net.CanTravelTo("crystal"));   // can't travel to an undiscovered rift
            Assert.IsTrue(net.Discover("crystal"));
            Assert.IsTrue(net.CanTravelTo("crystal"));
        }

        [Test]
        public void CrystalNormalizesToMapCentre()
        {
            LeylineRift crystal = default;
            foreach (var r in WorldMap.Rifts) if (r.Id == "crystal") crystal = r;
            var n = Minimap.WorldToNormalized(crystal.World, WorldMap.BoundsMin, WorldMap.BoundsMax);
            Assert.AreEqual(0.5f, n.x, 0.001f);
            Assert.AreEqual(0.5f, n.y, 0.001f);
        }

        // ---- presence registry (the live friend-position feed) ----
        [Test]
        public void PresenceReturnsFreshPositionsAndExpiresStaleOnes()
        {
            var reg = new PresenceRegistry();
            reg.Report("a", new Vector3(1f, 0f, 2f), 100f);
            Assert.IsTrue(reg.TryGet("a", 102f, 5f, out var p));
            Assert.AreEqual(new Vector3(1f, 0f, 2f), p);
            Assert.IsFalse(reg.TryGet("a", 110f, 5f, out _)); // silent past the window -> gone
        }

        [Test]
        public void PresenceDropRemovesAndStalePrunesFromFresh()
        {
            var reg = new PresenceRegistry();
            reg.Report("a", Vector3.zero, 0f);
            reg.Report("b", Vector3.one, 0f);
            reg.Drop("a");
            var fresh = reg.Fresh(1f, 5f);
            Assert.IsFalse(fresh.ContainsKey("a"));
            Assert.IsTrue(fresh.ContainsKey("b"));
            Assert.AreEqual(0, reg.Fresh(100f, 5f).Count); // b now stale and pruned
        }

        [Test]
        public void FreshPresenceFeedsVisibleFriendsThroughTheConsentGate()
        {
            // The two pure pieces compose as MapState uses them: a fresh, consenting friend shows; a fresh
            // non-consenting one does not.
            var reg = new PresenceRegistry();
            reg.Report("f1", new Vector3(5f, 0f, 5f), 10f);
            reg.Report("f2", new Vector3(9f, 0f, 9f), 10f);
            var sharing = new LocationSharing();
            sharing.SetSharing("f1", true); // f2 left off
            var positions = reg.Fresh(10f, 5f);
            var markers = Locator.VisibleFriends(new List<string> { "f1", "f2" }, sharing, positions);
            Assert.AreEqual(1, markers.Count);
            Assert.AreEqual("f1", markers[0].Id);
        }

        [Test]
        public void PresenceCodecRoundTrips()
        {
            var p = new Vector3(12.34f, -5f, 67.8f);
            Assert.IsTrue(PresenceCodec.TryDecode(PresenceCodec.Encode(p), out var back));
            Assert.AreEqual(p.x, back.x, 0.01f);
            Assert.AreEqual(p.y, back.y, 0.01f);
            Assert.AreEqual(p.z, back.z, 0.01f);
        }

        [Test]
        public void PresenceCodecRejectsMalformedOrEmpty()
        {
            Assert.IsFalse(PresenceCodec.TryDecode("", out _));
            Assert.IsFalse(PresenceCodec.TryDecode(null, out _));
            Assert.IsFalse(PresenceCodec.TryDecode("1,2", out _));   // too few components
            Assert.IsFalse(PresenceCodec.TryDecode("a,b,c", out _)); // not numbers
        }
    }
}
