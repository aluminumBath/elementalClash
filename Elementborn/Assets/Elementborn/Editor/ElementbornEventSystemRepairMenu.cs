#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornEventSystemRepairMenu
    {
        [MenuItem("Elementborn/UI/Fix Duplicate EventSystems In Open Scene")]
        public static void FixDuplicateEventSystemsInOpenScene()
        {
            int before = ElementbornEventSystemUtility.CountEventSystems(true);
            EventSystem system = ElementbornEventSystemUtility.EnsureSingleEventSystem(true, "manual editor repair");
            int after = ElementbornEventSystemUtility.CountEventSystems(true);

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();

            string activeName = system != null ? system.name : "<none>";
            Debug.Log($"Elementborn EventSystem repair complete. Before={before}, After={after}, Active={activeName}");
        }

        [MenuItem("Elementborn/UI/Report EventSystems In Open Scene")]
        public static void ReportEventSystemsInOpenScene()
        {
            EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
            Debug.Log($"Elementborn EventSystem report: {systems.Length} EventSystem object(s) found.");

            for (int i = 0; i < systems.Length; i++)
            {
                EventSystem system = systems[i];
                if (system == null)
                {
                    continue;
                }

                string path = GetPath(system.transform);
                Debug.Log($"EventSystem[{i}] active={system.gameObject.activeInHierarchy}, enabled={system.enabled}, path={path}");
            }
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null)
            {
                return "<null>";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
#endif
