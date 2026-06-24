using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>Worn-gear slots: four armor pieces — each separately enchantable with an element — plus a charm
    /// and a trinket.</summary>
    public enum EquipSlot { Helmet, Cloak, Chest, Boots, Charm, Trinket }

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
            { "iron_helm",       new GearDef("iron_helm",       EquipSlot.Helmet,  10, 0f) },
            { "warding_cloak",   new GearDef("warding_cloak",   EquipSlot.Cloak,   10, 0f) },
            { "hide",            new GearDef("hide",            EquipSlot.Chest,    8, 0f) },
            { "tough_leather",   new GearDef("tough_leather",   EquipSlot.Chest,   30, 0f) },
            { "sturdy_boots",    new GearDef("sturdy_boots",    EquipSlot.Boots,   10, 0f) },
            { "elemental_charm", new GearDef("elemental_charm", EquipSlot.Charm,   15, 0.15f) },
            { "old_relic",       new GearDef("old_relic",       EquipSlot.Trinket,  0, 0.10f) },
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
    /// What's worn in each slot, the bonuses that follow, and the element each armor piece is enchanted with.
    /// Equipping a gear item drops it into its slot (replacing whatever was there, and clearing that slot's old
    /// enchant); the aggregate <see cref="MaxHealthBonus"/>/<see cref="OffenseMultiplier"/> feed progression and
    /// combat, and the enchanted-element set feeds <see cref="ArmorResist"/>. Pure and unit-tested; savable by slot.
    /// </summary>
    public sealed class EquipLoadout
    {
        private readonly Dictionary<EquipSlot, string> _equipped = new Dictionary<EquipSlot, string>();
        private readonly Dictionary<EquipSlot, Element> _enchant = new Dictionary<EquipSlot, Element>();

        /// <summary>The four armor slots (the ones that can be enchanted); charm and trinket are not armor.</summary>
        public static bool IsArmorSlot(EquipSlot slot) =>
            slot == EquipSlot.Helmet || slot == EquipSlot.Cloak || slot == EquipSlot.Chest || slot == EquipSlot.Boots;

        public string EquippedIn(EquipSlot slot) => _equipped.TryGetValue(slot, out var id) ? id : null;

        public bool IsEquipped(string itemId)
        {
            foreach (var kv in _equipped) if (kv.Value == itemId) return true;
            return false;
        }

        /// <summary>Wear a gear item in its slot. False if the item isn't gear. A fresh piece carries no enchant.</summary>
        public bool Equip(string itemId)
        {
            if (!GearCatalog.TryGet(itemId, out var def)) return false;
            _equipped[def.Slot] = itemId;
            _enchant.Remove(def.Slot);
            return true;
        }

        public void Unequip(EquipSlot slot) { _equipped.Remove(slot); _enchant.Remove(slot); }

        // --- Enchanting: the piece worn in an armor slot can be imbued with one element. ---

        /// <summary>Imbue the armor worn in this slot with an element. False if the slot isn't armor or is empty.</summary>
        public bool Enchant(EquipSlot slot, Element element)
        {
            if (!IsArmorSlot(slot) || !_equipped.ContainsKey(slot)) return false;
            _enchant[slot] = element;
            return true;
        }

        public void ClearEnchant(EquipSlot slot) => _enchant.Remove(slot);

        public Element? EnchantIn(EquipSlot slot) => _enchant.TryGetValue(slot, out var e) ? e : (Element?)null;

        /// <summary>The enchant element of every armor slot that currently has a piece worn.</summary>
        public IEnumerable<Element> ActiveEnchants
        {
            get { foreach (var kv in _enchant) if (_equipped.ContainsKey(kv.Key)) yield return kv.Value; }
        }

        /// <summary>Incoming-damage multiplier vs an element, summed across enchanted armor (see <see cref="ArmorResist"/>).</summary>
        public float IncomingMultiplier(Element incoming) => ArmorResist.IncomingMultiplier(ActiveEnchants, incoming);

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

        // Persistence: worn item per slot, in enum order ("" = empty).
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

        // Enchant element per slot, in enum order ("" = none). Load AFTER Load(worn) so empty slots drop their enchant.
        public IReadOnlyList<string> ToSaveEnchants()
        {
            var slots = (EquipSlot[])System.Enum.GetValues(typeof(EquipSlot));
            var arr = new string[slots.Length];
            for (int i = 0; i < slots.Length; i++) { var e = EnchantIn(slots[i]); arr[i] = e.HasValue ? e.Value.ToString() : ""; }
            return arr;
        }

        public void LoadEnchants(IReadOnlyList<string> saved)
        {
            _enchant.Clear();
            if (saved == null) return;
            var slots = (EquipSlot[])System.Enum.GetValues(typeof(EquipSlot));
            for (int i = 0; i < slots.Length && i < saved.Count; i++)
                if (!string.IsNullOrEmpty(saved[i]) && System.Enum.TryParse(saved[i], out Element e) && _equipped.ContainsKey(slots[i]))
                    _enchant[slots[i]] = e;
        }
    }
}
