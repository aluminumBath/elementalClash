using UnityEngine;

namespace Elementborn.Game
{
    public sealed class InventoryDebugSeeder : MonoBehaviour
    {
        [SerializeField] private bool seedOnStart;
        [SerializeField] private InventoryItemDefinition[] items;
        [SerializeField] private int quantity = 1;
        [SerializeField] private int currency = 25;

        private void Start()
        {
            if (seedOnStart)
            {
                Seed();
            }
        }

        [ContextMenu("Seed Inventory")]
        public void Seed()
        {
            PlayerInventoryTracker.AddCurrency(currency);

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item != null)
                {
                    PlayerInventoryTracker.AddItem(item, quantity);
                }
            }
        }
    }
}
