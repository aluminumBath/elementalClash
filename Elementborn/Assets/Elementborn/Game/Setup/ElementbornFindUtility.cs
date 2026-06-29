using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Version-safe wrappers around Unity object-finding APIs.
    /// Use this instead of calling obsolete FindObject(s)OfType directly.
    /// </summary>
    public static class ElementbornFindUtility
    {
        public static T FindFirst<T>(bool includeInactive = true) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        public static T[] FindAll<T>(bool includeInactive = true) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(
                includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>(includeInactive);
#endif
        }
    }
}
