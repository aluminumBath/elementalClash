using UnityEngine;

namespace Elementborn.Game
{
    public sealed class StorageInteractable : BaseInteractable
    {
        [SerializeField] private StorageContainerInventory storage;
        [SerializeField] private bool markOnMap = true;

        private void Awake()
        {
            if (storage == null)
            {
                storage = GetComponent<StorageContainerInventory>();
            }
        }

        private void Start()
        {
            if (markOnMap)
            {
                string label = storage != null ? storage.DisplayName : "Storage";
                PlayerMapMarkerTracker.ReportStorageChest(transform.position, label);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string label = storage != null ? storage.DisplayName : "Storage";
            return InteractionPromptData.Simple(label, "Open");
        }

        public override void Interact(GameObject interactor)
        {
            Debug.Log("Storage opened. Wire this to a storage UI panel later.");
            base.Interact(interactor);
        }
    }
}
