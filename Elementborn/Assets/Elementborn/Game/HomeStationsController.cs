using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Owns the player's home Storage and Stable as persistent containers. They carry fixed ids, and the inventory +
    /// creature-bonding save bridges discover every container/stable by id, so their contents survive saves with no
    /// extra wiring. Created once and always present (so they exist before a save is loaded); the
    /// <see cref="HomeMenuViewer"/> only exposes them once the matching home addition is built. Lives on the Home object.
    /// </summary>
    public sealed class HomeStationsController : MonoBehaviour
    {
        public static HomeStationsController Instance { get; private set; }

        public const string StorageId = "home_storage";
        public const string StableId = "home_stable";

        public StorageContainerInventory Storage { get; private set; }
        public CreatureStable Stable { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            Storage = EnsureChild<StorageContainerInventory>(StorageId);
            Stable = EnsureChild<CreatureStable>(StableId);
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        // Child GameObject name becomes the container's id (StorageId/StableId fall back to the object name).
        private T EnsureChild<T>(string childName) where T : Component
        {
            var existing = transform.Find(childName);
            if (existing != null)
            {
                var comp = existing.GetComponent<T>();
                if (comp != null) return comp;
            }
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<T>();
        }
    }
}
