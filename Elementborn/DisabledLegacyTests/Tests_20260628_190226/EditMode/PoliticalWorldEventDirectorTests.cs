
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class PoliticalWorldEventDirectorTests
    {
        [SetUp]
        public void SetUp() => ElementbornEditModeTestUtility.ResetAll();

        [TearDown]
        public void TearDown() => ElementbornEditModeTestUtility.ResetAll();

        [Test]
        public void EvaluateThenActivate_TransitionsEventAndAppliesConsequence()
        {
            CapitalWorldStateTracker tracker = new GameObject("CapitalWorldStateTracker").AddComponent<CapitalWorldStateTracker>();
            tracker.AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, 60, "Setup");

            var definition = ScriptableObject.CreateInstance<PoliticalWorldEventDefinition>();
            ElementbornEditModeTestUtility.SetPrivate(definition, "eventId", "test_fire_event");
            ElementbornEditModeTestUtility.SetPrivate(definition, "displayName", "Test Fire Event");
            ElementbornEditModeTestUtility.SetPrivate(definition, "primaryCapital", CapitalId.FireCapital);
            ElementbornEditModeTestUtility.SetPrivate(definition, "worldPosition", new Vector3(3f, 0f, 4f));
            ElementbornEditModeTestUtility.SetPrivate(definition, "playerFacingSummary", "Summary");
            ElementbornEditModeTestUtility.SetPrivate(definition, "hiddenDirectorNotes", "Hidden");
            ElementbornEditModeTestUtility.SetPrivate(definition, "createMapMarker", true);
            ElementbornEditModeTestUtility.SetPrivate(definition, "conditions", new List<PoliticalWorldEventCondition>
            {
                new PoliticalWorldEventCondition { Capital = CapitalId.FireCapital, PressureType = CapitalPressureType.Unrest, MinimumValue = 50, RequireAtOrAbove = true }
            });
            ElementbornEditModeTestUtility.SetPrivate(definition, "consequences", new List<PoliticalWorldEventConsequence>
            {
                new PoliticalWorldEventConsequence { PressureType = CapitalPressureType.Unrest, PressureDelta = -10, Reason = "Cooling things down" }
            });

            PoliticalWorldEventDirector director = new GameObject("PoliticalWorldEventDirector").AddComponent<PoliticalWorldEventDirector>();
            director.SetEvents(new List<PoliticalWorldEventDefinition> { definition });
            director.EvaluateAll();
            Assert.AreEqual(PoliticalWorldEventState.Eligible, director.GetOrCreateRecord("test_fire_event").State);

            Assert.IsTrue(director.Activate("test_fire_event", "unit test"));
            Assert.AreEqual(PoliticalWorldEventState.Active, director.GetOrCreateRecord("test_fire_event").State);
            Assert.Less(tracker.GetOrCreate(CapitalId.FireCapital).GetOrCreatePressure(CapitalPressureType.Unrest).Value, 60);
        }
    }
}
