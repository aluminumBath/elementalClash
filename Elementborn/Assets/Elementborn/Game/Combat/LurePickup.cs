using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A taming lure resting in the world. Walk the player over it to add that creature's lure to the
    /// inventory — the specific item taming needs. The world spawner scatters these in markets, shrines,
    /// and camps for the creatures that roam nearby.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class LurePickup : MonoBehaviour
    {
        [SerializeField] private CreatureKind kind = CreatureKind.Horse;

        public void Configure(CreatureKind creatureKind) => kind = creatureKind;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var inv = other.GetComponentInParent<PlayerInventory>();
            if (inv == null && !other.CompareTag("Player")) return;
            if (inv == null) inv = PlayerInventory.Instance;
            if (inv == null) return;

            inv.AddLure(kind);
            Destroy(gameObject);
        }
    }
}
