using System.Reflection;
using UnityEngine;

namespace Elementborn.Tests.PlayMode
{
    internal static class ElementbornPlayModeTestUtility
    {
        public static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        public static void CleanupScene()
        {
#if UNITY_2023_1_OR_NEWER
            GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            GameObject[] objects = Object.FindObjectsOfType<GameObject>(true);
#endif
            foreach (GameObject go in objects)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }
        }
    }
}
