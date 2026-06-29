#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornPlayableRescueMenu
    {
        [MenuItem("Elementborn/Playable Setup/Install Playable Camera And Movement Rescue")]
        public static void InstallPlayableCameraAndMovementRescue()
        {
            ElementbornPlayableRescueController controller = ElementbornPlayableRescueController.EnsureInstalled();
            if (controller != null)
            {
                controller.RepairNow();
            }

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Elementborn playable camera/movement rescue installed. Press Play and use WASD/arrow keys.");
        }

        [MenuItem("Elementborn/Playable Setup/Open Generated Playable Scene")]
        public static void OpenGeneratedPlayableScene()
        {
            string path = "Assets/Elementborn/Generated/Scenes/Elementborn_Playable_Test.unity";
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning("Generated playable scene not found yet. Run Elementborn -> Playable Setup -> Build Rounded Playable Scene first.");
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log("Opened generated playable scene: " + path);
        }
    }
}
#endif
