#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeStateRepairMenu
    {
        [MenuItem("Elementborn/Prototype/Clear Prototype Save And Reset Open Scene")]
        public static void ClearPrototypeSaveAndResetOpenScene()
        {
            PlayerPrefs.DeleteKey(ElementbornPrototypeGameManager.SaveKeyQuestState);
            PlayerPrefs.DeleteKey(ElementbornPrototypeGameManager.SaveKeyPlayerX);
            PlayerPrefs.DeleteKey(ElementbornPrototypeGameManager.SaveKeyPlayerY);
            PlayerPrefs.DeleteKey(ElementbornPrototypeGameManager.SaveKeyPlayerZ);
            PlayerPrefs.DeleteKey(ElementbornPrototypeGameManager.SaveKeyElement);
            PlayerPrefs.DeleteKey(ElementbornPrototypeGameManager.SaveKeyPathChoice);
            PlayerPrefs.Save();

            ElementbornPrototypeGameManager manager = Object.FindAnyObjectByType<ElementbornPrototypeGameManager>();
            if (manager != null)
            {
                Undo.RecordObject(manager, "Reset Prototype Manager State");
                manager.questState = ElementbornPrototypeQuestState.NotStarted;
                manager.pathChoice = ElementbornPrototypePathChoice.None;
                manager.loadSavedStateOnAwake = false;
                manager.ResetSceneRuntimeState(true);
                EditorUtility.SetDirty(manager);
            }

            ElementbornPrototypeInteractable[] interactables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < interactables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = interactables[i];
                if (interactable != null && interactable.kind == ElementbornPrototypeInteractableKind.ShardResource)
                {
                    interactable.gameObject.SetActive(true);
                    EditorUtility.SetDirty(interactable.gameObject);
                }
            }

            ElementbornPrototypeDummyEnemy[] dummies =
                Object.FindObjectsByType<ElementbornPrototypeDummyEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < dummies.Length; i++)
            {
                ElementbornPrototypeDummyEnemy dummy = dummies[i];
                if (dummy == null)
                {
                    continue;
                }

                try
                {
                    dummy.ResetDummy();
                    EditorUtility.SetDirty(dummy);
                    EditorUtility.SetDirty(dummy.gameObject);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Elementborn could not reset prototype dummy '" + dummy.name + "': " + ex.Message);
                }
            }

            ElementbornPrototypeHostileEnemy[] hostiles =
                Object.FindObjectsByType<ElementbornPrototypeHostileEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < hostiles.Length; i++)
            {
                ElementbornPrototypeHostileEnemy hostile = hostiles[i];
                if (hostile == null)
                {
                    continue;
                }

                try
                {
                    hostile.ResetEnemy();
                    EditorUtility.SetDirty(hostile);
                    EditorUtility.SetDirty(hostile.gameObject);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Elementborn could not reset prototype hostile '" + hostile.name + "': " + ex.Message);
                }
            }

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Elementborn prototype save cleared and open scene reset to NotStarted.");
        }
    }
}
#endif
