using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Elementborn.Game;

namespace Elementborn.Tests.PlayMode
{
    public class FireCapitalPlayModeSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            ElementbornPlayModeTestUtility.CleanupScene();
        }

        [UnityTest]
        public IEnumerator FireCapitalHook_StartAndResolve_UpdatesRecord()
        {
            FireCapitalCourtHookDefinition hook = ScriptableObject.CreateInstance<FireCapitalCourtHookDefinition>();
            ElementbornPlayModeTestUtility.SetPrivate(hook, "hookId", "test_fire_hook");
            ElementbornPlayModeTestUtility.SetPrivate(hook, "title", "Test Fire Hook");
            ElementbornPlayModeTestUtility.SetPrivate(hook, "hookType", FireCapitalHookType.VolcanoPressure);
            ElementbornPlayModeTestUtility.SetPrivate(hook, "district", FireCapitalDistrict.CalderaThrone);
            ElementbornPlayModeTestUtility.SetPrivate(hook, "worldPosition", Vector3.zero);
            ElementbornPlayModeTestUtility.SetPrivate(hook, "pressureType", CapitalPressureType.HiddenThreat);
            ElementbornPlayModeTestUtility.SetPrivate(hook, "pressureDeltaOnStart", 5);
            ElementbornPlayModeTestUtility.SetPrivate(hook, "stabilityDeltaOnResolve", 5);
            ElementbornPlayModeTestUtility.SetPrivate(hook, "playerFacingSummary", "Test summary.");

            GameObject registryGo = new GameObject("FireCapitalRegistry");
            FireCapitalRegistry registry = registryGo.AddComponent<FireCapitalRegistry>();
            registry.SetHooks(new List<FireCapitalCourtHookDefinition> { hook });

            yield return null;
            registry.StartHook("test_fire_hook");
            yield return null;

            FireCapitalRuntimeRecord record = registry.GetOrCreateRecord("test_fire_hook");
            Assert.IsTrue(record.Started);
            Assert.AreEqual(1, record.TimesStarted);

            registry.ResolveHook("test_fire_hook", "playmode");
            yield return null;

            Assert.IsTrue(record.Resolved);
        }

        [UnityTest]
        public IEnumerator VolcanoHazard_PulseAndCalm_DoNotThrow()
        {
            GameObject go = new GameObject("Volcano");
            FireCapitalVolcanoHazardController volcano = go.AddComponent<FireCapitalVolcanoHazardController>();

            yield return null;
            volcano.PulseVolcanoPressure();
            volcano.CalmVolcano();
            yield return null;

            CapitalRuntimeState fire = CapitalWorldStateTracker.Ensure().GetOrCreate(CapitalId.FireCapital);
            Assert.NotNull(fire);
        }
    }
}
