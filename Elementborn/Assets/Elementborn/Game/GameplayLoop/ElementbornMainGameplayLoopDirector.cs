using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornMainGameplayLoopDirector : MonoBehaviour
    {
        public static ElementbornMainGameplayLoopDirector Instance { get; private set; }

        [SerializeField] private ElementbornGameplayLoopState state = ElementbornGameplayLoopState.NotStarted;
        [SerializeField] private bool startOnAwake = false;
        [SerializeField] private bool registerSystemsOnStart = true;
        [SerializeField] private bool startIntroQuestOnStart = true;
        [SerializeField] private QuestUiDefinition introQuest;
        [SerializeField] private List<ElementbornSpawnWaveDefinition> starterWaves = new List<ElementbornSpawnWaveDefinition>();
        [SerializeField] private Transform spawnedParent;
        [SerializeField] private GameObject fallbackPlayerPrefab;
        [SerializeField] private GameObject fallbackEnemyPrefab;

        public ElementbornGameplayLoopState State => state;
        public IReadOnlyList<ElementbornSpawnWaveDefinition> StarterWaves => starterWaves;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (startOnAwake)
            {
                StartGame();
            }
        }

        public static ElementbornMainGameplayLoopDirector Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(ElementbornMainGameplayLoopDirector));
            return go.AddComponent<ElementbornMainGameplayLoopDirector>();
        }

        [ContextMenu("Start Game")]
        public void StartGame()
        {
            SetState(ElementbornGameplayLoopState.Bootstrapping);
            EnsureCoreSystems();

            if (registerSystemsOnStart)
            {
                RegisterWorldAnchors();
            }

            SpawnOrMovePlayer();

            if (startIntroQuestOnStart && introQuest != null)
            {
                QuestUiTracker.StartQuest(introQuest);
            }

            SpawnStarterWaves();
            SetState(ElementbornGameplayLoopState.Explore);
            NotificationFeed.Post("Elementborn playable loop started.", NotificationType.Quest);
        }

        [ContextMenu("Spawn Starter Waves")]
        public void SpawnStarterWaves()
        {
            foreach (ElementbornSpawnWaveDefinition wave in starterWaves)
            {
                SpawnWave(wave);
            }
        }

        public void SpawnWave(ElementbornSpawnWaveDefinition wave)
        {
            if (wave == null)
            {
                return;
            }

            foreach (ElementbornSpawnWaveEntry entry in wave.Entries)
            {
                if (entry == null)
                {
                    continue;
                }

                List<ElementbornSpawnPoint> points = ElementbornSpawnRegistry.Ensure().FindAll(entry.SpawnRole);
                if (points.Count == 0)
                {
                    points.Add(ElementbornSpawnRegistry.Ensure().FindFirst(ElementbornSpawnRole.EnemyWave));
                }

                for (int i = 0; i < Mathf.Max(1, entry.Count); i++)
                {
                    Vector3 position = Vector3.zero;
                    Quaternion rotation = Quaternion.identity;
                    if (points.Count > 0 && points[i % points.Count] != null)
                    {
                        Transform point = points[i % points.Count].transform;
                        Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, entry.Radius);
                        position = point.position + new Vector3(offset.x, 0f, offset.y);
                        rotation = point.rotation;
                    }

                    GameObject prefab = entry.Prefab != null ? entry.Prefab : fallbackEnemyPrefab;
                    GameObject spawned = prefab != null
                        ? Instantiate(prefab, position, rotation)
                        : GameObject.CreatePrimitive(PrimitiveType.Capsule);

                    spawned.name = string.IsNullOrWhiteSpace(entry.EntryId)
                        ? wave.DisplayName + "_Spawned"
                        : entry.EntryId + "_Spawned";

                    spawned.transform.position = position;
                    spawned.transform.rotation = rotation;
                    if (entry.ParentToDirector)
                    {
                        if (spawnedParent == null)
                        {
                            GameObject parent = GameObject.Find("Spawned Gameplay Objects") ?? new GameObject("Spawned Gameplay Objects");
                            spawnedParent = parent.transform;
                        }
                        spawned.transform.SetParent(spawnedParent, true);
                    }
                }
            }

            NotificationFeed.Post($"Spawned wave: {wave.DisplayName}", NotificationType.Info);
        }

        public void EnterEncounter()
        {
            SetState(ElementbornGameplayLoopState.Encounter);
        }

        public void EnterRecovery()
        {
            SetState(ElementbornGameplayLoopState.Recovery);
        }

        public void ReturnToExplore()
        {
            SetState(ElementbornGameplayLoopState.Explore);
        }

        public void CompleteLoop()
        {
            SetState(ElementbornGameplayLoopState.Completed);
            NotificationFeed.Post("Playable loop completed.", NotificationType.Quest);
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Main Gameplay Loop: {state}");
            sb.AppendLine($"Starter waves: {starterWaves.Count}");
            sb.AppendLine(ElementbornSpawnRegistry.Ensure().BuildSummary());
            return sb.ToString();
        }

        private void SetState(ElementbornGameplayLoopState next)
        {
            state = next;
        }

        private void EnsureCoreSystems()
        {
            ElementbornRuntimeBootstrap.EnsureSingleton<PlayerInventoryTracker>("PlayerInventoryTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<PlayerMapMarkerTracker>("PlayerMapMarkerTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<WaypointTracker>("WaypointTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<NotificationFeed>("NotificationFeed");
            ElementbornRuntimeBootstrap.EnsureSingleton<PlayerJournalTracker>("PlayerJournalTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<QuestUiTracker>("QuestUiTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<CapitalWorldStateTracker>("CapitalWorldStateTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<PoliticalWorldEventDirector>("PoliticalWorldEventDirector");
            ElementbornRuntimeBootstrap.EnsureSingleton<QuestChainDirector>("QuestChainDirector");
            ElementbornRuntimeBootstrap.EnsureSingleton<SocialGroupRegistry>("SocialGroupRegistry");
            ElementbornRuntimeBootstrap.EnsureSingleton<StoryEncounterProgressTracker>("StoryEncounterProgressTracker");
            ElementbornRuntimeBootstrap.EnsureSingleton<CreatureOrphanageRecoveryRegistry>("CreatureOrphanageRecoveryRegistry");
            ElementbornRuntimeBootstrap.EnsureSingleton<ElementbornSpawnRegistry>("ElementbornSpawnRegistry");
        }

        private void RegisterWorldAnchors()
        {
            foreach (CapitalLandmarkDescriptor descriptor in ElementbornFindUtility.FindAll<CapitalLandmarkDescriptor>())
            {
                descriptor.Register();
            }

            SocialGroupRegistry.Ensure().RegisterJournalAndMap();
            CapitalWorldStateTracker.Ensure().RegisterJournalAndMap();
            NpcWorldIntegrationManager manager = ElementbornFindUtility.FindFirst<NpcWorldIntegrationManager>();
            if (manager != null)
            {
                manager.RegisterAll();
            }
        }

        private void SpawnOrMovePlayer()
        {
            ElementbornSpawnPoint playerSpawn = ElementbornSpawnRegistry.Ensure().FindFirst(ElementbornSpawnRole.PlayerStart);
            if (playerSpawn == null)
            {
                playerSpawn = ElementbornSpawnRegistry.Ensure().FindFirst(ElementbornSpawnRole.FireCapitalStart);
            }

            GameObject player = null;
            try
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            catch
            {
                player = null;
            }

            if (player == null)
            {
                player = fallbackPlayerPrefab != null
                    ? Instantiate(fallbackPlayerPrefab)
                    : GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player Test Rig";
                try { player.tag = "Player"; } catch { }
                if (player.GetComponent<PlayerTestRigSetup>() == null)
                {
                    player.AddComponent<PlayerTestRigSetup>().Configure();
                }
            }

            if (playerSpawn != null)
            {
                player.transform.position = playerSpawn.transform.position;
                player.transform.rotation = playerSpawn.transform.rotation;
            }
        }
    }
}
