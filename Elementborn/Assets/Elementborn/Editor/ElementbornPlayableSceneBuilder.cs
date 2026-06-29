
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPlayableSceneBuilder
    {
        private const string GeneratedSceneDir = "Assets/Elementborn/Generated/Scenes";

        [MenuItem("Elementborn/Playable Setup/Build Full Test Scene")]
        public static void BuildFullTestScene()
        {
            BuildRoundedPlayableScene();
        }

        [MenuItem("Elementborn/Playable Setup/Build Rounded Playable Scene")]
        public static void BuildRoundedPlayableScene()
        {
            Directory.CreateDirectory(GeneratedSceneDir);
            GenerateContentForPlayableScene();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLightingAndCamera();
            CreateWorldGround();
            CreateRuntimeSystems();
            InstallGeneratedSystems();
            CreateCapitalLandmarks();
            PlayableSceneProductionPolishBuilder.ApplyProductionPolish();
            GameObject player = CreatePlayerTestRig();
            player.transform.position = new Vector3(0f, 1f, -6f);
            CreateQuestCombatUiCanvas();
            CreateOrphanageDemo();
            CreateWolfPackEncounter();
            CreateTestEnemy();
            CreateBossArena();
            CreateBoatTestSetup();
            CreateChecklistObject();

            string path = $"{GeneratedSceneDir}/Elementborn_Playable_Test.unity";
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();
            Debug.Log($"Elementborn rounded playable scene built at {path}");
        }

        [MenuItem("Elementborn/Playable Setup/Generate Content For Playable Scene")]
        public static void GenerateContentForPlayableScene()
        {
            FireCapitalRoyalFamilyGenerator.GenerateAll();
            CapitalLandmarkPrefabGenerator.GenerateAll();
            NpcRosterCsvImporter.ImportAllRosters();
            FireCapitalAssetGenerator.GenerateAll();
            GameplayLoopAssetGenerator.GenerateAll();
            CapitalWorldStateAssetGenerator.GenerateAll();
            PoliticalWorldEventAssetGenerator.GenerateAll();
            QuestChainAssetGenerator.GenerateAll();
            SocialGroupAssetGenerator.GenerateAll();
            StoryEncounterAssetGenerator.GenerateAll();
            SocialNpcQuestAndPrefabGenerator.GenerateQuestsAndDialogue();
            SocialNpcQuestAndPrefabGenerator.CreatePlaceholderPrefabs();
            WindCapitalAssetGenerator.GenerateAll();
            MetalCapitalAssetGenerator.GenerateAll();
            NpcWorldIntegrationAssetGenerator.GenerateAll();
        }

        [MenuItem("Elementborn/Playable Setup/Create Runtime Systems Bootstrap")]
        public static void CreateRuntimeSystems()
        {
            GameObject go = GameObject.Find("Elementborn Runtime Systems");
            if (go == null) go = new GameObject("Elementborn Runtime Systems");
            var bootstrap = Ensure<ElementbornRuntimeBootstrap>(go);
            bootstrap.EnsureRuntimeSystems();
            Ensure<QuestUiSaveBridge>(go);
            Ensure<InventorySaveBridge>(go);
            Ensure<MapMarkerSaveBridge>(go);
            Ensure<JournalSaveBridge>(go);
            Ensure<FactionReputationSaveBridge>(go);
            Ensure<SpellCastingSaveBridge>(go);
            Ensure<BossSaveBridge>(go);
            Ensure<EnemyAiSaveBridge>(go);
            Ensure<RecipeBookSaveBridge>(go);
            Ensure<CreatureBondingSaveBridge>(go);
            Ensure<AbilitySaveBridge>(go);
            Ensure<EquipmentSaveBridge>(go);
        }

        [MenuItem("Elementborn/Playable Setup/Install Generated Game Systems In Open Scene")]
        public static void InstallGeneratedSystems()
        {
            FireCapitalAssetGenerator.InstallSystems();
            GameplayLoopAssetGenerator.InstallGameplayLoopInOpenScene();
            CapitalWorldStateAssetGenerator.InstallSystems();
            PoliticalWorldEventAssetGenerator.InstallDirector();
            QuestChainAssetGenerator.InstallDirector();
            SocialGroupAssetGenerator.InstallRegistry();
            StoryEncounterAssetGenerator.InstallRegistry();
            WindCapitalAssetGenerator.InstallSystems();
            MetalCapitalAssetGenerator.InstallSystems();
            NpcWorldIntegrationAssetGenerator.InstallManager();
            SocialNpcQuestAndPrefabGenerator.InstallPlaceholders();
            NarrativeRuntimeSaveSetupMenu.InstallNarrativeRuntimeSaveBridges();
            StorySystemsDebugDashboardSetupMenu.InstallDashboard();
            AdminWristUiSetupMenu.InstallInOpenScene();
            ElementbornTestReadinessSetupMenu.CreateOnboardingQuest();
            ElementbornTestReadinessSetupMenu.InstallTestHarnessInOpenScene();
            ElementbornTestReadinessSetupMenu.WriteEditorReadinessReport();
            ConfigureNpcIntegrationForPlaytests();
        }

        [MenuItem("Elementborn/Playable Setup/Create Player Test Rig")]
        public static GameObject CreatePlayerTestRig()
        {
            GameObject player = null;
            try { player = GameObject.FindGameObjectWithTag("Player"); } catch { player = null; }
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player Test Rig";
                player.tag = "Player";
                player.transform.position = new Vector3(0f, 1f, 0f);
            }

            var setup = Ensure<PlayerTestRigSetup>(player);
            setup.Configure();
            return player;
        }

        [MenuItem("Elementborn/Playable Setup/Create Quest/Combat UI Canvas")]
        public static void CreateQuestCombatUiCanvas()
        {
            BootstrapUiFactory.BuildDefaultCanvas();
        }

        [MenuItem("Elementborn/Playable Setup/Create Test Enemy")]
        public static void CreateTestEnemy()
        {
            GameObject spawner = new GameObject("Test Enemy Spawn Point");
            spawner.transform.position = new Vector3(18f, 1f, 8f);
            var spawn = spawner.AddComponent<TestEnemySpawnPoint>();
            spawn.Spawn();
        }

        [MenuItem("Elementborn/Playable Setup/Create Test Boss Arena")]
        public static void CreateBossArena()
        {
            GameObject builder = new GameObject("Test Boss Arena Builder");
            builder.transform.position = new Vector3(36f, 0f, 0f);
            builder.AddComponent<TestBossArenaSetup>().Build();
        }

        [MenuItem("Elementborn/Playable Setup/Create Boat Test Setup")]
        public static void CreateBoatTestSetup()
        {
            GameObject builder = new GameObject("Test Boat Builder");
            builder.transform.position = new Vector3(-24f, 0f, 24f);
            builder.AddComponent<TestBoatSetupHelper>().BuildBoat();
        }

        [MenuItem("Elementborn/Playable Setup/Create Starter Scene Checklist")]
        public static void CreateChecklistObject()
        {
            GameObject go = GameObject.Find("Playable Scene Checklist");
            if (go == null) go = new GameObject("Playable Scene Checklist");
            var checklist = Ensure<StarterSceneChecklistGenerator>(go);
            checklist.SeedDefaultItems();
            checklist.WriteChecklistMarkdown();
        }

        [MenuItem("Elementborn/Playable Setup/Install Capital Landmarks In Open Scene")]
        public static void CreateCapitalLandmarks()
        {
            CapitalLandmarkPrefabGenerator.InstallInOpenScene();
        }

        [MenuItem("Elementborn/Playable Setup/Create Orphanage Demo")]
        public static void CreateOrphanageDemo()
        {
            GameObject root = new GameObject("Crab-Sign Creature Orphanage");
            root.transform.position = new Vector3(-8f, 0f, -16f);
            CreatePrimitive(root.transform, PrimitiveType.Cube, "Building", new Vector3(0f, 2f, 0f), new Vector3(8f, 4f, 6f), new Color(0.55f, 0.38f, 0.22f));
            CreatePrimitive(root.transform, PrimitiveType.Cube, "Roof", new Vector3(0f, 4.8f, 0f), new Vector3(9f, 1f, 7f), new Color(0.45f, 0.15f, 0.12f));
            CreatePrimitive(root.transform, PrimitiveType.Cylinder, "CrabSign", new Vector3(0f, 3.5f, 3.3f), new Vector3(0.8f, 0.2f, 0.8f), new Color(0.95f, 0.4f, 0.28f));

            GameObject heal = new GameObject("Orphanage Healing Trigger");
            heal.transform.SetParent(root.transform, false);
            heal.transform.localPosition = new Vector3(-2f, 0f, 4f);
            heal.AddComponent<CreatureOrphanageInteractable>();

            GameObject recovery = new GameObject("Orphanage Recovery Ledger");
            recovery.transform.SetParent(root.transform, false);
            recovery.transform.localPosition = new Vector3(2f, 0f, 4f);
            recovery.AddComponent<CreatureOrphanageRecoveryInteractable>();
        }

        [MenuItem("Elementborn/Playable Setup/Create Wolf Pack Encounter Demo")]
        public static void CreateWolfPackEncounter()
        {
            GameObject root = new GameObject("Romilus Madrangea Pack");
            root.transform.position = new Vector3(22f, 0f, -18f);

            GameObject leaderA = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leaderA.name = "Romilus";
            leaderA.transform.SetParent(root.transform, false);
            leaderA.transform.localPosition = new Vector3(-2f, 1f, 0f);

            GameObject leaderB = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leaderB.name = "Madrangea";
            leaderB.transform.SetParent(root.transform, false);
            leaderB.transform.localPosition = new Vector3(2f, 1f, 0f);

            GameObject memberA = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            memberA.name = "PackMember_A";
            memberA.transform.SetParent(root.transform, false);
            memberA.transform.localPosition = new Vector3(-5f, 1f, 2f);

            GameObject memberB = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            memberB.name = "PackMember_B";
            memberB.transform.SetParent(root.transform, false);
            memberB.transform.localPosition = new Vector3(5f, 1f, 2f);

            GameObject respawnA = new GameObject("RespawnPoint_A");
            respawnA.transform.SetParent(root.transform, false);
            respawnA.transform.localPosition = memberA.transform.localPosition;
            GameObject respawnB = new GameObject("RespawnPoint_B");
            respawnB.transform.SetParent(root.transform, false);
            respawnB.transform.localPosition = memberB.transform.localPosition;

            TimedDualLeaderPackRespawnController controller = root.AddComponent<TimedDualLeaderPackRespawnController>();
            SetPrivate(controller, "packId", "romilus_madrangea_pack");
            SetPrivate(controller, "leaderA", leaderA);
            SetPrivate(controller, "leaderB", leaderB);
            SetPrivate(controller, "defeatWindowSeconds", 300f);
            SetPrivate(controller, "respawnDelaySeconds", 5f);
            SetPrivate(controller, "respawnIfOnlyOneLeaderFalls", true);
            SetPrivate(controller, "packMembers", new System.Collections.Generic.List<GameObject> { memberA, memberB });
            SetPrivate(controller, "respawnPoints", new System.Collections.Generic.List<Transform> { respawnA.transform, respawnB.transform });
        }

        private static void CreateWorldGround()
        {
            CreatePrimitive(null, PrimitiveType.Plane, "Ground", new Vector3(0f, 0f, 0f), new Vector3(18f, 1f, 18f), new Color(0.28f, 0.38f, 0.26f));
            CreatePrimitive(null, PrimitiveType.Plane, "Water", new Vector3(0f, -0.05f, 0f), new Vector3(6f, 1f, 6f), new Color(0.12f, 0.34f, 0.62f));
        }

        private static void CreateLightingAndCamera()
        {
            GameObject light = new GameObject("Directional Light");
            var l = light.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject camera = new GameObject("Main Camera");
            camera.tag = "MainCamera";
            var cam = camera.AddComponent<Camera>();
            cam.transform.position = new Vector3(0f, 26f, -26f);
            cam.transform.rotation = Quaternion.Euler(42f, 0f, 0f);
            camera.AddComponent<AudioListener>();
        }

        private static void ConfigureNpcIntegrationForPlaytests()
        {
            NpcWorldIntegrationManager manager = Object.FindObjectOfType<NpcWorldIntegrationManager>();
            if (manager == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("registerMarkersOnStart").boolValue = true;
            so.FindProperty("addJournalEntriesOnStart").boolValue = true;
            so.FindProperty("spawnPlaceholderNpcsOnStart").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
        }

        private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localPosition = position;
                go.transform.localScale = scale;
            }
            else
            {
                go.transform.position = position;
                go.transform.localScale = scale;
            }
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
            }
            return go;
        }

        private static void SetPrivate(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
                EditorUtility.SetDirty(target as Object);
            }
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}
#endif
