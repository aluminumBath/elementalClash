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
            if (!PlayerInventoryTracker.HasItemId(itemId, 1)) return false; // must possess it to wear it
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

        /// <summary>Imbue the armor worn in an armor slot with an element (additive elemental resistance). False if
        /// the slot isn't armor or is empty.</summary>
        public bool Enchant(EquipSlot slot, Element element)
        {
            if (!_loadout.Enchant(slot, element)) return false;
            PushArmor();
            return true;
        }

        public void ClearEnchant(EquipSlot slot) { _loadout.ClearEnchant(slot); PushArmor(); }

        // Register the loadout's elemental resistances on the player's body. The delegate reads live loadout state,
        // so equipping/enchanting after this is reflected automatically; we just (re)bind it defensively.
        private Damageable _body;
        private void Start() => PushArmor();
        private void PushArmor()
        {
            if (_body == null)
                _body = GetComponent<Damageable>() ?? GetComponentInParent<Damageable>() ?? GetComponentInChildren<Damageable>();
            if (_body == null) // fallback: bind to the player's own Damageable wherever this controller lives
            {
                var combat = FindObjectOfType<PlayerCombatController>();
                if (combat != null) _body = combat.GetComponentInParent<Damageable>();
            }
            if (_body != null) _body.IncomingElementScale = _loadout.IncomingMultiplier;
        }

        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.equippedSlots.Clear();
            foreach (var s in _loadout.ToSave()) d.equippedSlots.Add(s);
            d.equippedEnchants.Clear();
            foreach (var s in _loadout.ToSaveEnchants()) d.equippedEnchants.Add(s);
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            _loadout.Load(d.equippedSlots);
            _loadout.LoadEnchants(d.equippedEnchants); // after Load(worn) so enchants on empty slots drop
            ProgressionController.Instance?.RefreshBonus();
            PushArmor();
        }
    }
}
