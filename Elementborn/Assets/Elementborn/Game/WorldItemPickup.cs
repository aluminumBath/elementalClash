using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A catalog item resting in the world: its 3D model (from <see cref="ItemModelNames"/>) shows where it sits,
    /// and walking the player over it adds it to the inventory. Place by hand or via a spawner; set the item id
    /// and amount. Items without a mapped model keep a primitive — the collider still works either way.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class WorldItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId = "ember_shard";
        [SerializeField] private int amount = 1;

        public void Configure(string id, int count = 1) { itemId = id; amount = Mathf.Max(1, count); }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void Start()
        {
            // Only attach a model for a real item id; unknown ids keep the primitive placeholder.
            if (ItemCatalog.Get(itemId) != null)
                ModelLibrary.Attach(ItemModelNames.ResourcePath(itemId), gameObject, "Item");
        }

        private void OnTriggerEnter(Collider other)
        {
            var inv = other.GetComponentInParent<PlayerInventory>();
            if (inv == null && !other.CompareTag("Player")) return;
            if (inv == null) inv = PlayerInventory.Instance;
            var def = ItemCatalog.Get(itemId);
            if (inv == null || def == null) return;

            inv.AddItem(itemId, amount);
            GameHud.Instance?.Toast($"+{amount} {def.Name}");
            Destroy(gameObject);
        }
    }
}
