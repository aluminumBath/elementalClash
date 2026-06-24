using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The equipment panel (default key <b>V</b>). Shows the slots with what is worn (armor slots can be enchanted with an element) and the running
    /// totals (+max HP, x power), and lists the gear in your bag with Equip/Unequip buttons. Equipping requires
    /// owning the item and re-applies the stat bonuses (max health via progression, power via combat). Built via
    /// <see cref="OverlayUi"/>, so it's world-space in VR. The bootstrap scene adds one.</summary>
    public sealed class EquipmentViewer : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.V;

        private Canvas _canvas;
        private Transform _content;
        private bool _open;

        private void Awake() { Build(); Hide(); }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; if (_canvas != null) _canvas.gameObject.SetActive(true); Rebuild(); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Build()
        {
            var p = OverlayUi.Panel("EquipmentCanvas", "Equipment", 56, new Vector2(740, 720), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var eq = EquipmentController.Instance;
            var loadout = eq != null ? eq.Loadout : null;

            OverlayUi.Header(_content, "Worn   (+" + (eq != null ? eq.MaxHealthBonus : 0) + " max HP,  x"
                + (eq != null ? eq.OffenseMultiplier.ToString("0.00") : "1.00") + " power)");

            foreach (EquipSlot slot in System.Enum.GetValues(typeof(EquipSlot)))
            {
                string worn = loadout != null ? loadout.EquippedIn(slot) : null;
                if (!string.IsNullOrEmpty(worn))
                {
                    var s = slot;
                    UiTheme.Button(_content, slot + ":  " + Name(worn) + "     —  Unequip", () => DoUnequip(s), 660, 42);
                    if (EquipLoadout.IsArmorSlot(slot))
                    {
                        var e = loadout.EnchantIn(slot);
                        string label = "      ⤷ Enchant: " + (e.HasValue ? e.Value.ToString() : "None") + "   (tap to change)";
                        UiTheme.Button(_content, label, () => DoCycleEnchant(s), 660, 36);
                    }
                }
                else
                {
                    OverlayUi.Body(_content, slot + ":  (empty)", 18, new Color(0.70f, 0.72f, 0.78f, 1f));
                }
            }

            OverlayUi.Header(_content, "Gear in your bag");
            bool any = false;
            var pi = PlayerInventory.Instance;
            if (pi != null)
                foreach (var e in pi.Items.Entries())
                {
                    if (!GearCatalog.TryGet(e.Key, out var g)) continue;
                    if (loadout != null && loadout.IsEquipped(e.Key)) continue; // already worn
                    any = true;
                    string id = e.Key;
                    string bonus = (g.BonusMaxHealth > 0 ? "+" + g.BonusMaxHealth + " HP  " : "") +
                                   (g.OffenseBonus > 0f ? "+" + Mathf.RoundToInt(g.OffenseBonus * 100f) + "% power" : "");
                    UiTheme.Button(_content, "Equip " + Name(id) + "  [" + g.Slot + "]   " + bonus, () => DoEquip(id), 660, 42);
                }
            if (!any) OverlayUi.Body(_content, "No gear in your bag yet — craft armor (Iron Helm, Warding Cloak, Tough Leather, Sturdy Boots) or an Elemental Charm (B).", 18);
        }

        // None -> Fire -> Water -> Earth -> Air -> None
        private static readonly Element[] EnchantCycle = { Element.Fire, Element.Water, Element.Earth, Element.Air };
        private void DoCycleEnchant(EquipSlot slot)
        {
            var eq = EquipmentController.Instance;
            if (eq == null) return;
            var cur = eq.Loadout.EnchantIn(slot);
            int next = cur.HasValue ? System.Array.IndexOf(EnchantCycle, cur.Value) + 1 : 0;
            if (next < 0 || next >= EnchantCycle.Length) eq.ClearEnchant(slot);
            else eq.Enchant(slot, EnchantCycle[next]);
            var now = eq.Loadout.EnchantIn(slot);
            GameHud.Instance?.Toast("Enchant: " + (now.HasValue ? now.Value.ToString() : "None"));
            AudioController.Instance?.Confirm();
            Rebuild();
        }

        private void DoEquip(string itemId)
        {
            var eq = EquipmentController.Instance;
            if (eq != null && eq.Equip(itemId))
            {
                GameHud.Instance?.Toast("Equipped " + Name(itemId));
                AudioController.Instance?.Confirm();
            }
            Rebuild();
        }

        private void DoUnequip(EquipSlot slot)
        {
            EquipmentController.Instance?.Unequip(slot);
            AudioController.Instance?.Back();
            Rebuild();
        }

        private static string Name(string itemId) => ItemCatalog.Get(itemId)?.Name ?? itemId;
    }
}
