using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Last-mile play-mode safety net for generated prototype scenes.
    ///
    /// This does not replace the real player/camera/controller stack. It only guarantees that
    /// pressing Play shows something visible and gives the tester basic WASD movement while
    /// the fuller gameplay prefabs are still being wired up.
    ///
    /// v77 fixes:
    /// - CharacterController center now matches the capsule mesh so the capsule no longer sinks halfway into the ground.
    /// - Runtime grounding clamp keeps the rescue player above the visible plane.
    /// - Adds a temporary in-game menu overlay while the final main menu flow is not built yet.
    /// </summary>
    public sealed class ElementbornPlayableRescueController : MonoBehaviour
    {
        private const string RescueRootName = "Elementborn Playable Rescue";
        private const string RescuePlayerName = "Elementborn Rescue Player";
        private const string RescueGroundName = "Elementborn Rescue Ground";
        private const string RescueCameraName = "Elementborn Rescue Camera";
        private const float RescueGroundY = 0f;
        private const float RescuePlayerVisualHalfHeight = 1f;

        [SerializeField] private bool showControlsOverlay = true;
        [SerializeField] private bool createGroundIfMissing = true;
        [SerializeField] private bool createPlayerIfMissing = true;
        [SerializeField] private bool createCameraIfMissing = true;
        [SerializeField] private bool showTemporaryMainMenu = true;

        private GameObject player;
        private Transform playerTransform;
        private CharacterController characterController;
        private Camera playCamera;
        private float verticalVelocity;
        private int repairFramesRemaining = 30;
        private bool menuOpen = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindAnyObjectByType<ElementbornPrototypeGameManager>() != null)
            {
                return;
            }

            EnsureInstalled();
        }

        public static ElementbornPlayableRescueController EnsureInstalled()
        {
            ElementbornPlayableRescueController existing = FindAnyObjectByType<ElementbornPlayableRescueController>();
            if (existing != null)
            {
                existing.RepairNow();
                return existing;
            }

            GameObject root = GameObject.Find(RescueRootName);
            if (root == null)
            {
                root = new GameObject(RescueRootName);
            }

            ElementbornPlayableRescueController controller = root.GetComponent<ElementbornPlayableRescueController>();
            if (controller == null)
            {
                controller = root.AddComponent<ElementbornPlayableRescueController>();
            }

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(root);
            }

            controller.RepairNow();
            return controller;
        }

        public void RepairNow()
        {
            EnsureLighting();
            EnsureGround();
            EnsurePlayer();
            EnsureCamera();
            KeepPlayerAboveGround();
        }

        private void Awake()
        {
            menuOpen = showTemporaryMainMenu;
            RepairNow();
        }

        private void OnEnable()
        {
            RepairNow();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                menuOpen = !menuOpen;
            }

            if (repairFramesRemaining > 0)
            {
                repairFramesRemaining--;
                RepairNow();
            }

            if (!menuOpen)
            {
                HandleMovement();
            }

            KeepPlayerAboveGround();
            UpdateCameraFollow();
        }

        private void EnsureLighting()
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].enabled)
                {
                    return;
                }
            }

            GameObject lightGo = new GameObject("Elementborn Rescue Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private void EnsureGround()
        {
            if (!createGroundIfMissing)
            {
                return;
            }

            if (GameObject.Find(RescueGroundName) != null)
            {
                return;
            }

            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer is ParticleSystemRenderer)
                {
                    continue;
                }

                Bounds bounds = renderer.bounds;
                if (bounds.size.x > 15f && bounds.size.z > 15f)
                {
                    return;
                }
            }

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = RescueGroundName;
            ground.transform.position = new Vector3(0f, RescueGroundY, 0f);
            ground.transform.localScale = new Vector3(12f, 1f, 12f);

            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.sharedMaterial = CreateRescueMaterial(new Color(0.25f, 0.45f, 0.25f));
            }
        }

        private void EnsurePlayer()
        {
            if (!createPlayerIfMissing)
            {
                return;
            }

            if (player != null && characterController != null)
            {
                ConfigureCharacterController(characterController);
                return;
            }

            player = FindExistingPlayer();
            if (player == null)
            {
                Vector3 spawn = FindSpawnPosition();
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = RescuePlayerName;
                player.transform.position = spawn;

                Renderer renderer = player.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = CreateRescueMaterial(new Color(0.2f, 0.45f, 0.95f));
                }

                TrySetTag(player, "Player");
            }

            playerTransform = player.transform;
            characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                Collider collider = player.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                characterController = player.AddComponent<CharacterController>();
            }

            ConfigureCharacterController(characterController);
            KeepPlayerAboveGround();
        }

        private void ConfigureCharacterController(CharacterController controller)
        {
            if (controller == null)
            {
                return;
            }

            controller.height = 2f;
            controller.radius = 0.35f;

            // Critical v77 fix:
            // The Unity capsule mesh is centered on the GameObject. With center=(0,1,0),
            // the controller bottom sat one unit above the visual bottom, so gravity moved
            // the transform down until the mesh was halfway underground.
            controller.center = Vector3.zero;
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 55f;
            controller.skinWidth = 0.04f;
        }

        private GameObject FindExistingPlayer()
        {
            GameObject taggedPlayer = null;
            try
            {
                taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            }
            catch
            {
                taggedPlayer = null;
            }

            if (taggedPlayer != null)
            {
                return taggedPlayer;
            }

            CharacterController existingController = FindAnyObjectByType<CharacterController>();
            if (existingController != null)
            {
                return existingController.gameObject;
            }

            GameObject named = GameObject.Find("Player");
            if (named != null)
            {
                return named;
            }

            return null;
        }

        private Vector3 FindSpawnPosition()
        {
            Transform spawn = FindSpawnByName("PlayerStart_Central") ?? FindSpawnByName("PlayerStart") ?? FindSpawnByName("SpawnPoint");
            if (spawn != null)
            {
                Vector3 position = spawn.position;
                if (position.y < RescueGroundY + RescuePlayerVisualHalfHeight)
                {
                    position.y = RescueGroundY + RescuePlayerVisualHalfHeight;
                }

                return position;
            }

            return new Vector3(0f, RescueGroundY + RescuePlayerVisualHalfHeight, -8f);
        }

        private Transform FindSpawnByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            GameObject go = GameObject.Find(name);
            return go != null ? go.transform : null;
        }

        private void EnsureCamera()
        {
            if (!createCameraIfMissing)
            {
                return;
            }

            if (playCamera != null && playCamera.enabled)
            {
                return;
            }

            playCamera = Camera.main;
            if (playCamera == null)
            {
                Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < cameras.Length; i++)
                {
                    if (cameras[i] != null && cameras[i].enabled)
                    {
                        playCamera = cameras[i];
                        break;
                    }
                }
            }

            if (playCamera == null)
            {
                GameObject cameraGo = new GameObject(RescueCameraName);
                playCamera = cameraGo.AddComponent<Camera>();
                playCamera.fieldOfView = 65f;
                TrySetTag(cameraGo, "MainCamera");
            }

            if (playerTransform != null)
            {
                UpdateCameraFollow(true);
            }
        }

        private void HandleMovement()
        {
            if (playerTransform == null || characterController == null)
            {
                return;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (Mathf.Approximately(horizontal, 0f))
            {
                horizontal = KeyAxis(KeyCode.A, KeyCode.D) + KeyAxis(KeyCode.LeftArrow, KeyCode.RightArrow);
            }

            if (Mathf.Approximately(vertical, 0f))
            {
                vertical = KeyAxis(KeyCode.S, KeyCode.W) + KeyAxis(KeyCode.DownArrow, KeyCode.UpArrow);
            }

            Vector3 forward = playCamera != null ? playCamera.transform.forward : Vector3.forward;
            Vector3 right = playCamera != null ? playCamera.transform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 move = forward * vertical + right * horizontal;
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 8f : 4f;

            if (characterController.isGrounded || playerTransform.position.y <= RescueGroundY + RescuePlayerVisualHalfHeight + 0.02f)
            {
                verticalVelocity = -0.5f;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    verticalVelocity = 5f;
                }
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            Vector3 velocity = move * speed;
            velocity.y = verticalVelocity;

            characterController.Move(velocity * Time.deltaTime);

            if (move.sqrMagnitude > 0.001f)
            {
                playerTransform.rotation = Quaternion.Slerp(
                    playerTransform.rotation,
                    Quaternion.LookRotation(move, Vector3.up),
                    Time.deltaTime * 12f);
            }
        }

        private void KeepPlayerAboveGround()
        {
            if (playerTransform == null)
            {
                return;
            }

            float minimumY = RescueGroundY + RescuePlayerVisualHalfHeight;
            if (playerTransform.position.y < minimumY)
            {
                Vector3 position = playerTransform.position;
                position.y = minimumY;
                playerTransform.position = position;
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = -0.5f;
                }
            }
        }

        private float KeyAxis(KeyCode negative, KeyCode positive)
        {
            float value = 0f;
            if (Input.GetKey(negative))
            {
                value -= 1f;
            }

            if (Input.GetKey(positive))
            {
                value += 1f;
            }

            return value;
        }

        private void UpdateCameraFollow(bool snap = false)
        {
            if (playCamera == null || playerTransform == null)
            {
                return;
            }

            Vector3 desired = playerTransform.position + new Vector3(0f, 4.5f, -7f);
            playCamera.transform.position = snap
                ? desired
                : Vector3.Lerp(playCamera.transform.position, desired, Time.deltaTime * 8f);

            Vector3 lookTarget = playerTransform.position + Vector3.up * 1.2f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - playCamera.transform.position, Vector3.up);
            playCamera.transform.rotation = snap
                ? desiredRotation
                : Quaternion.Slerp(playCamera.transform.rotation, desiredRotation, Time.deltaTime * 8f);
        }

        private Material CreateRescueMaterial(Color color)
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

            Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Hidden/InternalErrorShader"));
            material.color = color;
            return material;
        }

        private void TrySetTag(GameObject go, string tagName)
        {
            if (go == null || string.IsNullOrWhiteSpace(tagName))
            {
                return;
            }

            try
            {
                go.tag = tagName;
            }
            catch
            {
                // Built-in tags should exist, but do not let tag setup block playability.
            }
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (showTemporaryMainMenu && menuOpen)
            {
                DrawTemporaryMainMenu();
                return;
            }

            if (showControlsOverlay)
            {
                DrawControlsOverlay();
            }
        }

        private void DrawTemporaryMainMenu()
        {
            const int width = 460;
            const int height = 250;
            Rect rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            GUI.Box(rect, "Elementborn Prototype Menu");

            GUI.Label(new Rect(rect.x + 24, rect.y + 44, width - 48, 42),
                "This is a temporary playable-test menu. It will be replaced by the final title/main menu flow.");

            if (GUI.Button(new Rect(rect.x + 120, rect.y + 98, 220, 32), "Start / Resume Prototype"))
            {
                menuOpen = false;
                RepairNow();
            }

            if (GUI.Button(new Rect(rect.x + 120, rect.y + 138, 220, 32), "Recenter Player"))
            {
                if (playerTransform != null)
                {
                    playerTransform.position = new Vector3(0f, RescueGroundY + RescuePlayerVisualHalfHeight, -8f);
                    verticalVelocity = -0.5f;
                    UpdateCameraFollow(true);
                }
            }

            if (GUI.Button(new Rect(rect.x + 120, rect.y + 178, 220, 32), "Repair Scene Again"))
            {
                RepairNow();
            }

            GUI.Label(new Rect(rect.x + 24, rect.y + 218, width - 48, 22), "Esc toggles this menu.");
        }

        private void DrawControlsOverlay()
        {
            const int width = 460;
            const int height = 92;
            Rect rect = new Rect(16, 16, width, height);
            GUI.Box(rect, "Elementborn Playable Rescue");
            GUI.Label(new Rect(28, 42, width - 24, 20), "Move: WASD / Arrow Keys    Sprint: Shift    Jump: Space");
            GUI.Label(new Rect(28, 62, width - 24, 20), "Menu: Esc    This rescue layer appears while final gameplay prefabs are wired up.");
        }
    }
}
