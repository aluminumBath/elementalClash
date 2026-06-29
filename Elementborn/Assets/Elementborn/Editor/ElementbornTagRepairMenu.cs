#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornTagRepairMenu
    {
        private static readonly string[] RequiredCustomTags =
        {
            "Enemy",
            "Interactable",
            "Boat",
            "Boss",
            "Projectile",
            "ResourceNode",
            "QuestObjective"
        };

        [MenuItem("Elementborn/Unity Setup/Repair Required Tags Now")]
        public static void RepairRequiredTagsNow()
        {
            Object[] tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                Debug.LogError("Elementborn could not load ProjectSettings/TagManager.asset. Use the v75 PowerShell repair script to patch it directly.");
                return;
            }

            SerializedObject tagManager = new SerializedObject(tagManagerAssets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            if (tags == null)
            {
                Debug.LogError("Elementborn could not find the tags property in TagManager.asset.");
                return;
            }

            int added = 0;
            for (int i = 0; i < RequiredCustomTags.Length; i++)
            {
                if (AddTag(tags, RequiredCustomTags[i]))
                {
                    added++;
                }
            }

            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            bool interactablePresent = HasTag("Interactable");
            Debug.Log("Elementborn required tag repair complete. Added " + added + " tag(s). Interactable present=" + interactablePresent);
        }

        [MenuItem("Elementborn/Unity Setup/Report Required Tags")]
        public static void ReportRequiredTags()
        {
            HashSet<string> existing = new HashSet<string>(InternalEditorUtility.tags);
            for (int i = 0; i < RequiredCustomTags.Length; i++)
            {
                string tag = RequiredCustomTags[i];
                string status = existing.Contains(tag) ? "OK" : "MISSING";
                Debug.Log("Elementborn required tag '" + tag + "': " + status);
            }
        }

        private static bool AddTag(SerializedProperty tags, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || HasTag(tag))
            {
                return false;
            }

            int index = tags.arraySize;
            tags.InsertArrayElementAtIndex(index);
            SerializedProperty element = tags.GetArrayElementAtIndex(index);
            element.stringValue = tag;
            return true;
        }

        private static bool HasTag(string tag)
        {
            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
