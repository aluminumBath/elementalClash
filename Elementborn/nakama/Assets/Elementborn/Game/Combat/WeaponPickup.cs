using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A weapon resting on the map. Walk a weapon-user (anything with a WeaponHolder) over it to
    /// equip it; channelers have no WeaponHolder, so it ignores them. Scatter these across the world.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class WeaponPickup : MonoBehaviour
    {
        [SerializeField] private WeaponType type = WeaponType.Sword;
        [SerializeField] private WeaponMaterial material = WeaponMaterial.Metal;

        /// <summary>Sets the weapon this pickup grants (used by the world spawner).</summary>
        public void Configure(WeaponType weaponType, WeaponMaterial weaponMaterial)
        {
            type = weaponType;
            material = weaponMaterial;
        }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var holder = other.GetComponentInParent<WeaponHolder>();
            if (holder == null) return; // not a weapon-user (e.g., a channeler)
            holder.Equip(new WeaponInstance(type, material));
            Destroy(gameObject);
        }
    }
}
