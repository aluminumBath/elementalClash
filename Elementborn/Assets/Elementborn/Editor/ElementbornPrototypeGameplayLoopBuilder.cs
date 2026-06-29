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
            CreateGround();
            GameObject player = CreatePlayer();
            CreateCamera(player.transform);
            CreateManager(player);
            CreateGuideNpc();
            CreateShard();
            CreateReturnPedestal();
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
            if (GameObject.Find("Prototype Ground") == null)
            {
                CreateGround();
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
            RenderSettings.ambientLight = new Color(0.45f, 0.5f, 0.58f);

            GameObject lightGo = GameObject.Find("Prototype Sun");
            if (lightGo == null)
            {
                lightGo = new GameObject("Prototype Sun");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.35f;
                lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            }
        }

        private static void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Prototype Ground";
            ground.transform.localScale = new Vector3(18f, 1f, 18f);
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial("Prototype Ground Green", new Color(0.25f, 0.44f, 0.24f));
            }
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
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Prototype Player";
            player.transform.position = new Vector3(0f, 1f, -8f);
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial("Prototype Player Blue", new Color(0.2f, 0.45f, 0.95f));
            }

            Collider collider = player.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            player.AddComponent<CharacterController>();
            ElementbornPrototypePlayerController controller = player.AddComponent<ElementbornPrototypePlayerController>();
            controller.interactRange = 5.5f;
            TrySetTag(player, "Player");
            return player;
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
        }

        private static void CreateGuideNpc()
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = "Ember Guide";
            npc.transform.position = new Vector3(-4f, 1f, -3f);
            npc.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            Renderer renderer = npc.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial("Prototype Guide Orange", new Color(0.95f, 0.45f, 0.18f));
            }

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
            shard.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);
            Renderer renderer = shard.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial("Prototype Shard Cyan", new Color(0.1f, 0.9f, 1f));
            }

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
            Renderer renderer = pedestal.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial("Prototype Pedestal Violet", new Color(0.55f, 0.25f, 0.95f));
            }

            ElementbornPrototypeInteractable interactable = pedestal.AddComponent<ElementbornPrototypeInteractable>();
            interactable.kind = ElementbornPrototypeInteractableKind.ReturnPoint;
            interactable.displayName = "Return Pedestal";
            interactable.activationRadius = 4f;
            AddLabel(pedestal.transform, "Return Pedestal", new Vector3(0f, 1.4f, 0f));
        }

        private static void CreateLandmarks()
        {
            CreateLandmark("Basalt Stone A", new Vector3(5.2f, 1f, 4.5f), new Vector3(1.2f, 2f, 1.2f), new Color(0.12f, 0.12f, 0.14f));
            CreateLandmark("Basalt Stone B", new Vector3(8.6f, 0.75f, 3.5f), new Vector3(1.4f, 1.5f, 1.4f), new Color(0.13f, 0.12f, 0.12f));
            CreateLandmark("Water Marker", new Vector3(-7f, 0.2f, 5f), new Vector3(3f, 0.15f, 3f), new Color(0.1f, 0.35f, 0.9f));
        }

        private static void CreateLandmark(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject landmark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landmark.name = name;
            landmark.transform.position = position;
            landmark.transform.localScale = scale;
            Renderer renderer = landmark.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial(name + " Material", color);
            }
        }

        private static void CreateInstructionSigns()
        {
            CreateSign("Prototype Instructions", new Vector3(0f, 1.5f, -12f), "Talk > Collect > Return");
            CreateSign("Controls Sign", new Vector3(-7f, 1.5f, -8f), "WASD + E");
        }

        private static void CreateSign(string name, Vector3 position, string label)
        {
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = name;
            sign.transform.position = position;
            sign.transform.localScale = new Vector3(3.5f, 1.4f, 0.18f);
            Renderer renderer = sign.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMaterial(name + " Material", new Color(0.32f, 0.18f, 0.08f));
            }

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
