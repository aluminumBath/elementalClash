using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Elementborn.Game
{
    /// <summary>
    /// Central EventSystem helper.
    /// Unity uGUI warns every frame if more than one EventSystem exists in the loaded scene(s).
    ///
    /// v72 note:
    /// Do NOT assign EventSystem.current directly. Unity logs:
    /// "Failed setting EventSystem.current to unknown EventSystem ... No module"
    /// if a candidate EventSystem does not yet have a valid input module. Instead, this
    /// utility ensures exactly one active EventSystem with an input module and lets Unity
    /// establish EventSystem.current naturally.
    /// </summary>
    public static class ElementbornEventSystemUtility
    {
        public static EventSystem EnsureSingleEventSystem(bool createIfMissing = true, string reason = null)
        {
            EventSystem[] systems = FindEventSystems();
            EventSystem keep = ChooseEventSystemToKeep(systems);

            if (keep == null && createIfMissing)
            {
                GameObject go = new GameObject("EventSystem");
                keep = go.AddComponent<EventSystem>();
                EnsureInputModule(keep);

                if (!string.IsNullOrWhiteSpace(reason))
                {
                    Debug.Log($"[Elementborn] Created EventSystem ({reason}).");
                }

                return keep;
            }

            if (keep == null)
            {
                return null;
            }

            EnsureInputModule(keep);
            EnableKeptEventSystem(keep);
            RemoveDuplicateEventSystems(keep, systems, reason);

            return keep;
        }

        public static int CountEventSystems(bool includeInactive = true)
        {
            return FindEventSystems(includeInactive).Length;
        }

        private static EventSystem[] FindEventSystems(bool includeInactive = true)
        {
            try
            {
                return UnityEngine.Object.FindObjectsByType<EventSystem>(
                    includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
            }
            catch
            {
                EventSystem current = EventSystem.current;
                return current != null ? new[] { current } : Array.Empty<EventSystem>();
            }
        }

        private static EventSystem ChooseEventSystemToKeep(EventSystem[] systems)
        {
            if (systems == null || systems.Length == 0)
            {
                return null;
            }

            // Prefer current only if it already has a usable input module.
            EventSystem current = EventSystem.current;
            if (current != null && HasEnabledInputModule(current))
            {
                for (int i = 0; i < systems.Length; i++)
                {
                    if (systems[i] == current)
                    {
                        return systems[i];
                    }
                }
            }

            // Then prefer an active enabled EventSystem that already has an enabled module.
            for (int i = 0; i < systems.Length; i++)
            {
                EventSystem system = systems[i];
                if (system != null &&
                    system.isActiveAndEnabled &&
                    system.gameObject.activeInHierarchy &&
                    HasEnabledInputModule(system))
                {
                    return system;
                }
            }

            // Then prefer an active EventSystem.
            for (int i = 0; i < systems.Length; i++)
            {
                EventSystem system = systems[i];
                if (system != null &&
                    system.gameObject.activeInHierarchy)
                {
                    return system;
                }
            }

            // Last resort: keep the first valid one and make it usable.
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] != null)
                {
                    return systems[i];
                }
            }

            return null;
        }

        private static void EnableKeptEventSystem(EventSystem keep)
        {
            if (keep == null)
            {
                return;
            }

            if (!keep.enabled)
            {
                keep.enabled = true;
            }

            if (keep.gameObject != null && !keep.gameObject.activeSelf)
            {
                keep.gameObject.SetActive(true);
            }
        }

        private static bool HasEnabledInputModule(EventSystem eventSystem)
        {
            if (eventSystem == null)
            {
                return false;
            }

            BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null && modules[i].enabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveDuplicateEventSystems(EventSystem keep, EventSystem[] systems, string reason)
        {
            if (keep == null || systems == null || systems.Length <= 1)
            {
                return;
            }

            int removed = 0;

            for (int i = 0; i < systems.Length; i++)
            {
                EventSystem duplicate = systems[i];
                if (duplicate == null || duplicate == keep)
                {
                    continue;
                }

                DisableOrDestroyDuplicate(duplicate);
                removed++;
            }

            if (removed > 0)
            {
                string suffix = string.IsNullOrWhiteSpace(reason) ? string.Empty : $" ({reason})";
                Debug.Log($"[Elementborn] Removed/disabled {removed} duplicate EventSystem object(s){suffix}.");
            }
        }

        private static void DisableOrDestroyDuplicate(EventSystem duplicate)
        {
            if (duplicate == null)
            {
                return;
            }

            GameObject go = duplicate.gameObject;
            BaseInputModule[] modules = go != null ? go.GetComponents<BaseInputModule>() : Array.Empty<BaseInputModule>();

            if (CanSafelyDestroyEventSystemObject(go))
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(go);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }

                return;
            }

            duplicate.enabled = false;

            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null)
                {
                    modules[i].enabled = false;
                }
            }

            if (go != null && !go.name.StartsWith("Disabled Duplicate EventSystem", StringComparison.Ordinal))
            {
                go.name = "Disabled Duplicate EventSystem - " + go.name;
            }
        }

        private static bool CanSafelyDestroyEventSystemObject(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null ||
                    component is Transform ||
                    component is EventSystem ||
                    component is BaseInputModule)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static void EnsureInputModule(EventSystem eventSystem)
        {
            if (eventSystem == null)
            {
                return;
            }

            BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null && modules[i].enabled)
                {
                    return;
                }
            }

            // Re-enable an existing module first.
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null)
                {
                    modules[i].enabled = true;
                    return;
                }
            }

            Type inputSystemModule = FindInputSystemUiInputModuleType();
            if (inputSystemModule != null)
            {
                try
                {
                    Component module = eventSystem.gameObject.AddComponent(inputSystemModule);
                    if (module is BaseInputModule baseInputModule)
                    {
                        baseInputModule.enabled = true;
                    }

                    return;
                }
                catch
                {
                    // Fall back to the legacy module below.
                }
            }

            try
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
            catch
            {
                // If the active input backend disallows the legacy module, leave the EventSystem itself intact.
            }
        }

        private static Type FindInputSystemUiInputModuleType()
        {
            try
            {
                Type type = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (type != null && typeof(BaseInputModule).IsAssignableFrom(type))
                {
                    return type;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }

    /// <summary>
    /// Runtime guard that dedupes EventSystems on scene load and during the first few frames,
    /// catching late-created UI canvases/world-map views.
    /// </summary>
    public sealed class ElementbornEventSystemGuard : MonoBehaviour
    {
        private int framesRemaining = 180;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallAfterSceneLoad()
        {
            ElementbornEventSystemUtility.EnsureSingleEventSystem(true, "runtime scene load");
            EnsureGuard();
        }

        private static void EnsureGuard()
        {
            try
            {
                ElementbornEventSystemGuard existing = UnityEngine.Object.FindAnyObjectByType<ElementbornEventSystemGuard>();
                if (existing != null)
                {
                    return;
                }

                GameObject go = new GameObject("Elementborn EventSystem Guard");
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(go);
                }

                go.AddComponent<ElementbornEventSystemGuard>();
            }
            catch
            {
                // Never block play mode because a diagnostics guard could not be installed.
            }
        }

        private void Awake()
        {
            ElementbornEventSystemUtility.EnsureSingleEventSystem(true, "guard awake");
        }

        private void OnEnable()
        {
            ElementbornEventSystemUtility.EnsureSingleEventSystem(true, "guard enable");
        }

        private void Update()
        {
            if (framesRemaining <= 0)
            {
                return;
            }

            framesRemaining--;
            ElementbornEventSystemUtility.EnsureSingleEventSystem(true, "guard first frames");
        }
    }
}
