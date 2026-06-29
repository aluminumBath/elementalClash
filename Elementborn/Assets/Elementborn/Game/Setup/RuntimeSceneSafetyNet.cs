using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Defensive runtime safety net for play-test scenes.
    /// It creates very small fallback scene objects when common Unity setup pieces are missing.
    /// </summary>
    public sealed class RuntimeSceneSafetyNet : MonoBehaviour
    {
        [SerializeField] private bool ensureEventSystem = true;
        [SerializeField] private bool ensureMainCamera = true;
        [SerializeField] private bool ensureCanvas = true;
        [SerializeField] private bool ensurePlayerObject = true;
        [SerializeField] private bool ensureRuntimeBootstrap = true;
        [SerializeField] private bool runOnAwake = true;
        [SerializeField] private bool logActions = true;

        private void Awake()
        {
            if (runOnAwake)
            {
                RunSafetyChecks();
            }
        }

        [ContextMenu("Run Safety Checks")]
        public void RunSafetyChecks()
        {
            if (ensureEventSystem)
            {
                EnsureEventSystem();
            }

            if (ensureMainCamera)
            {
                EnsureMainCamera();
            }

            if (ensureCanvas)
            {
                EnsureCanvas();
            }

            if (ensureRuntimeBootstrap)
            {
                EnsureRuntimeBootstrap();
            }

            if (ensurePlayerObject)
            {
                EnsurePlayer();
            }
        }

        public void EnsureEventSystem()
        {
            int before = ElementbornEventSystemUtility.CountEventSystems(true);
            EventSystem system = ElementbornEventSystemUtility.EnsureSingleEventSystem(true, "RuntimeSceneSafetyNet");
            int after = ElementbornEventSystemUtility.CountEventSystems(true);

            if (system != null && before == 0)
            {
                Log("Created fallback EventSystem.");
            }
            else if (before != after)
            {
                Log($"Repaired EventSystem count from {before} to {after}.");
            }
        }

        public void EnsureMainCamera()
        {
            if (Camera.main != null || ElementbornFindUtility.FindFirst<Camera>() != null)
            {
                return;
            }

            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 8f, -10f);
            go.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            go.AddComponent<AudioListener>();
            Log("Created fallback Main Camera.");
        }

        public void EnsureCanvas()
        {
            if (ElementbornFindUtility.FindFirst<Canvas>() != null)
            {
                return;
            }

            var go = new GameObject("Fallback UI Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            Log("Created fallback UI Canvas.");
        }

        public void EnsureRuntimeBootstrap()
        {
            if (ElementbornFindUtility.FindFirst<ElementbornRuntimeBootstrap>() != null)
            {
                return;
            }

            var go = new GameObject("Elementborn Runtime Bootstrap");
            go.AddComponent<ElementbornRuntimeBootstrap>();
            Log("Created fallback ElementbornRuntimeBootstrap.");
        }

        public void EnsurePlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Fallback Player";
                TryAssignTag(player, "Player");
                player.transform.position = Vector3.up;
                Log("Created fallback Player capsule.");
            }

            player.EnsureComponent<CharacterController>();
            player.EnsureComponent<PlayerInteractor>();
            player.EnsureComponent<SimpleCombatHealth>();
            player.EnsureComponent<StaminaResource>();
            player.EnsureComponent<CombatDefenseController>();
            player.EnsureComponent<SpellResourcePool>();
            player.EnsureComponent<SpellCastController>();
            player.EnsureComponent<SpellLoadoutController>();
        }

        private void TryAssignTag(GameObject go, string tag)
        {
            try
            {
                go.tag = tag;
            }
            catch
            {
                Log($"Could not assign tag '{tag}'. Run Elementborn → Unity Setup → Ensure Tags and Layers.");
            }
        }

        private void Log(string message)
        {
            if (logActions)
            {
                Debug.Log($"[RuntimeSceneSafetyNet] {message}");
            }
        }
    }
}
