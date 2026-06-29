using System.Reflection;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AdminRuntimeCommandRouter : MonoBehaviour
    {
        public static AdminRuntimeCommandRouter Instance { get; private set; }

        [SerializeField] private bool searchInactiveObjects = true;
        [SerializeField] private bool logUnhandledCommands = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static AdminRuntimeCommandRouter Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(AdminRuntimeCommandRouter));
            return go.AddComponent<AdminRuntimeCommandRouter>();
        }

        public bool ExecuteCommand(string command, out string message)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                message = "No command entered.";
                return false;
            }

            bool handled = TryExecuteOnExistingBridges(command.Trim(), out message);
            if (handled)
            {
                return true;
            }

            handled = TryExecuteKnownShortcut(command.Trim(), out message);
            if (handled)
            {
                return true;
            }

            message = "No admin bridge handled command: " + command;
            if (logUnhandledCommands)
            {
                Debug.LogWarning(message);
            }
            return false;
        }

        public bool ExecuteCommand(string command)
        {
            return ExecuteCommand(command, out _);
        }

        private bool TryExecuteOnExistingBridges(string command, out string message)
        {
            message = "";
#if UNITY_2023_1_OR_NEWER
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(
                searchInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
#else
            MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(searchInactiveObjects);
#endif

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || behaviour == this)
                {
                    continue;
                }

                MethodInfo method = behaviour.GetType().GetMethod(
                    "ExecuteCommand",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(string) },
                    null);

                if (method == null)
                {
                    continue;
                }

                object result = method.Invoke(behaviour, new object[] { command });
                if (result is bool handled && handled)
                {
                    message = behaviour.GetType().Name + " handled: " + command;
                    Debug.Log(message);
                    return true;
                }
            }

            return false;
        }

        private bool TryExecuteKnownShortcut(string command, out string message)
        {
            message = "";

            if (command == "dashboard.refresh")
            {
                StorySystemsDebugDashboard dashboard = ElementbornFindUtility.FindFirst<StorySystemsDebugDashboard>();
                if (dashboard != null)
                {
                    dashboard.Refresh();
                }
                message = "Dashboard refreshed.";
                return true;
            }

            if (command == "loop.start")
            {
                ElementbornMainGameplayLoopDirector.Ensure().StartGame();
                message = "Gameplay loop started.";
                return true;
            }

            if (command == "loop.spawn")
            {
                ElementbornMainGameplayLoopDirector.Ensure().SpawnStarterWaves();
                message = "Starter waves spawned.";
                return true;
            }

            if (command == "fire.volcano")
            {
                FireCapitalVolcanoHazardController volcano = ElementbornFindUtility.FindFirst<FireCapitalVolcanoHazardController>();
                if (volcano == null)
                {
                    volcano = new GameObject(nameof(FireCapitalVolcanoHazardController)).AddComponent<FireCapitalVolcanoHazardController>();
                }
                volcano.PulseVolcanoPressure();
                message = "Fire Capital volcano pulse triggered.";
                return true;
            }

            if (command == "fire.calm")
            {
                FireCapitalVolcanoHazardController volcano = ElementbornFindUtility.FindFirst<FireCapitalVolcanoHazardController>();
                if (volcano == null)
                {
                    volcano = new GameObject(nameof(FireCapitalVolcanoHazardController)).AddComponent<FireCapitalVolcanoHazardController>();
                }
                volcano.CalmVolcano();
                message = "Fire Capital volcano calmed.";
                return true;
            }

            if (command == "narrative.save")
            {
                NarrativeRuntimeSaveBridge bridge = ElementbornFindUtility.FindFirst<NarrativeRuntimeSaveBridge>();
                if (bridge == null)
                {
                    bridge = new GameObject(nameof(NarrativeRuntimeSaveBridge)).AddComponent<NarrativeRuntimeSaveBridge>();
                }
                bridge.SaveCurrentSlot();
                message = "Narrative slot saved.";
                return true;
            }

            if (command == "narrative.load")
            {
                NarrativeRuntimeSaveBridge bridge = ElementbornFindUtility.FindFirst<NarrativeRuntimeSaveBridge>();
                if (bridge == null)
                {
                    bridge = new GameObject(nameof(NarrativeRuntimeSaveBridge)).AddComponent<NarrativeRuntimeSaveBridge>();
                }
                bridge.LoadCurrentSlot();
                message = "Narrative slot loaded.";
                return true;
            }

            return false;
        }
    }
}
