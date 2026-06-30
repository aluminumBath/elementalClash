#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeGameplayLoopBuilder
    {
        private const string ScenePath = "Assets/Elementborn/Generated/Scenes/Elementborn_Prototype_Gameplay.unity";
        private const string PinkEyeAxolotlFbxPath = "Assets/Elementborn/Art/Models/MeshyImported/PinkEyeAxolotl/Meshy_AI_Pink_Eye_Axolotl_3D_M_0629191922_image-to-3d-texture.fbx";

        [MenuItem("Elementborn/Prototype/Build Prototype Gameplay Loop Scene")]
        public static void BuildPrototypeGameplayLoopScene()
        {
            EnsureFolder("Assets/Elementborn/Generated");
            EnsureFolder("Assets/Elementborn/Generated/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Elementborn_Prototype_Gameplay";

            CreateLighting();
            CreateElementalArena();
            GameObject player = CreatePlayer();
            CreateCamera(player.transform);
            CreateManager(player);
            CreateGuideNpc();
            CreateShard();
            CreateReturnPedestal();
            CreateTrainingDummy();
            CreateHostileEnemy();
            CreateElementalGates();
            CreateEnvoys();
            CreateResourceNodes();
            CreateHealingShrines();
            CreateLootChests();
            CreateLoreStones();
            CreateHubDressing();
            CreateLandmarks();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ElementbornPrototypeCleanFantasyHubBuilder.RebuildCleanFantasyHubLook(false);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("Built Elementborn prototype gameplay loop scene: " + ScenePath);
        }

        [MenuItem("Elementborn/Prototype/Open Prototype Gameplay Loop Scene")]
        public static void OpenPrototypeGameplayLoopScene()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogWarning("Prototype scene does not exist yet. Run Elementborn -> Prototype -> Build Prototype Gameplay Loop Scene.");
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Opened Elementborn prototype gameplay loop scene: " + ScenePath);
        }

        [MenuItem("Elementborn/Prototype/Install Prototype Loop In Open Scene")]
        public static void InstallPrototypeLoopInOpenScene()
        {
            Scene scene = EditorSceneManager.GetActiveScene();

            CreateLighting();
            if (GameObject.Find("Elementborn Test Arena") == null) CreateElementalArena();

            GameObject player = FindOrCreatePlayer();
            if (Camera.main == null) CreateCamera(player.transform);
            if (Object.FindAnyObjectByType<ElementbornPrototypeGameManager>() == null) CreateManager(player);
            if (GameObject.Find("Ember Guide") == null) CreateGuideNpc();
            if (GameObject.Find("Glowing Shard") == null) CreateShard();
            if (GameObject.Find("Return Pedestal") == null) CreateReturnPedestal();
            if (GameObject.Find("Training Dummy") == null) CreateTrainingDummy();
            if (GameObject.Find("Training Hostile") == null) CreateHostileEnemy();
            if (GameObject.Find("Fire Gate") == null) CreateElementalGates();
            if (GameObject.Find("Fire Envoy") == null) CreateEnvoys();
            if (GameObject.Find("Fire Essence Node") == null) CreateResourceNodes();
            if (GameObject.Find("Convergence Healing Shrine") == null) CreateHealingShrines();
            if (GameObject.Find("Convergence Reward Chest") == null) CreateLootChests();
            if (GameObject.Find("Lore Stone of Unity") == null) CreateLoreStones();
            if (GameObject.Find("Hub Market Stall A") == null) CreateHubDressing();

            if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);

            ElementbornPrototypeCleanFantasyHubBuilder.RebuildCleanFantasyHubLook(false);
            AssetDatabase.SaveAssets();
            Debug.Log("Installed Elementborn prototype gameplay loop in the open scene.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, name);
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.52f, 0.58f);

            GameObject lightGo = GameObject.Find("Prototype Sun");
            if (lightGo == null)
            {
                lightGo = new GameObject("Prototype Sun");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.45f;
                lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            }
        }

        private static void CreateElementalArena()
        {
            GameObject arena = new GameObject("Elementborn Test Arena");

            CreateArenaTile(arena.transform, "Fire Quarter", new Vector3(9f, -0.01f, 9f), new Color(0.55f, 0.18f, 0.08f));
            CreateArenaTile(arena.transform, "Water Quarter", new Vector3(-9f, -0.01f, 9f), new Color(0.08f, 0.22f, 0.58f));
            CreateArenaTile(arena.transform, "Earth Quarter", new Vector3(-9f, -0.01f, -9f), new Color(0.16f, 0.42f, 0.16f));
            CreateArenaTile(arena.transform, "Air Quarter", new Vector3(9f, -0.01f, -9f), new Color(0.58f, 0.72f, 0.76f));

            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            center.name = "Central Convergence Platform";
            center.transform.SetParent(arena.transform);
            center.transform.position = new Vector3(0f, 0.08f, 0f);
            center.transform.localScale = new Vector3(4.2f, 0.16f, 4.2f);
            SetMaterial(center, "Central Convergence Material", new Color(0.32f, 0.28f, 0.48f));

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Prototype Ground";
            ground.transform.SetParent(arena.transform);
            ground.transform.position = new Vector3(0f, -0.03f, 0f);
            ground.transform.localScale = new Vector3(22f, 1f, 22f);
            SetMaterial(ground, "Prototype Base Ground", new Color(0.22f, 0.34f, 0.25f));

            CreateRoad("North Road", new Vector3(0f, 0.04f, 8f), new Vector3(3f, 0.06f, 13f));
            CreateRoad("South Road", new Vector3(0f, 0.04f, -8f), new Vector3(3f, 0.06f, 13f));
            CreateRoad("East Road", new Vector3(8f, 0.04f, 0f), new Vector3(13f, 0.06f, 3f));
            CreateRoad("West Road", new Vector3(-8f, 0.04f, 0f), new Vector3(13f, 0.06f, 3f));
        }

        private static void CreateArenaTile(Transform parent, string name, Vector3 position, Color color)
        {
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = name;
            tile.transform.SetParent(parent);
            tile.transform.position = position;
            tile.transform.localScale = new Vector3(17f, 0.05f, 17f);
            SetMaterial(tile, name + " Material", color);
        }

        private static void CreateRoad(string name, Vector3 position, Vector3 scale)
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = name;
            road.transform.position = position;
            road.transform.localScale = scale;
            SetMaterial(road, name + " Material", new Color(0.28f, 0.25f, 0.22f));
        }

        private static GameObject FindOrCreatePlayer()
        {
            ElementbornPrototypePlayerController existing = Object.FindAnyObjectByType<ElementbornPrototypePlayerController>();
            return existing != null ? existing.gameObject : CreatePlayer();
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Prototype Player");
            player.transform.position = new Vector3(0f, 1f, -8f);

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.35f;
            characterController.center = Vector3.zero;

            ElementbornPrototypePlayerStats stats = player.AddComponent<ElementbornPrototypePlayerStats>();
            stats.respawnPosition = new Vector3(0f, 1f, -8f);

            ElementbornPrototypePlayerController controller = player.AddComponent<ElementbornPrototypePlayerController>();
            controller.interactRange = 5.5f;

            ElementbornPrototypeElementalAbility ability = player.AddComponent<ElementbornPrototypeElementalAbility>();
            ability.currentElement = ElementbornPrototypeElementType.Fire;

            player.AddComponent<ElementbornPrototypePlayerVisual>();

            TrySetTag(player, "Player");
            CreateStyledPlayerProxy(player.transform);

            return player;
        }

        private static void CreateStyledPlayerProxy(Transform player)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Stylized Channeler Body";
            body.transform.SetParent(player, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            SetMaterial(body, "Channeler Tunic Blue", new Color(0.12f, 0.25f, 0.55f));
            DestroyCollider(body);

            GameObject sash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sash.name = "Elemental Sash";
            sash.transform.SetParent(player, false);
            sash.transform.localPosition = new Vector3(0f, 0.25f, -0.42f);
            sash.transform.localScale = new Vector3(0.95f, 0.18f, 0.08f);
            SetMaterial(sash, "Fire Attunement Sash", ElementbornPrototypeVisualUtility.GetElementColor(ElementbornPrototypeElementType.Fire));
            DestroyCollider(sash);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Stylized Channeler Head";
            head.transform.SetParent(player, false);
            head.transform.localPosition = new Vector3(0f, 1.22f, 0f);
            head.transform.localScale = new Vector3(0.52f, 0.52f, 0.52f);
            SetMaterial(head, "Channeler Head Material", new Color(0.82f, 0.62f, 0.45f));
            DestroyCollider(head);

            AddCrownOrHood(player, new Color(0.08f, 0.12f, 0.22f));
            AddShoulderCloak(player, new Color(0.08f, 0.12f, 0.22f));
        }

        private static void CreateCamera(Transform player)
        {
            GameObject cameraGo = new GameObject("Prototype Main Camera");
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 65f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = player.position + new Vector3(0f, 5f, -8f);
            camera.transform.LookAt(player.position + Vector3.up * 1.2f);
            ElementbornPrototypeCameraFollow follow = cameraGo.AddComponent<ElementbornPrototypeCameraFollow>();
            follow.target = player;
            TrySetTag(cameraGo, "MainCamera");
        }

        private static void CreateManager(GameObject player)
        {
            GameObject managerGo = new GameObject("Prototype Game Manager");
            ElementbornPrototypeGameManager manager = managerGo.AddComponent<ElementbornPrototypeGameManager>();
            manager.player = player.GetComponent<ElementbornPrototypePlayerController>();
            manager.playCamera = Camera.main;
            manager.selectedElement = ElementbornPrototypeElementType.Fire;
            manager.loadSavedStateOnAwake = false;
        }

        private static void CreateGuideNpc()
        {
            GameObject npc = CreateCapsuleNpc("Ember Guide", new Vector3(-4f, 1f, -3f), new Color(0.95f, 0.45f, 0.18f));
            ElementbornPrototypeInteractable interactable = npc.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.GuideNpc;
            interactable.displayName = "Ember Guide";
            interactable.activationRadius = 5f;
            AddLabel(npc.transform, "Ember Guide", new Vector3(0f, 2.4f, 0f));
            AddMarker(npc, "!", ElementbornPrototypeMarkerKind.Talk, 3.3f);
            AddCrownOrHood(npc.transform, new Color(0.8f, 0.18f, 0.06f));
        }

        private static GameObject CreateCapsuleNpc(string name, Vector3 position, Color color)
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = name;
            npc.transform.position = position;
            npc.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            SetMaterial(npc, name + " Material", color);
            return npc;
        }

        private static void CreateShard()
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shard.name = "Glowing Shard";
            shard.transform.position = new Vector3(7f, 1f, 1.5f);
            shard.transform.localScale = new Vector3(0.9f, 1.25f, 0.9f);
            SetMaterial(shard, "Prototype Shard Cyan", new Color(0.1f, 0.9f, 1f));

            ElementbornPrototypeInteractable interactable = shard.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.ShardResource;
            interactable.displayName = "Glowing Shard";
            interactable.activationRadius = 3.25f;
            AddLabel(shard.transform, "Glowing Shard", new Vector3(0f, 1.8f, 0f));
            AddMarker(shard, "◆", ElementbornPrototypeMarkerKind.Objective, 2.5f);
            shard.AddComponent<ElementbornPrototypeSpin>().bobAmplitude = 0.08f;
        }

        private static void CreateReturnPedestal()
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Return Pedestal";
            pedestal.transform.position = new Vector3(0f, 0.5f, 6f);
            pedestal.transform.localScale = new Vector3(1.4f, 0.5f, 1.4f);
            SetMaterial(pedestal, "Prototype Pedestal Violet", new Color(0.55f, 0.25f, 0.95f));

            ElementbornPrototypeInteractable interactable = pedestal.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.ReturnPoint;
            interactable.displayName = "Return Pedestal";
            interactable.activationRadius = 4f;
            AddLabel(pedestal.transform, "Return Pedestal", new Vector3(0f, 1.4f, 0f));
            AddMarker(pedestal, "⬢", ElementbornPrototypeMarkerKind.Objective, 2.4f);
        }

        private static void CreateTrainingDummy()
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = "Training Dummy";
            dummy.transform.position = new Vector3(9.5f, 1f, 6.5f);
            dummy.transform.localScale = new Vector3(0.95f, 1.2f, 0.95f);
            SetMaterial(dummy, "Training Dummy Charcoal", new Color(0.18f, 0.12f, 0.1f));
            dummy.AddComponent<ElementbornPrototypeDummyEnemy>();
            AddLabel(dummy.transform, "Training Dummy\nPress Q", new Vector3(0f, 2.7f, 0f));
            AddMarker(dummy, "◎", ElementbornPrototypeMarkerKind.Combat, 3.6f);
        }

        private static void CreateHostileEnemy()
        {
            GameObject hostile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hostile.name = "Training Hostile";
            hostile.transform.position = new Vector3(-9f, 1f, -4f);
            hostile.transform.localScale = new Vector3(0.9f, 1.1f, 0.9f);
            SetMaterial(hostile, "Hostile Red", new Color(0.42f, 0.08f, 0.08f));
            hostile.AddComponent<ElementbornPrototypeHostileEnemy>();
            AddLabel(hostile.transform, "Training Hostile\nDamages Player", new Vector3(0f, 2.7f, 0f));
            AddMarker(hostile, "⚔", ElementbornPrototypeMarkerKind.Combat, 3.6f);
            AddShoulderCloak(hostile.transform, new Color(0.25f, 0.02f, 0.02f));
        }

        private static void CreateElementalGates()
        {
            CreateGate("Fire Gate", new Vector3(0f, 1.8f, 14f), 0f, ElementbornPrototypeElementType.Fire);
            CreateGate("Water Gate", new Vector3(-14f, 1.8f, 0f), 90f, ElementbornPrototypeElementType.Water);
            CreateGate("Earth Gate", new Vector3(0f, 1.8f, -14f), 180f, ElementbornPrototypeElementType.Earth);
            CreateGate("Air Gate", new Vector3(14f, 1.8f, 0f), -90f, ElementbornPrototypeElementType.Air);
        }

        private static void CreateGate(string name, Vector3 position, float yaw, ElementbornPrototypeElementType element)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Color color = ElementbornPrototypeVisualUtility.GetElementColor(element);

            GameObject left = CreateChildCube(root.transform, "Left Pillar", new Vector3(-1.4f, 0f, 0f), new Vector3(0.4f, 3.6f, 0.4f), color);
            GameObject right = CreateChildCube(root.transform, "Right Pillar", new Vector3(1.4f, 0f, 0f), new Vector3(0.4f, 3.6f, 0.4f), color);
            GameObject top = CreateChildCube(root.transform, "Top Beam", new Vector3(0f, 1.7f, 0f), new Vector3(3.2f, 0.35f, 0.45f), color);

            TextMesh label = AddLabel(top.transform, ElementbornPrototypeVisualUtility.GetElementName(element) + "\nGATE", new Vector3(0f, 1.2f, -0.35f));

            ElementbornPrototypeElementGate gate = root.AddComponent<ElementbornPrototypeElementGate>();
            gate.element = element;
            gate.leftPillar = left.transform;
            gate.rightPillar = right.transform;
            gate.topBeam = top.transform;
            gate.label = label;

            ElementbornPrototypeInteractable interactable = root.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.ElementGate;
            interactable.displayName = name;
            interactable.gateElement = element;
            interactable.element = element;
            interactable.gateController = gate;
            interactable.activationRadius = 5f;
            AddMarker(root, "⇧", ElementbornPrototypeMarkerKind.Gate, 4.6f);
        }

        private static void CreateEnvoys()
        {
            CreateEnvoy("Fire Envoy", new Vector3(4.8f, 1f, 4.8f), ElementbornPrototypeElementType.Fire,
                "Fire Envoy: Fire channelers prize resolve. Supremacists call it purity; unifiers call it courage.");
            CreateEnvoy("Water Envoy", new Vector3(-4.8f, 1f, 4.8f), ElementbornPrototypeElementType.Water,
                "Water Envoy: Water adapts. It remembers old wounds and finds paths through hard stone.");
            CreateEnvoy("Earth Envoy", new Vector3(-4.8f, 1f, -4.8f), ElementbornPrototypeElementType.Earth,
                "Earth Envoy: Earth stands for kinship, borders, memory, and stubborn survival.");
            CreateEnvoy("Air Envoy", new Vector3(4.8f, 1f, -4.8f), ElementbornPrototypeElementType.Air,
                "Air Envoy: Air carries rumor, prayer, rebellion, and freedom across every wall.");
        }

        private static void CreateEnvoy(string name, Vector3 position, ElementbornPrototypeElementType element, string text)
        {
            GameObject envoy = CreateCapsuleNpc(name, position, ElementbornPrototypeVisualUtility.GetElementColor(element));
            ElementbornPrototypeInteractable interactable = envoy.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.EnvoyNpc;
            interactable.displayName = name;
            interactable.element = element;
            interactable.customText = text;
            interactable.activationRadius = 4.5f;
            AddLabel(envoy.transform, name, new Vector3(0f, 2.4f, 0f));
            AddMarker(envoy, "?", ElementbornPrototypeMarkerKind.Talk, 3.25f);
            AddShoulderCloak(envoy.transform, ElementbornPrototypeVisualUtility.GetElementColor(element));
        }

        private static void CreateResourceNodes()
        {
            CreateResourceNode("Fire Essence Node", new Vector3(8f, 0.8f, 8.8f), ElementbornPrototypeElementType.Fire);
            CreateResourceNode("Water Essence Node", new Vector3(-8.8f, 0.8f, 8f), ElementbornPrototypeElementType.Water);
            CreateResourceNode("Earth Essence Node", new Vector3(-8f, 0.8f, -8.8f), ElementbornPrototypeElementType.Earth);
            CreateResourceNode("Air Essence Node", new Vector3(8.8f, 0.8f, -8f), ElementbornPrototypeElementType.Air);
        }

        private static void CreateResourceNode(string name, Vector3 position, ElementbornPrototypeElementType element)
        {
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node.name = name;
            node.transform.position = position;
            node.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            SetMaterial(node, name + " Material", ElementbornPrototypeVisualUtility.GetElementColor(element));

            ElementbornPrototypeInteractable interactable = node.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.ResourceNode;
            interactable.displayName = name;
            interactable.element = element;
            interactable.amount = 1;
            interactable.activationRadius = 3f;
            AddLabel(node.transform, name, new Vector3(0f, 1.4f, 0f));
            node.AddComponent<ElementbornPrototypeSpin>().bobAmplitude = 0.08f;
            AddMarker(node, "+", ElementbornPrototypeMarkerKind.Resource, 2.25f);
        }

        private static void CreateHealingShrines()
        {
            CreateShrine("Convergence Healing Shrine", new Vector3(-3.2f, 0.5f, 1.6f), new Color(0.35f, 1f, 0.72f));
            CreateShrine("Outer Rest Shrine", new Vector3(10f, 0.5f, -2f), new Color(0.7f, 0.9f, 1f));
        }

        private static void CreateShrine(string name, Vector3 position, Color color)
        {
            GameObject shrine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shrine.name = name;
            shrine.transform.position = position;
            shrine.transform.localScale = new Vector3(1f, 0.5f, 1f);
            SetMaterial(shrine, name + " Material", color);

            ElementbornPrototypeInteractable interactable = shrine.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.HealingShrine;
            interactable.displayName = name;
            interactable.activationRadius = 3.5f;
            AddLabel(shrine.transform, "Healing Shrine", new Vector3(0f, 1.3f, 0f));
            AddMarker(shrine, "✚", ElementbornPrototypeMarkerKind.Shrine, 2.4f);
        }

        private static void CreateLootChests()
        {
            CreateChest("Convergence Reward Chest", new Vector3(3.2f, 0.45f, 1.6f), "Chest needs essence after the hostile is defeated.");
            CreateChest("Side Supply Chest", new Vector3(-9f, 0.45f, 2.5f), "A small chest used to test loot interaction.");
        }

        private static void CreateChest(string name, Vector3 position, string text)
        {
            GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = name;
            chest.transform.position = position;
            chest.transform.localScale = new Vector3(1.2f, 0.8f, 0.8f);
            SetMaterial(chest, name + " Material", new Color(0.72f, 0.45f, 0.16f));

            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lid.name = name + " Lid";
            lid.transform.SetParent(chest.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            lid.transform.localScale = new Vector3(1.08f, 0.18f, 1.05f);
            SetMaterial(lid, name + " Lid Material", new Color(0.92f, 0.68f, 0.26f));
            DestroyCollider(lid);

            ElementbornPrototypeInteractable interactable = chest.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.LootChest;
            interactable.displayName = name;
            interactable.customText = text;
            interactable.activationRadius = 3.25f;
            AddLabel(chest.transform, name, new Vector3(0f, 1.2f, 0f));
            AddMarker(chest, "$", ElementbornPrototypeMarkerKind.Loot, 2.35f);
        }

        private static void CreateLoreStones()
        {
            CreateLoreStone("Lore Stone of Unity", new Vector3(-2.2f, 1.2f, -2.4f),
                "The oldest inscription says the center was built before the factions had names. It warns: division begins when power forgets responsibility.");
            CreateLoreStone("Lore Stone of Dominion", new Vector3(2.2f, 1.2f, -2.4f),
                "A later hand carved over the stone: peace is only real when the strongest element enforces it.");
            CreateLoreStone("Lore Stone of Bloodlines", new Vector3(0f, 1.2f, -4.2f),
                "Some bloodlines channel cleanly. Some combine strangely. Some are feared because no one understands what they might become.");
        }

        private static void CreateLoreStone(string name, Vector3 position, string text)
        {
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = name;
            stone.transform.position = position;
            stone.transform.localScale = new Vector3(0.55f, 2.2f, 0.35f);
            SetMaterial(stone, name + " Material", new Color(0.22f, 0.22f, 0.28f));

            ElementbornPrototypeInteractable interactable = stone.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.LoreStone;
            interactable.displayName = name;
            interactable.customText = text;
            interactable.activationRadius = 3.5f;
            AddLabel(stone.transform, name.Replace("Lore Stone of ", ""), new Vector3(0f, 1.35f, -0.25f));
            AddMarker(stone, "i", ElementbornPrototypeMarkerKind.Lore, 2.9f);
        }

        private static void CreateHubDressing()
        {
            CreateMarketStall("Hub Market Stall A", new Vector3(-5.5f, 0.55f, 0f), new Color(0.5f, 0.18f, 0.08f));
            CreateMarketStall("Hub Market Stall B", new Vector3(5.5f, 0.55f, 0f), new Color(0.08f, 0.18f, 0.5f));
            CreateBanner("Unifier Banner", new Vector3(-1.6f, 1.4f, 3.2f), new Color(0.2f, 0.75f, 0.45f));
            CreateBanner("Dominion Banner", new Vector3(1.6f, 1.4f, 3.2f), new Color(0.75f, 0.18f, 0.18f));
            CreateArchway("Small Central Arch", new Vector3(0f, 1.4f, 3.2f), new Color(0.33f, 0.29f, 0.38f));
        }

        private static void CreateMarketStall(string name, Vector3 position, Color color)
        {
            GameObject baseBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseBox.name = name;
            baseBox.transform.position = position;
            baseBox.transform.localScale = new Vector3(2.2f, 0.7f, 1.2f);
            SetMaterial(baseBox, name + " Wood", new Color(0.32f, 0.18f, 0.08f));

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopy.name = name + " Canopy";
            canopy.transform.position = position + Vector3.up * 0.9f;
            canopy.transform.localScale = new Vector3(2.5f, 0.25f, 1.5f);
            SetMaterial(canopy, name + " Canopy Material", color);
        }

        private static void CreateBanner(string name, Vector3 position, Color color)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pole.name = name + " Pole";
            pole.transform.position = position + Vector3.left * 0.35f;
            pole.transform.localScale = new Vector3(0.12f, 2.4f, 0.12f);
            SetMaterial(pole, name + " Pole Material", new Color(0.18f, 0.16f, 0.12f));

            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = name;
            cloth.transform.position = position + new Vector3(0.2f, 0.35f, 0f);
            cloth.transform.localScale = new Vector3(0.9f, 1.2f, 0.08f);
            SetMaterial(cloth, name + " Cloth", color);
        }

        private static void CreateArchway(string name, Vector3 position, Color color)
        {
            CreateChildlessCube(name + " Left", position + new Vector3(-1.2f, 0f, 0f), new Vector3(0.3f, 2.8f, 0.3f), color);
            CreateChildlessCube(name + " Right", position + new Vector3(1.2f, 0f, 0f), new Vector3(0.3f, 2.8f, 0.3f), color);
            CreateChildlessCube(name, position + new Vector3(0f, 1.4f, 0f), new Vector3(2.7f, 0.3f, 0.35f), color);
        }



        private static void CreateImportedModelShowcase()
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Imported Meshy Axolotl Showcase";
            pedestal.transform.position = new Vector3(-11.2f, 0.45f, 6.8f);
            pedestal.transform.localScale = new Vector3(1.4f, 0.45f, 1.4f);
            SetMaterial(pedestal, "Imported Model Pedestal", new Color(0.1f, 0.22f, 0.34f));

            AttachImportedModelVisual(
                pedestal.transform,
                PinkEyeAxolotlFbxPath,
                "Imported Axolotl Showcase Visual",
                new Vector3(0f, 0.25f, 0f),
                2.2f,
                ElementbornPrototypeModelAnimationMode.Swim,
                "Showcase creature visual backed by imported Meshy FBX.");

            ElementbornPrototypeInteractable interactable = pedestal.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.LoreStone;
            interactable.displayName = "Imported Meshy Model";
            interactable.customText = "This is the first imported FBX creature asset in the prototype. It uses procedural bone/root animation as a bridge before authored clips.";
            interactable.activationRadius = 4f;

            AddLabel(pedestal.transform, "Imported\\nMeshy Model", new Vector3(0f, 2.6f, 0f));
            AddMarker(pedestal, "3D", ElementbornPrototypeMarkerKind.Lore, 3.6f);
        }

        private static GameObject AttachImportedModelVisual(
            Transform parent,
            string assetPath,
            string childName,
            Vector3 localOffset,
            float targetHeight,
            ElementbornPrototypeModelAnimationMode animationMode,
            string notes)
        {
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (modelAsset == null)
            {
                GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fallback.name = childName + " Fallback";
                fallback.transform.SetParent(parent, false);
                fallback.transform.localPosition = localOffset + Vector3.up * 1f;
                fallback.transform.localScale = Vector3.one * 1.25f;
                SetMaterial(fallback, childName + " Fallback Material", new Color(0.85f, 0.25f, 0.55f));
                fallback.AddComponent<ElementbornPrototypeSpin>().bobAmplitude = 0.08f;
                return fallback;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(modelAsset);
            }

            instance.name = childName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localOffset;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            NormalizeImportedModelScale(instance.transform, targetHeight, localOffset);

            ElementbornPrototypeImportedModelAnimator animator = instance.GetComponent<ElementbornPrototypeImportedModelAnimator>();
            if (animator == null)
            {
                animator = instance.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            }

            animator.mode = animationMode;

            ElementbornPrototypeImportedModelTag tag = instance.GetComponent<ElementbornPrototypeImportedModelTag>();
            if (tag == null)
            {
                tag = instance.AddComponent<ElementbornPrototypeImportedModelTag>();
            }

            tag.sourceAssetPath = assetPath;
            tag.modelRole = childName;
            tag.notes = notes;

            return instance;
        }

        private static void NormalizeImportedModelScale(Transform modelRoot, float targetHeight, Vector3 desiredLocalOffset)
        {
            if (modelRoot == null)
            {
                return;
            }

            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                modelRoot.localScale = Vector3.one * 0.02f;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            float height = Mathf.Max(0.001f, bounds.size.y);
            float scale = Mathf.Clamp(targetHeight / height, 0.001f, 10f);
            modelRoot.localScale = Vector3.one * scale;

            // Recompute bounds after scale and center the model over the parent.
            renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            Vector3 worldParent = modelRoot.parent != null ? modelRoot.parent.TransformPoint(desiredLocalOffset) : desiredLocalOffset;
            Vector3 correction = worldParent - bounds.center;
            correction.y += bounds.extents.y;
            modelRoot.position += correction;
        }

        private static void HideRenderer(GameObject go)
        {
            Renderer renderer = go != null ? go.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private static void CreateAssetBackedVisuals()
        {
            CreateAssetBillboard(
                "Fire Capital Vista Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/volcanic_fortress_in_fiery_colors.png",
                new Vector3(11.5f, 2.4f, 12.2f),
                Quaternion.Euler(0f, 225f, 0f),
                new Vector2(4.8f, 3.2f),
                "Fire Capital Vista",
                ElementbornPrototypeMarkerKind.Lore,
                false);

            CreateAssetBillboard(
                "Social NPC Roster Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/social_npcs_character_lineup_illustration.png",
                new Vector3(-6.8f, 2.2f, 5.8f),
                Quaternion.Euler(0f, 145f, 0f),
                new Vector2(4.2f, 2.7f),
                "NPC Roster",
                ElementbornPrototypeMarkerKind.Talk,
                false);

            CreateAssetBillboard(
                "Spell Training Icon Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/fantasy_spell_and_combat_icons_chart.png",
                new Vector3(6.4f, 2.2f, 5.8f),
                Quaternion.Euler(0f, 215f, 0f),
                new Vector2(4.0f, 2.7f),
                "Spell Icons",
                ElementbornPrototypeMarkerKind.Objective,
                false);

            CreateAssetBillboard(
                "Quest Gear Reward Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/fantasy_quest_and_equipment_icons_ui.png",
                new Vector3(4.6f, 2.1f, -5.8f),
                Quaternion.Euler(0f, 325f, 0f),
                new Vector2(3.8f, 2.6f),
                "Quest Gear",
                ElementbornPrototypeMarkerKind.Loot,
                false);

            CreateAssetBillboard(
                "Map Marker Reference Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/stylized_game_map_marker_assets_sheet.png",
                new Vector3(-4.8f, 2.1f, -5.8f),
                Quaternion.Euler(0f, 35f, 0f),
                new Vector2(3.8f, 2.6f),
                "Map Markers",
                ElementbornPrototypeMarkerKind.Lore,
                false);

            CreateAssetBillboard(
                "Game Asset Design Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/stylized_game_asset_design_sheet.png",
                new Vector3(12.4f, 2.1f, -3.6f),
                Quaternion.Euler(0f, 270f, 0f),
                new Vector2(3.8f, 2.6f),
                "Asset Sheet",
                ElementbornPrototypeMarkerKind.Lore,
                false);

            CreateAssetBillboard(
                "UI Style Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/fantasy_ui_design_reference_poster.png",
                new Vector3(-12.4f, 2.1f, 3.6f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector2(3.8f, 2.6f),
                "UI Style",
                ElementbornPrototypeMarkerKind.Lore,
                false);

            CreateAssetBillboard(
                "Boss Icon Board",
                "Assets/Elementborn/Art/Concept/WindwakerReplacement/fantasy_boss_icon_reference_sheet.png",
                new Vector3(-11.8f, 2.1f, -9.8f),
                Quaternion.Euler(0f, 45f, 0f),
                new Vector2(3.7f, 2.5f),
                "Boss Icons",
                ElementbornPrototypeMarkerKind.Combat,
                false);
        }

        private static GameObject CreateAssetBillboard(
            string name,
            string texturePath,
            Vector3 position,
            Quaternion rotation,
            Vector2 size,
            string labelText,
            ElementbornPrototypeMarkerKind markerKind,
            bool faceCamera)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            root.transform.rotation = rotation;

            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Quad);
            board.name = name + " Image";
            board.transform.SetParent(root.transform, false);
            board.transform.localPosition = Vector3.zero;
            board.transform.localRotation = Quaternion.identity;
            board.transform.localScale = new Vector3(size.x, size.y, 1f);

            Renderer renderer = board.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateTextureMaterial(name + " Material", texturePath, new Color(0.18f, 0.16f, 0.22f));
            }

            Collider collider = board.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = name + " Backing";
            frame.transform.SetParent(root.transform, false);
            frame.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            frame.transform.localRotation = Quaternion.identity;
            frame.transform.localScale = new Vector3(size.x + 0.24f, size.y + 0.24f, 0.08f);
            SetMaterial(frame, name + " Frame Material", new Color(0.12f, 0.08f, 0.05f));

            // Move backing behind the quad after material assignment. Negative z is behind the image plane.
            frame.transform.localPosition = new Vector3(0f, 0f, 0.08f);

            AddLabel(root.transform, labelText, new Vector3(0f, -size.y * 0.58f, 0f));
            AddMarker(root, "ART", markerKind, size.y * 0.65f + 1.0f);

            ElementbornPrototypeBillboardFacing facing = root.AddComponent<ElementbornPrototypeBillboardFacing>();
            facing.faceCamera = faceCamera;
            facing.onlyYaw = true;

            return root;
        }

        private static Material CreateTextureMaterial(string name, string texturePath, Color fallbackColor)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                return CreateMaterial(name + " Fallback", fallbackColor);
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Texture");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return CreateMaterial(name + " Fallback", fallbackColor);
            }

            Material material = new Material(shader);
            material.name = name;

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", Color.white);
            }

            return material;
        }

        private static void CreateLandmarks()
        {
            CreateLandmark("Fire Spire", new Vector3(12f, 1.8f, 9f), new Vector3(1.3f, 3.6f, 1.3f), new Color(0.6f, 0.13f, 0.08f));
            CreateLandmark("Water Marker", new Vector3(-12f, 0.2f, 9f), new Vector3(4f, 0.2f, 4f), new Color(0.1f, 0.35f, 0.9f));
            CreateLandmark("Earth Monolith", new Vector3(-12f, 1.6f, -9f), new Vector3(1.9f, 3.2f, 1.9f), new Color(0.14f, 0.3f, 0.12f));
            CreateLandmark("Air Obelisk", new Vector3(12f, 1.6f, -9f), new Vector3(1f, 3.2f, 1f), new Color(0.65f, 0.78f, 0.82f));
        }

        private static void CreateLandmark(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position;
            landmark.transform.localScale = scale;
            SetMaterial(landmark, name + " Material", color);
        }

        private static void CreateInstructionSigns()
        {
            CreateSign("Prototype Instructions", new Vector3(0f, 1.5f, -12f), "Talk > Choose > Collect > Return > Gate > Defeat > Essence > Chest > Lore");
            CreateSign("Controls Sign", new Vector3(-7f, 1.5f, -8f), "WASD + E + Q\nShift stamina");
        }

        private static void CreateSign(string name, Vector3 position, string label)
        {
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = name;
            sign.transform.position = position;
            sign.transform.localScale = new Vector3(3.5f, 1.4f, 0.18f);
            SetMaterial(sign, name + " Material", new Color(0.32f, 0.18f, 0.08f));
            AddLabel(sign.transform, label, new Vector3(0f, 0f, -0.12f));
        }

        private static GameObject CreateChildCube(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            SetMaterial(go, name + " Material", color);
            return go;
        }

        private static void CreateChildlessCube(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            SetMaterial(go, name + " Material", color);
        }

        private static TextMesh AddLabel(Transform parent, string text, Vector3 localPosition)
        {
            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent, false);
            labelGo.transform.localPosition = localPosition;
            labelGo.transform.localRotation = Quaternion.identity;
            labelGo.transform.localScale = Vector3.one * 0.18f;

            TextMesh label = labelGo.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.3f;
            label.fontSize = 48;
            label.color = Color.white;
            return label;
        }

        private static void SetMaterial(GameObject go, string materialName, Color color)
        {
            Renderer renderer = go != null ? go.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial(materialName, color);
            }
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("HDRP/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }


        private static ElementbornPrototypeQuestMarker AddMarker(GameObject go, string text, ElementbornPrototypeMarkerKind kind, float height = 2.8f)
        {
            ElementbornPrototypeQuestMarker marker = go.AddComponent<ElementbornPrototypeQuestMarker>();
            marker.markerText = text;
            marker.kind = kind;
            marker.localOffset = new Vector3(0f, height, 0f);
            return marker;
        }

        private static void AddCrownOrHood(Transform parent, Color color)
        {
            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hood.name = "Stylized Hood Ring";
            hood.transform.SetParent(parent, false);
            hood.transform.localPosition = new Vector3(0f, 1.56f, 0f);
            hood.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
            SetMaterial(hood, "Stylized Hood Material", color);
            DestroyCollider(hood);
        }

        private static void AddShoulderCloak(Transform parent, Color color)
        {
            GameObject cloak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloak.name = "Stylized Shoulder Cloak";
            cloak.transform.SetParent(parent, false);
            cloak.transform.localPosition = new Vector3(0f, 0.82f, 0.24f);
            cloak.transform.localScale = new Vector3(1.05f, 0.5f, 0.16f);
            SetMaterial(cloak, "Stylized Cloak Material", color);
            DestroyCollider(cloak);
        }

        private static void DestroyCollider(GameObject go)
        {
            Collider collider = go != null ? go.GetComponent<Collider>() : null;
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void TrySetTag(GameObject go, string tagName)
        {
            if (go == null) return;

            try
            {
                go.tag = tagName;
            }
            catch
            {
                // Tag repair is handled by earlier setup patches; do not block scene generation.
            }
        }
    }
}
#endif
