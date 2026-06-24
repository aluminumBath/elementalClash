using System.Collections.Generic;

namespace Elementborn.Core
{
    public enum EquipSlot { Armor, Charm, Trinket }

    /// <summary>An item that can be worn in a slot, with the passive bonuses it grants.</summary>
    public readonly struct GearDef
    {
        public readonly string ItemId;
        public readonly EquipSlot Slot;
        public readonly int BonusMaxHealth;
        public readonly float OffenseBonus; // additive fraction, e.g. 0.15 = +15% outgoing power

        public GearDef(string itemId, EquipSlot slot, int bonusMaxHealth, float offenseBonus)
        {
            ItemId = itemId; Slot = slot; BonusMaxHealth = bonusMaxHealth; OffenseBonus = offenseBonus;
        }
    }

    /// <summary>Which items are gear and what they do. Every id here is a real <see cref="ItemCatalog"/> item
    /// (a test asserts it).</summary>
    public static class GearCatalog
    {
        private static readonly Dictionary<string, GearDef> ById = new Dictionary<string, GearDef>
        {
            { "hide",            new GearDef("hide",            EquipSlot.Armor,   8, 0f) },
            { "tough_leather",   new GearDef("tough_leather",   EquipSlot.Armor,  30, 0f) },
            { "elemental_charm", new GearDef("elemental_charm", EquipSlot.Charm,  15, 0.15f) },
            { "old_relic",       new GearDef("old_relic",       EquipSlot.Trinket, 0, 0.10f) },
        };

        public static bool IsGear(string itemId) => itemId != null && ById.ContainsKey(itemId);

        public static bool TryGet(string itemId, out GearDef def)
        {
            if (itemId != null && ById.TryGetValue(itemId, out def)) return true;
            def = default;
            return false;
        }

        public static IEnumerable<GearDef> All => ById.Values;
    }

    /// <summary>
    /// What's worn in each slot and the bonuses that follow. Equipping a gear item drops it into its slot
    /// (replacing whatever was there); the aggregate <see cref="MaxHealthBonus"/> and <see cref="OffenseMultiplier"/>
    /// are read by the progression and combat layers. Pure and unit-tested; savable by slot.
    /// </summary>
    public sealed class EquipLoadout
    {
        private readonly Dictionary<EquipSlot, string> _equipped = new Dictionary<EquipSlot, string>();

        public string EquippedIn(EquipSlot slot) => _equipped.TryGetValue(slot, out var id) ? id : null;

        public bool IsEquipped(string itemId)
        {
            foreach (var kv in _equipped) if (kv.Value == itemId) return true;
            return false;
        }

        /// <summary>Wear a gear item in its slot. False if the item isn't gear.</summary>
        public bool Equip(string itemId)
        {
            if (!GearCatalog.TryGet(itemId, out var def)) return false;
            _equipped[def.Slot] = itemId;
            return true;
        }

        public void Unequip(EquipSlot slot) => _equipped.Remove(slot);

        public int MaxHealthBonus
        {
            get
            {
                int n = 0;
                foreach (var kv in _equipped) if (GearCatalog.TryGet(kv.Value, out var d)) n += d.BonusMaxHealth;
                return n;
            }
        }

        public float OffenseMultiplier
        {
            get
            {
                float m = 1f;
                foreach (var kv in _equipped) if (GearCatalog.TryGet(kv.Value, out var d)) m += d.OffenseBonus;
                return m;
            }
        }

        // Persistence: one entry per slot, in enum order ("" = empty).
        public IReadOnlyList<string> ToSave()
        {
            var slots = (EquipSlot[])System.Enum.GetValues(typeof(EquipSlot));
            var arr = new string[slots.Length];
            for (int i = 0; i < slots.Length; i++) arr[i] = EquippedIn(slots[i]) ?? "";
            return arr;
        }

        public void Load(IReadOnlyList<string> saved)
        {
            _equipped.Clear();
            if (saved == null) return;
            var slots = (EquipSlot[])System.Enum.GetValues(typeof(EquipSlot));
            for (int i = 0; i < slots.Length && i < saved.Count; i++)
                if (!string.IsNullOrEmpty(saved[i])) _equipped[slots[i]] = saved[i];
        }
    }
}
