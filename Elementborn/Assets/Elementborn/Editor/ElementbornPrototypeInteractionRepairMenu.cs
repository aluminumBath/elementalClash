#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPrototypeInteractionRepairMenu
    {
        [MenuItem("Elementborn/Prototype/Repair Prototype Interactions In Open Scene")]
        public static void RepairPrototypeInteractionsInOpenScene()
        {
            int players = 0;
            int interactables = 0;

            ElementbornPrototypePlayerController[] playerControllers =
                Object.FindObjectsByType<ElementbornPrototypePlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < playerControllers.Length; i++)
            {
                ElementbornPrototypePlayerController player = playerControllers[i];
                if (player == null)
                {
                    continue;
                }

                Undo.RecordObject(player, "Repair Prototype Player Interaction Range");
                player.interactRange = Mathf.Max(player.interactRange, 5.5f);
                EditorUtility.SetDirty(player);
                players++;
            }

            ElementbornPrototypeInteractable[] sceneInteractables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < sceneInteractables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = sceneInteractables[i];
                if (interactable == null)
                {
                    continue;
                }

                Undo.RecordObject(interactable, "Repair Prototype Interactable Radius");

                switch (interactable.kind)
                {
                    case ElementbornPrototypeInteractableKind.GuideNpc:
                        interactable.activationRadius = Mathf.Max(interactable.activationRadius, 5f);
                        break;
                    case ElementbornPrototypeInteractableKind.ShardResource:
                        interactable.activationRadius = Mathf.Max(interactable.activationRadius, 3.25f);
                        break;
                    case ElementbornPrototypeInteractableKind.ReturnPoint:
                        interactable.activationRadius = Mathf.Max(interactable.activationRadius, 4f);
                        break;
                    default:
                        interactable.activationRadius = Mathf.Max(interactable.activationRadius, 3.5f);
                        break;
                }

                interactable.createTriggerRadius = true;
                interactable.EnsureInteractionRadius();
                EditorUtility.SetDirty(interactable);
                interactables++;
            }

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Elementborn prototype interaction repair complete. Player controllers=" + players + ", interactables=" + interactables + ".");
        }

        [MenuItem("Elementborn/Prototype/Report Prototype Interactions")]
        public static void ReportPrototypeInteractions()
        {
            ElementbornPrototypePlayerController[] playerControllers =
                Object.FindObjectsByType<ElementbornPrototypePlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("Prototype player controllers: " + playerControllers.Length);
            for (int i = 0; i < playerControllers.Length; i++)
            {
                ElementbornPrototypePlayerController player = playerControllers[i];
                if (player != null)
                {
                    Debug.Log("Player[" + i + "] name=" + player.name + " interactRange=" + player.interactRange);
                }
            }

            ElementbornPrototypeInteractable[] sceneInteractables =
                Object.FindObjectsByType<ElementbornPrototypeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("Prototype interactables: " + sceneInteractables.Length);
            for (int i = 0; i < sceneInteractables.Length; i++)
            {
                ElementbornPrototypeInteractable interactable = sceneInteractables[i];
                if (interactable != null)
                {
                    Debug.Log("Interactable[" + i + "] name=" + interactable.name + " kind=" + interactable.kind + " radius=" + interactable.activationRadius);
                }
            }
        }
    }
}
#endif
