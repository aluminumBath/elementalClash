using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A toggled character overlay (default key C) showing your level, XP toward the next level, and the bonuses
    /// it grants, plus your element. Refreshes live as you gain XP. Put one on a bootstrap object (the scene
    /// adds it).
    /// </summary>
    public sealed class CharacterScreenController : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.C;

        private Canvas _canvas;
        private Transform _content;
        private bool _open;
        private bool _hooked;

        private void Awake()
        {
            var p = OverlayUi.Panel("CharacterCanvas", "Character", 56, new Vector2(720, 520), Hide);
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
            if (_hooked || ProgressionController.Instance == null) return;
            ProgressionController.Instance.Changed += Refresh;
            _hooked = true;
        }

        private void Unhook()
        {
            if (!_hooked || ProgressionController.Instance == null) return;
            ProgressionController.Instance.Changed -= Refresh;
            _hooked = false;
        }

        private void Refresh() { if (_open) Rebuild(); }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var pc = ProgressionController.Instance;
            if (pc == null) { OverlayUi.Body(_content, "No progression loaded.", 20); return; }

            var p = pc.Progression;
            OverlayUi.Header(_content, "Level " + p.Level, 30);
            OverlayUi.Body(_content, "XP:  " + p.Xp + " / " + p.XpToNext + " to next level", 20);
            OverlayUi.Body(_content, "Bonus max health:  +" + p.BonusMaxHealth, 20,
                new Color(0.52f, 0.80f, 0.52f, 1f));

            var inv = PlayerInventory.Instance;
            if (inv != null)
            {
                string element = inv.PlayerElement.HasValue ? inv.PlayerElement.Value.ToString()
                    : (inv.PlayerIsConfluence ? "Confluence" : "—");
                OverlayUi.Header(_content, "Element", 22);
                OverlayUi.Body(_content, element, 20, new Color(0.80f, 0.82f, 0.88f, 1f));
            }

            var perks = pc.Perks;
            OverlayUi.Header(_content, "Perks   (" + perks.AvailablePoints + " point" + (perks.AvailablePoints == 1 ? "" : "s") + " to spend)", 22);
            foreach (var def in PerkCatalog.All)
            {
                int rank = perks.RankOf(def.Id);
                OverlayUi.Body(_content, def.Name + "   [" + rank + "/" + def.MaxRank + "]   " + def.Description, 16,
                    new Color(0.80f, 0.82f, 0.88f, 1f));
                if (perks.CanRank(def.Id))
                {
                    var id = def.Id;
                    UiTheme.Button(_content, "Spend a point on " + def.Name, () => pc.SpendPerk(id), 380, 44);
                }
            }
        }
    }
}
