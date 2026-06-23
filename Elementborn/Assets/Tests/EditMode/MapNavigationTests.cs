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
    }
}
