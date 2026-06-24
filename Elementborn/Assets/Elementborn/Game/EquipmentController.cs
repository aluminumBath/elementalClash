using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Holds the player's worn gear (<see cref="EquipLoadout"/>) and exposes its bonuses: <see cref="MaxHealthBonus"/>
    /// is folded into <see cref="ProgressionController"/>'s max-health calculation, and <see cref="OffenseMultiplier"/>
    /// scales outgoing ability power in <see cref="PlayerCombatController"/>. Equipping requires owning the item;
    /// state persists through <see cref="PlayerInventory"/>. The bootstrap scene adds one.
    /// </summary>
    public sealed class EquipmentController : MonoBehaviour
    {
        public static EquipmentController Instance { get; private set; }

        private readonly EquipLoadout _loadout = new EquipLoadout();
        public EquipLoadout Loadout => _loadout;

        public int MaxHealthBonus => _loadout.MaxHealthBonus;
        public float OffenseMultiplier => _loadout.OffenseMultiplier;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Equip a gear item the player owns; re-applies the max-health bonus. False if not gear or not owned.</summary>
        public bool Equip(string itemId)
        {
            if (!GearCatalog.IsGear(itemId)) return false;
            var pi = PlayerInventory.Instance;
            if (pi != null && !pi.Items.Has(itemId, 1)) return false; // must possess it to wear it
            if (!_loadout.Equip(itemId)) return false;
            ProgressionController.Instance?.RefreshBonus();
            QuestEvents.RaiseItemEquipped(itemId);
            return true;
        }

        public void Unequip(EquipSlot slot)
        {
            _loadout.Unequip(slot);
            ProgressionController.Instance?.RefreshBonus();
        }

        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.equippedSlots.Clear();
            foreach (var s in _loadout.ToSave()) d.equippedSlots.Add(s);
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            _loadout.Load(d.equippedSlots);
            ProgressionController.Instance?.RefreshBonus();
        }
    }
}
