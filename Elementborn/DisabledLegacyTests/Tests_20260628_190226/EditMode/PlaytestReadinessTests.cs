using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class PlaytestReadinessTests
    {
        [SetUp]
        public void SetUp()
        {
            ElementbornEditModeTestUtility.ResetAll();
        }

        [TearDown]
        public void TearDown()
        {
            ElementbornEditModeTestUtility.ResetAll();
        }

        [Test]
        public void Scanner_WithMinimalRequiredScene_HasNoErrors()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player Test Rig";
            player.tag = "Player";

            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();

            GameObject loopGo = new GameObject("Loop");
            loopGo.AddComponent<ElementbornMainGameplayLoopDirector>();
            loopGo.AddComponent<ElementbornSpawnRegistry>();

            GameObject spawnGo = new GameObject("PlayerStart");
            ElementbornSpawnPoint spawn = spawnGo.AddComponent<ElementbornSpawnPoint>();
            spawn.Configure("player_start", ElementbornSpawnRole.PlayerStart);

            ElementbornTestReadinessReport report = new GameObject("Scanner").AddComponent<ElementbornTestReadinessScanner>().ScanCurrentScene();

            Assert.AreEqual(0, report.ErrorCount, report.ToMarkdown());
        }

        [Test]
        public void ResetService_ClearsCreatureOrphanageResidents()
        {
            CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                "reset_test_creature",
                "Reset Test Creature",
                CreatureOrphanageDepartureReason.RanAway,
                "test");

            Assert.NotNull(CreatureOrphanageRecoveryRegistry.Ensure().Find("reset_test_creature"));

            ElementbornPlaytestResetService.Ensure().ResetRuntimeState(deleteSaves: false);

            Assert.IsNull(CreatureOrphanageRecoveryRegistry.Ensure().Find("reset_test_creature"));
        }

        [Test]
        public void PresetService_CreatureRanAway_CreatesRecoverableResident()
        {
            ElementbornPlaytestPresetService.Ensure().ApplyPreset(ElementbornPlaytestPreset.CreatureRanAway);

            CreatureOrphanageResidentRecord resident = CreatureOrphanageRecoveryRegistry.Ensure().Find("preset_runaway_emberfox");
            Assert.NotNull(resident);
            Assert.AreEqual(CreatureOrphanageResidentState.AvailableToLureBack, resident.State);
        }

        [Test]
        public void Harness_TeleportFireCapital_MovesPlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player Test Rig";
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            GameObject spawnGo = new GameObject("FireCapitalStart");
            spawnGo.transform.position = new Vector3(-70f, 1f, -58f);
            ElementbornSpawnPoint spawn = spawnGo.AddComponent<ElementbornSpawnPoint>();
            spawn.Configure("fire_start", ElementbornSpawnRole.FireCapitalStart);

            ElementbornPlaytestHarnessController.Ensure().TeleportFireCapital();

            Assert.AreEqual(spawnGo.transform.position, player.transform.position);
        }
    }
}
