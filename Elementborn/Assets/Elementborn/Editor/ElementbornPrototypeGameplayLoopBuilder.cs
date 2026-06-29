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
            CreateLandmarks();
            CreateInstructionSigns();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
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
            if (GameObject.Find("Elementborn Test Arena") == null)
            {
                CreateElementalArena();
            }

            GameObject player = FindOrCreatePlayer();
            if (Camera.main == null)
            {
                CreateCamera(player.transform);
            }

            if (Object.FindAnyObjectByType<ElementbornPrototypeGameManager>() == null)
            {
                CreateManager(player);
            }

            if (GameObject.Find("Ember Guide") == null)
            {
                CreateGuideNpc();
            }

            if (GameObject.Find("Glowing Shard") == null)
            {
                CreateShard();
            }

            if (GameObject.Find("Return Pedestal") == null)
            {
                CreateReturnPedestal();
            }

            if (GameObject.Find("Training Dummy") == null)
            {
                CreateTrainingDummy();
            }

            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Installed Elementborn prototype gameplay loop in the open scene.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

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

        private static GameObject FindOrCreatePlayer()
        {
            ElementbornPrototypePlayerController existing = Object.FindAnyObjectByType<ElementbornPrototypePlayerController>();
            if (existing != null)
            {
                return existing.gameObject;
            }

            return CreatePlayer();
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Prototype Player");
            player.transform.position = new Vector3(0f, 1f, -8f);

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.35f;
            characterController.center = Vector3.zero;

            ElementbornPrototypePlayerController controller = player.AddComponent<ElementbornPrototypePlayerController>();
            controller.interactRange = 5.5f;

            ElementbornPrototypeElementalAbility ability = player.AddComponent<ElementbornPrototypeElementalAbility>();
            ability.currentElement = ElementbornPrototypeElementType.Fire;

            ElementbornPrototypePlayerVisual visual = player.AddComponent<ElementbornPrototypePlayerVisual>();

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

            Collider bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                Object.DestroyImmediate(bodyCollider);
            }

            GameObject sash = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sash.name = "Elemental Sash";
            sash.transform.SetParent(player, false);
            sash.transform.localPosition = new Vector3(0f, 0.25f, -0.42f);
            sash.transform.localScale = new Vector3(0.95f, 0.18f, 0.08f);
            SetMaterial(sash, "Fire Attunement Sash", ElementbornPrototypeVisualUtility.GetElementColor(ElementbornPrototypeElementType.Fire));
            Collider sashCollider = sash.GetComponent<Collider>();
            if (sashCollider != null)
            {
                Object.DestroyImmediate(sashCollider);
            }

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Stylized Channeler Head";
            head.transform.SetParent(player, false);
            head.transform.localPosition = new Vector3(0f, 1.22f, 0f);
            head.transform.localScale = new Vector3(0.52f, 0.52f, 0.52f);
            SetMaterial(head, "Channeler Head Material", new Color(0.82f, 0.62f, 0.45f));
            Collider headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
            {
                Object.DestroyImmediate(headCollider);
            }
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
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = "Ember Guide";
            npc.transform.position = new Vector3(-4f, 1f, -3f);
            npc.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            SetMaterial(npc, "Prototype Guide Orange", new Color(0.95f, 0.45f, 0.18f));

            ElementbornPrototypeInteractable interactable = npc.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.GuideNpc;
            interactable.displayName = "Ember Guide";
            interactable.activationRadius = 5f;
            AddLabel(npc.transform, "Ember Guide", new Vector3(0f, 2.4f, 0f));
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
            CreateSign("Prototype Instructions", new Vector3(0f, 1.5f, -12f), "Talk > Collect > Return > Cast");
            CreateSign("Controls Sign", new Vector3(-7f, 1.5f, -8f), "WASD + E + Q");
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

        private static void AddLabel(Transform parent, string text, Vector3 localPosition)
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
            if (shader == null)
            {
                shader = Shader.Find("HDRP/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.name = name;
            material.color = color;
            return material;
        }

        private static void TrySetTag(GameObject go, string tagName)
        {
            if (go == null)
            {
                return;
            }

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
