using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A toggled inventory overlay (default key I) listing the player's items grouped by category, with counts and
    /// each item's value. Read-only for now — buying and selling happen at a merchant; this is the "what do I
    /// have" view. Refreshes live as items are gained or spent. Put one on a bootstrap object (the scene adds it).
    /// </summary>
    public sealed class InventoryController : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.I;

        private Canvas _canvas;
        private Transform _content;
        private bool _open;
        private bool _hooked;

        private void Awake()
        {
            var p = OverlayUi.Panel("InventoryCanvas", "Inventory", 56, new Vector2(760, 620), Hide);
            _canvas = p.canvas; _content = p.content;
            Hide();
        }

        private void OnDestroy() => Unhook();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }

        private void Show()
        {
            _open = true;
            Hook();
            if (_canvas != null) _canvas.gameObject.SetActive(true);
            Rebuild();
        }

        private void Hide()
        {
            _open = false;
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        private void Hook()
        {
            if (_hooked) return;
            var inv = PlayerInventory.Instance;
            if (inv != null) { inv.Items.Changed += Refresh; _hooked = true; }
        }

        private void Unhook()
        {
            if (!_hooked) return;
            var inv = PlayerInventory.Instance;
            if (inv != null) inv.Items.Changed -= Refresh;
            _hooked = false;
        }

        private void Refresh() { if (_open) Rebuild(); }

        private void Use(string itemId)
        {
            if (!Consumables.TryGet(itemId, out var effect)) return;
            var pi = PlayerInventory.Instance;
            if (pi == null || !pi.Items.Has(itemId, 1)) return;
            pi.Items.Remove(itemId, 1);

            var tagged = GameObject.FindGameObjectWithTag("Player");
            var dmg = tagged != null ? tagged.GetComponentInParent<Damageable>() : null;
            if (effect.Heal > 0 && dmg != null && dmg.Health != null) dmg.Health.Heal(effect.Heal);
            if (effect.RefillStamina) StaminaController.Instance?.Refill();

            GameHud.Instance?.Toast("Used " + (ItemCatalog.Get(itemId)?.Name ?? itemId));
            AudioController.Instance?.Confirm();
            Rebuild();
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var inv = PlayerInventory.Instance;
            var entries = inv != null ? inv.Items.Entries() : null;
            if (entries == null || entries.Count == 0)
            {
                OverlayUi.Body(_content, "Empty. Defeat creatures for drops, or buy from a merchant.", 20);
                return;
            }

            foreach (ItemCategory cat in System.Enum.GetValues(typeof(ItemCategory)))
            {
                bool headerShown = false;
                foreach (var e in entries)
                {
                    var def = ItemCatalog.Get(e.Key);
                    if (def == null || def.Category != cat) continue;
                    if (!headerShown) { OverlayUi.Header(_content, cat.ToString(), 22); headerShown = true; }
                    if (Consumables.IsConsumable(e.Key))
                    {
                        string id = e.Key; // capture for the closure
                        UiTheme.Button(_content, def.Name + "   x" + e.Value + "    —  Use", () => Use(id), 660, 42);
                    }
                    else
                    {
                        OverlayUi.Body(_content, def.Name + "   x" + e.Value + "   (" + def.Value + " ea)", 18,
                            new Color(0.80f, 0.82f, 0.88f, 1f));
                    }
                }
            }
        }
    }
}
