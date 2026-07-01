using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The crafting panel (default key <b>B</b>; also on the VR hub). Lists every <see cref="RecipeBook"/>
    /// recipe with its output and inputs (and how many you hold); clicking crafts it — consuming the inputs from
    /// the player's inventory and granting the output (which also counts toward item achievements). Built via
    /// <see cref="OverlayUi"/>, so it's world-space in VR. The bootstrap scene adds one.</summary>
    public sealed class CraftingViewer : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.B;

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
            var p = OverlayUi.Panel("CraftingCanvas", Localization.T("ui.title.crafting"), 56, new Vector2(740, 720), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var have = Snapshot();
            OverlayUi.Header(_content, "Recipes  (numbers in parentheses are what you hold)");
            foreach (var r in RecipeBook.All)
            {
                var recipe = r; // capture for the closure
                string outName = ItemCatalog.Get(r.OutputItemId)?.Name ?? r.OutputItemId;
                string label = (Crafting.CanCraft(r, have) ? "" : "(need) ") +
                               outName + (r.OutputCount > 1 ? " x" + r.OutputCount : "") + "   <   " + Inputs(r, have);
                UiTheme.Button(_content, label, () => Craft(recipe), 660, 46);
            }
        }

        private static Dictionary<string, int> Snapshot()
        {
            var have = new Dictionary<string, int>();
            foreach (var e in PlayerInventoryTracker.EntriesById()) have[e.Key] = e.Value;
            return have;
        }

        private static string Inputs(Recipe r, IReadOnlyDictionary<string, int> have)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < r.Inputs.Count; i++)
            {
                var ing = r.Inputs[i];
                if (i > 0) sb.Append(", ");
                have.TryGetValue(ing.ItemId, out int held);
                sb.Append(ing.Count).Append(' ').Append(ItemCatalog.Get(ing.ItemId)?.Name ?? ing.ItemId)
                  .Append(" (").Append(held).Append(')');
            }
            return sb.ToString();
        }

        private void Craft(Recipe r)
        {
            var pi = PlayerInventory.Instance;
            if (pi == null) return;

            if (!Crafting.CanCraft(r, Snapshot()))
            {
                GameHud.Instance?.Toast("Missing materials for " + r.Name);
                AudioController.Instance?.Back();
                return;
            }

            foreach (var ing in r.Inputs) PlayerInventoryTracker.RemoveItemId(ing.ItemId, ing.Count);
            pi.AddItem(r.OutputItemId, r.OutputCount); // grants + counts toward the Collector achievement
            QuestEvents.RaiseItemCrafted(r.OutputItemId);

            string outName = ItemCatalog.Get(r.OutputItemId)?.Name ?? r.OutputItemId;
            GameHud.Instance?.Toast("Crafted " + outName + (r.OutputCount > 1 ? " x" + r.OutputCount : ""));
            AudioController.Instance?.Confirm();
            Rebuild();
        }
    }
}
