using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The Wardrobe panel (default key <b>J</b>). Lists the cosmetic Channeler looks with the current one
    /// marked; tap an unlocked look to wear it. Looks unlock with player level, and the whole panel requires a
    /// Wardrobe built at your home. Appearance only — selecting a look never changes your element. Built via
    /// <see cref="OverlayUi"/> so it's world-space in VR; the bootstrap scene adds one.</summary>
    public sealed class WardrobeViewer : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.J;

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
            var p = OverlayUi.Panel("WardrobeCanvas", "Wardrobe", 57, new Vector2(720, 760), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var inv = PlayerInventory.Instance;
            if (inv == null) { OverlayUi.Body(_content, "No player.", 18); return; }

            if (!inv.Home.Has(HomeAddition.Wardrobe))
            {
                OverlayUi.Header(_content, "No Wardrobe yet");
                OverlayUi.Body(_content, "Build a Wardrobe at your home (claim a plot at level 5, then build the "
                    + "Wardrobe for 160 silver) to change your look.", 18);
                return;
            }

            int level = ProgressionController.Instance != null ? ProgressionController.Instance.Progression.Level : 1;
            var current = inv.Wardrobe.Current;

            OverlayUi.Header(_content, "Wearing:  " + WardrobeCatalog.DisplayName(current));
            OverlayUi.Body(_content, "Appearance only — your element never changes.", 16,
                new Color(0.70f, 0.72f, 0.78f, 1f));

            foreach (var look in WardrobeCatalog.All)
            {
                string name = WardrobeCatalog.DisplayName(look);
                bool isCurrent = look == current;

                if (WardrobeCatalog.IsUnlocked(look, level))
                {
                    string label = "Wear " + name + (isCurrent ? "     (worn)" : "");
                    var l = look;
                    UiTheme.Button(_content, label, () => DoSelect(l), 660, 44);
                }
                else
                {
                    OverlayUi.Body(_content, name + "   —  unlocks at level " + WardrobeCatalog.RequiredLevelFor(look),
                        18, new Color(0.62f, 0.64f, 0.70f, 1f));
                }
            }
        }

        private void DoSelect(ChannelerLook look)
        {
            WardrobeController.Instance?.SelectLook(look);
            Rebuild();
        }
    }
}
