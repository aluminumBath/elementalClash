using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(Collider))]
    public sealed class ItemPickupTrigger : MonoBehaviour
    {
        [SerializeField] private InventoryItemDefinition itemDefinition;
        [SerializeField] private string fallbackItemId = "";
        [SerializeField] private int quantity = 1;
        [SerializeField] private bool destroyOnPickup = true;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            InventoryTransactionResult result = itemDefinition != null
                ? PlayerInventoryTracker.AddItem(itemDefinition, quantity)
                : PlayerInventoryTracker.AddItemId(fallbackItemId, quantity);

            if (result.Moved > 0 && destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
