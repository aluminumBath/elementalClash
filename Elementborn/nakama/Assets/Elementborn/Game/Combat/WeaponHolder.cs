using System;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Holds the equipped weapon for a player without an element. Map pickups call
    /// <see cref="Equip"/>; the weapon shatters when its owner takes a hit matching the
    /// material's weakness (wood↔fire, metal↔oreshaping, ice↔water channeling), leaving the
    /// player unarmed until they grab another from the map.
    /// </summary>
    public sealed class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private Damageable owner;

        public bool HasWeapon { get; private set; }
        public WeaponInstance Current { get; private set; }

        public event Action<WeaponInstance> Equipped;
        public event Action Broke;

        private void Start()
        {
            if (owner == null) owner = GetComponent<Damageable>();
            if (owner != null) owner.Health.Damaged += OnOwnerDamaged;
        }

        private void OnDestroy()
        {
            if (owner != null && owner.Health != null) owner.Health.Damaged -= OnOwnerDamaged;
        }

        public void Equip(WeaponInstance weapon)
        {
            Current = weapon;
            HasWeapon = true;
            Equipped?.Invoke(weapon);
        }

        public void Break()
        {
            HasWeapon = false;
            Broke?.Invoke();
        }

        private void OnOwnerDamaged(DamageInfo damage)
        {
            if (HasWeapon && Weapons.IsBrokenBy(Current, damage)) Break();
        }
    }
}
