#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Elementborn.Game.EditorTools
{
    public static class Elementborn3DTextOrientationRepairMenu
    {
        [MenuItem("Elementborn/Visuals/Fix Backwards 3D Text Labels")]
        public static void FixBackwards3DTextLabels()
        {
            TextMesh[] labels = Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int repaired = 0;

            for (int i = 0; i < labels.Length; i++)
            {
                TextMesh label = labels[i];
                if (label == null)
                {
                    continue;
                }

                Transform transform = label.transform;
                bool looksLikeSceneSign =
                    label.name == "Label" ||
                    transform.parent != null && transform.parent.name.StartsWith("Sign_") ||
                    transform.parent != null && transform.parent.name.Contains("Sign");

                if (!looksLikeSceneSign)
                {
                    continue;
                }

                Undo.RecordObject(transform, "Fix 3D Text Orientation");

                Vector3 localPosition = transform.localPosition;
                if (Mathf.Abs(localPosition.z) < 0.01f)
                {
                    localPosition.z = -0.16f;
                }

                transform.localPosition = localPosition;
                transform.localRotation = Quaternion.identity;
                transform.localScale = new Vector3(
                    Mathf.Abs(transform.localScale.x) < 0.001f ? 1f : Mathf.Abs(transform.localScale.x),
                    Mathf.Abs(transform.localScale.y) < 0.001f ? 1f : Mathf.Abs(transform.localScale.y),
                    Mathf.Abs(transform.localScale.z) < 0.001f ? 1f : Mathf.Abs(transform.localScale.z));

                label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center;

                EditorUtility.SetDirty(label);
                repaired++;
            }

            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Elementborn 3D text orientation repair complete. Repaired " + repaired + " TextMesh label(s).");
        }

        [MenuItem("Elementborn/Visuals/Report 3D Text Labels")]
        public static void Report3DTextLabels()
        {
            TextMesh[] labels = Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("Elementborn 3D text report: found " + labels.Length + " TextMesh object(s).");

            for (int i = 0; i < labels.Length; i++)
            {
                TextMesh label = labels[i];
                if (label == null)
                {
                    continue;
                }

                Transform transform = label.transform;
                Debug.Log(
                    "TextMesh[" + i + "] name=" + label.name +
                    " parent=" + (transform.parent != null ? transform.parent.name : "<none>") +
                    " text=\"" + label.text + "\"" +
                    " localEuler=" + transform.localEulerAngles +
                    " localScale=" + transform.localScale);
            }
        }
    }
}
#endif
