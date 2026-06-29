using UnityEngine;

namespace Elementborn.Game
{
    public static class SafeComponentExtensions
    {
        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static T EnsureComponent<T>(this Component component) where T : Component
        {
            return component != null ? component.gameObject.EnsureComponent<T>() : null;
        }

        public static bool TryGetComponentInParentSafe<T>(this Component component, out T result) where T : Component
        {
            result = null;
            if (component == null)
            {
                return false;
            }

            result = component.GetComponentInParent<T>();
            return result != null;
        }

        public static bool TryGetComponentInChildrenSafe<T>(this Component component, out T result) where T : Component
        {
            result = null;
            if (component == null)
            {
                return false;
            }

            result = component.GetComponentInChildren<T>();
            return result != null;
        }
    }
}
