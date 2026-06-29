
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class TimedDualLeaderPackRespawnControllerTests
    {
        [SetUp]
        public void SetUp() => ElementbornEditModeTestUtility.ResetAll();

        [TearDown]
        public void TearDown() => ElementbornEditModeTestUtility.ResetAll();

        [Test]
        public void DualLeaderDefeat_MarksPackAsDefeated()
        {
            GameObject root = new GameObject("WolfPack");
            GameObject leaderA = new GameObject("Romilus");
            GameObject leaderB = new GameObject("Madrangea");
            leaderA.transform.SetParent(root.transform);
            leaderB.transform.SetParent(root.transform);

            TimedDualLeaderPackRespawnController controller = root.AddComponent<TimedDualLeaderPackRespawnController>();
            ElementbornEditModeTestUtility.SetPrivate(controller, "packId", "romilus_madrangea_pack");
            ElementbornEditModeTestUtility.SetPrivate(controller, "leaderA", leaderA);
            ElementbornEditModeTestUtility.SetPrivate(controller, "leaderB", leaderB);
            ElementbornEditModeTestUtility.SetPrivate(controller, "packMembers", new List<GameObject>());
            ElementbornEditModeTestUtility.SetPrivate(controller, "respawnPoints", new List<Transform>());

            controller.NotifyRomilusDefeated();
            controller.NotifyMadrangeaDefeated();

            Assert.IsTrue(controller.PackDefeated);
            Assert.AreEqual(StoryEncounterState.Resolved, StoryEncounterProgressTracker.Ensure().GetOrCreate("romilus_madrangea_pack").State);
        }

        [Test]
        public void ForceRespawnPack_ReactivatesPackMembers()
        {
            GameObject root = new GameObject("WolfPack");
            GameObject leaderA = new GameObject("Romilus");
            GameObject leaderB = new GameObject("Madrangea");
            GameObject member = new GameObject("Wolf");
            leaderA.SetActive(false);
            leaderB.SetActive(false);
            member.SetActive(false);
            leaderA.transform.SetParent(root.transform);
            leaderB.transform.SetParent(root.transform);
            member.transform.SetParent(root.transform);
            GameObject point = new GameObject("Point");
            point.transform.position = new Vector3(9f, 0f, 2f);

            TimedDualLeaderPackRespawnController controller = root.AddComponent<TimedDualLeaderPackRespawnController>();
            ElementbornEditModeTestUtility.SetPrivate(controller, "packMembers", new List<GameObject> { member });
            ElementbornEditModeTestUtility.SetPrivate(controller, "respawnPoints", new List<Transform> { point.transform });
            ElementbornEditModeTestUtility.SetPrivate(controller, "leaderA", leaderA);
            ElementbornEditModeTestUtility.SetPrivate(controller, "leaderB", leaderB);

            controller.ForceRespawnPack();
            Assert.IsTrue(leaderA.activeSelf);
            Assert.IsTrue(leaderB.activeSelf);
            Assert.IsTrue(member.activeSelf);
            Assert.AreEqual(point.transform.position, member.transform.position);
            Assert.AreEqual(StoryEncounterState.Respawning, StoryEncounterProgressTracker.Ensure().GetOrCreate("romilus_madrangea_pack").State);
        }
    }
}
