using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Drops a simple pickup object for an item from the player's inventory.
    /// </summary>
    public sealed class InventoryDropper : MonoBehaviour
    {
        [SerializeField] private Transform dropOrigin;
        [SerializeField] private float dropForwardOffset = 1.25f;

        public bool DropItem(InventoryItemDefinition definition, int quantity = 1)
        {
            if (definition == null)
            {
                return false;
            }

            if (!PlayerInventoryTracker.HasItem(definition, quantity))
            {
                NotificationFeed.Post($"You do not have {definition.DisplayName} x{quantity}.", NotificationType.Warning);
                return false;
            }

            PlayerInventoryTracker.RemoveItem(definition, quantity);

            Vector3 origin = dropOrigin != null ? dropOrigin.position : transform.position;
            Vector3 forward = dropOrigin != null ? dropOrigin.forward : transform.forward;
            Vector3 position = origin + forward * dropForwardOffset;

            GameObject go;
            if (definition.WorldPrefab != null)
            {
                go = Instantiate(definition.WorldPrefab, position, Quaternion.identity);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = position;
                go.name = "Dropped_" + definition.ItemId;
            }

            var pickup = go.GetComponent<ItemPickupInteractable>();
            if (pickup == null)
            {
                pickup = go.AddComponent<ItemPickupInteractable>();
            }

            // The pickup fields are private; this component is mainly for world prefab workflows.
            // For quick debug drops, fallback pickup wiring can be added in the inspector on the prefab.
            NotificationFeed.Post($"Dropped {definition.DisplayName} x{quantity}.", NotificationType.Inventory);
            return true;
        }
    }
}
