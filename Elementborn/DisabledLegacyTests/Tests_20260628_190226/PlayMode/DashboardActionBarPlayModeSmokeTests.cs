using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Elementborn.Game;

namespace Elementborn.Tests.PlayMode
{
    public class DashboardActionBarPlayModeSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            ElementbornPlayModeTestUtility.CleanupScene();
        }

        [UnityTest]
        public IEnumerator DashboardButtons_InvokeCoreActions()
        {
            GameObject canvas = new GameObject("Dashboard");
            canvas.SetActive(false);
            StorySystemsDebugDashboard dashboard = canvas.AddComponent<StorySystemsDebugDashboard>();
            StorySystemsDebugDashboardActionBar bar = canvas.AddComponent<StorySystemsDebugDashboardActionBar>();

            Button startButton = new GameObject("StartLoopButton").AddComponent<Button>();
            Button waveButton = new GameObject("SpawnWaveButton").AddComponent<Button>();
            Button volcanoButton = new GameObject("VolcanoButton").AddComponent<Button>();
            startButton.transform.SetParent(canvas.transform);
            waveButton.transform.SetParent(canvas.transform);
            volcanoButton.transform.SetParent(canvas.transform);

            ElementbornPlayModeTestUtility.SetPrivate(bar, "dashboard", dashboard);
            ElementbornPlayModeTestUtility.SetPrivate(bar, "startLoopButton", startButton);
            ElementbornPlayModeTestUtility.SetPrivate(bar, "spawnWaveButton", waveButton);
            ElementbornPlayModeTestUtility.SetPrivate(bar, "fireVolcanoButton", volcanoButton);

            GameObject spawnGo = new GameObject("PlayerStart");
            ElementbornSpawnPoint spawn = spawnGo.AddComponent<ElementbornSpawnPoint>();
            spawn.Configure("player_start", ElementbornSpawnRole.PlayerStart);

            canvas.SetActive(true);
            yield return null;

            startButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(ElementbornGameplayLoopState.Explore, ElementbornMainGameplayLoopDirector.Ensure().State);

            volcanoButton.onClick.Invoke();
            yield return null;
            Assert.NotNull(CapitalWorldStateTracker.Ensure().GetOrCreate(CapitalId.FireCapital));
        }
    }
}
