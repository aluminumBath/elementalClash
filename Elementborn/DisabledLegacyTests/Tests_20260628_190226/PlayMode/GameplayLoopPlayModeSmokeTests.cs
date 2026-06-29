using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Elementborn.Game;

namespace Elementborn.Tests.PlayMode
{
    public class GameplayLoopPlayModeSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            ElementbornPlayModeTestUtility.CleanupScene();
        }

        [UnityTest]
        public IEnumerator GameplayLoop_StartGame_ReachesExploreAndCreatesPlayer()
        {
            GameObject spawnGo = new GameObject("PlayerStart");
            spawnGo.transform.position = new Vector3(2f, 1f, 3f);
            ElementbornSpawnPoint spawn = spawnGo.AddComponent<ElementbornSpawnPoint>();
            spawn.Configure("player_start", ElementbornSpawnRole.PlayerStart);

            GameObject directorGo = new GameObject("GameplayLoopDirector");
            ElementbornSpawnRegistry registry = directorGo.AddComponent<ElementbornSpawnRegistry>();
            ElementbornMainGameplayLoopDirector director = directorGo.AddComponent<ElementbornMainGameplayLoopDirector>();
            ElementbornPlayModeTestUtility.SetPrivate(director, "startOnAwake", false);
            ElementbornPlayModeTestUtility.SetPrivate(director, "registerSystemsOnStart", false);
            ElementbornPlayModeTestUtility.SetPrivate(director, "startIntroQuestOnStart", false);

            yield return null;
            director.StartGame();
            yield return null;

            Assert.AreEqual(ElementbornGameplayLoopState.Explore, director.State);
            GameObject player = null;
            try { player = GameObject.FindGameObjectWithTag("Player"); } catch { player = GameObject.Find("Player Test Rig"); }
            Assert.NotNull(player);
            Assert.AreEqual(spawnGo.transform.position, player.transform.position);
            Assert.IsTrue(registry.SpawnPoints.Count >= 1);
        }

        [UnityTest]
        public IEnumerator GameplayLoop_SpawnWave_CreatesSpawnedObjects()
        {
            GameObject pointGo = new GameObject("EnemyWave");
            pointGo.transform.position = Vector3.zero;
            ElementbornSpawnPoint point = pointGo.AddComponent<ElementbornSpawnPoint>();
            point.Configure("enemy_wave", ElementbornSpawnRole.EnemyWave);

            GameObject enemyPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyPrefab.name = "TestEnemyPrefab";
            enemyPrefab.SetActive(false);

            ElementbornSpawnWaveDefinition wave = ScriptableObject.CreateInstance<ElementbornSpawnWaveDefinition>();
            var entry = new ElementbornSpawnWaveEntry
            {
                EntryId = "test_enemy",
                Prefab = enemyPrefab,
                SpawnRole = ElementbornSpawnRole.EnemyWave,
                Count = 2,
                Radius = 0f
            };
            ElementbornPlayModeTestUtility.SetPrivate(wave, "waveId", "test_wave");
            ElementbornPlayModeTestUtility.SetPrivate(wave, "displayName", "Test Wave");
            ElementbornPlayModeTestUtility.SetPrivate(wave, "entries", new System.Collections.Generic.List<ElementbornSpawnWaveEntry> { entry });

            GameObject directorGo = new GameObject("GameplayLoopDirector");
            ElementbornSpawnRegistry registry = directorGo.AddComponent<ElementbornSpawnRegistry>();
            ElementbornMainGameplayLoopDirector director = directorGo.AddComponent<ElementbornMainGameplayLoopDirector>();
            ElementbornPlayModeTestUtility.SetPrivate(director, "startOnAwake", false);

            yield return null;
            director.SpawnWave(wave);
            yield return null;

#if UNITY_2023_1_OR_NEWER
            GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
#endif
            int spawned = 0;
            foreach (GameObject go in all)
            {
                if (go != null && go.name.Contains("test_enemy_Spawned"))
                {
                    spawned++;
                }
            }
            Assert.GreaterOrEqual(spawned, 2);
        }
    }
}
