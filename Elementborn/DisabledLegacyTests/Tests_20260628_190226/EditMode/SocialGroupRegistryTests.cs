
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class SocialGroupRegistryTests
    {
        [SetUp]
        public void SetUp() => ElementbornEditModeTestUtility.ResetAll();

        [TearDown]
        public void TearDown() => ElementbornEditModeTestUtility.ResetAll();

        [Test]
        public void ActivateEvent_UpdatesRuntimeAndJournal()
        {
            var group = ScriptableObject.CreateInstance<SocialNpcGroupDefinition>();
            ElementbornEditModeTestUtility.SetPrivate(group, "groupId", "fire_neighbors");
            ElementbornEditModeTestUtility.SetPrivate(group, "displayName", "Fire Neighbors");
            ElementbornEditModeTestUtility.SetPrivate(group, "primaryCapital", CapitalId.FireCapital);
            ElementbornEditModeTestUtility.SetPrivate(group, "groupCenter", new Vector3(1f, 0f, 2f));
            ElementbornEditModeTestUtility.SetPrivate(group, "summary", "Neighborhood watch and mutual aid.");

            var evt = ScriptableObject.CreateInstance<SocialGroupEventDefinition>();
            ElementbornEditModeTestUtility.SetPrivate(evt, "eventId", "kelly_watch");
            ElementbornEditModeTestUtility.SetPrivate(evt, "displayName", "Kelly's Watch");
            ElementbornEditModeTestUtility.SetPrivate(evt, "eventType", SocialGroupEventType.NeighborhoodProtection);
            ElementbornEditModeTestUtility.SetPrivate(evt, "group", group);
            ElementbornEditModeTestUtility.SetPrivate(evt, "pressureType", CapitalPressureType.Unrest);
            ElementbornEditModeTestUtility.SetPrivate(evt, "pressureDelta", -5);
            ElementbornEditModeTestUtility.SetPrivate(evt, "playerFacingSummary", "Kelly has calmed the street.");
            ElementbornEditModeTestUtility.SetPrivate(evt, "directorNotes", "Test event.");

            SocialGroupRegistry registry = new GameObject("SocialGroupRegistry").AddComponent<SocialGroupRegistry>();
            registry.SetData(new List<SocialNpcGroupDefinition> { group }, new List<SocialGroupEventDefinition> { evt });
            registry.RegisterJournalAndMap();
            registry.ActivateEvent("kelly_watch");

            SocialGroupRuntimeRecord record = registry.GetOrCreateRecord("fire_neighbors");
            Assert.AreEqual("kelly_watch", record.LastEventId);
            Assert.Greater(record.NeighborhoodTrust, 0);
            Assert.NotNull(PlayerJournalTracker.Find("social_group_event_kelly_watch"));
            Assert.IsTrue(PlayerMapMarkerTracker.GetAllMarkers().Any(m => m.MarkerId == "social_group_event_kelly_watch"));
        }
    }
}
