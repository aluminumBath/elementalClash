using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The Home menu (default key <b>H</b>). Build additions onto your claimed home and use the ones you've
    /// built: Workshop opens crafting, Enchanting Table opens equipment, Wardrobe opens the look picker, the Garden
    /// harvests the silver it has grown, Storage deposits/withdraws items, and the Stable houses your creatures.
    /// Built via <see cref="OverlayUi"/> so it's world-space in VR; the bootstrap scene adds one.</summary>
    public sealed class HomeMenuViewer : MonoBehaviour
    {
        private enum Mode { Main, Storage, Stable }

        [SerializeField] private Key toggleKey = Key.H;
        private static readonly Color Dim = new Color(0.70f, 0.72f, 0.78f, 1f);

        private Canvas _canvas;
        private Transform _content;
        private bool _open;
        private Mode _mode = Mode.Main;

        private void Awake() { Build(); Hide(); }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame)
            {
                if (_mode != Mode.Main) { _mode = Mode.Main; Rebuild(); }
                else Hide();
            }
        }

        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; _mode = Mode.Main; if (_canvas != null) _canvas.gameObject.SetActive(true); Rebuild(); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Build()
        {
            var p = OverlayUi.Panel("HomeCanvas", "Home", 58, new Vector2(740, 800), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var inv = PlayerInventory.Instance;
            if (inv == null) { OverlayUi.Body(_content, "No player.", 18); return; }

            if (!inv.HasHouse)
            {
                OverlayUi.Header(_content, "No home yet");
                OverlayUi.Body(_content, "Claim a home plot (reach level 5, then claim at the plot marker) to start "
                    + "building stations here.", 18);
                return;
            }

            switch (_mode)
            {
                case Mode.Storage: RebuildStorage(); break;
                case Mode.Stable:  RebuildStable();  break;
                default:           RebuildMain(inv); break;
            }
        }

        private void RebuildMain(PlayerInventory inv)
        {
            OverlayUi.Header(_content, "Home   (silver: " + inv.Wallet.CountOf(Currency.Silver) + ")");
            foreach (var a in HomesteadCatalog.All)
            {
                if (inv.Home.Has(a)) AddBuiltRow(inv, a);
                else AddBuildRow(a);
            }
        }

        private void AddBuiltRow(PlayerInventory inv, HomeAddition a)
        {
            switch (a)
            {
                case HomeAddition.Workshop:
                    UiTheme.Button(_content, "Open Workshop  —  Crafting", OpenCrafting, 660, 44); break;
                case HomeAddition.EnchantingTable:
                    UiTheme.Button(_content, "Open Enchanting Table  —  Equipment", OpenEquipment, 660, 44); break;
                case HomeAddition.Wardrobe:
                    UiTheme.Button(_content, "Open Wardrobe  —  change your look", OpenWardrobe, 660, 44); break;
                case HomeAddition.Garden:
                    UiTheme.Button(_content, "Harvest Garden  (" + inv.Garden.Ready + " silver ready)", DoHarvest, 660, 44); break;
                case HomeAddition.Storage:
                    UiTheme.Button(_content, "Open Storage  —  deposit & withdraw", () => { _mode = Mode.Storage; Rebuild(); }, 660, 44); break;
                case HomeAddition.Stable:
                    UiTheme.Button(_content, "Open Stable  —  house your creatures", () => { _mode = Mode.Stable; Rebuild(); }, 660, 44); break;
            }
        }

        private void AddBuildRow(HomeAddition a)
        {
            string label = "Build " + HomesteadCatalog.DisplayName(a) + "  —  " + HomesteadCatalog.CostOf(a)
                + " silver  (Lv " + HomesteadCatalog.RequiredLevelFor(a) + ")";
            var add = a;
            UiTheme.Button(_content, label, () => DoBuild(add), 660, 42);
        }

        // ---- Storage: deposit/withdraw whole stacks. Shows BOTH inventory pools (read-only check) and lets you
        // deposit from either — non-destructive: items land in the shared chest, withdrawable. This is migration
        // step 1; nothing is auto-moved or deleted. ----
        private void RebuildStorage()
        {
            UiTheme.Button(_content, "< Back", BackToMain, 200, 40);
            OverlayUi.Header(_content, "Home Storage");

            var storage = HomeStationsController.Instance != null ? HomeStationsController.Instance.Storage : null;
            if (storage == null) { OverlayUi.Body(_content, "Storage unavailable.", 18); return; }

            var inv = PlayerInventory.Instance;
            var tracker = PlayerInventoryTracker.Instance;

            // Read-only check: how many items sit in each of the two inventory pools right now.
            int bagCount = 0;
            if (tracker != null) foreach (var st in tracker.Stacks) if (!st.IsEmpty) bagCount += st.Quantity;
            int legacyCount = 0;
            if (inv != null) foreach (var e in inv.Items.Entries()) legacyCount += e.Value;
            OverlayUi.Body(_content, "Bag: " + bagCount + " items     Legacy bag: " + legacyCount + " items", 16, Dim);

            OverlayUi.Header(_content, "Stored");
            bool anyStored = false;
            foreach (var st in storage.Stacks)
            {
                if (st.IsEmpty) continue;
                anyStored = true;
                string id = st.ResolvedItemId; int q = st.Quantity;
                UiTheme.Button(_content, "Withdraw " + st.DisplayName + " x" + q,
                    () => { storage.TransferToPlayer(id, q); Rebuild(); }, 660, 40);
            }
            if (!anyStored) OverlayUi.Body(_content, "  (empty)", 16, Dim);

            OverlayUi.Header(_content, "Your bag");
            bool anyBag = false;
            if (tracker != null)
                foreach (var st in tracker.Stacks)
                {
                    if (st.IsEmpty) continue;
                    anyBag = true;
                    string id = st.ResolvedItemId; int q = st.Quantity;
                    UiTheme.Button(_content, "Deposit " + st.DisplayName + " x" + q,
                        () => Deposit(storage, id, q), 660, 40);
                }
            if (!anyBag) OverlayUi.Body(_content, "  (nothing to deposit)", 16, Dim);

            // Legacy pool (crafting/equipment inventory). Depositing here moves items into the shared chest so they
            // can be withdrawn into the main bag — a safe, player-driven first step toward one unified inventory.
            OverlayUi.Header(_content, "Your bag (legacy)");
            bool anyLegacy = false;
            if (inv != null)
                foreach (var e in inv.Items.Entries())
                {
                    if (e.Value <= 0) continue;
                    anyLegacy = true;
                    string id = e.Key; int q = e.Value;
                    string nm = ItemCatalog.Get(id)?.Name ?? id;
                    UiTheme.Button(_content, "Deposit " + nm + " x" + q, () => DepositLegacy(storage, id, q), 660, 40);
                }
            if (!anyLegacy) OverlayUi.Body(_content, "  (legacy bag empty)", 16, Dim);
        }

        private void Deposit(StorageContainerInventory storage, string itemId, int qty)
        {
            if (PlayerInventoryTracker.HasItemId(itemId, qty))
            {
                var add = storage.AddId(itemId, qty);
                if (add.Moved > 0) PlayerInventoryTracker.RemoveItemId(itemId, add.Moved);
            }
            Rebuild();
        }

        // Deposit from the legacy (crafting/equipment) pool: only remove what the chest actually accepted.
        private void DepositLegacy(StorageContainerInventory storage, string itemId, int qty)
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return;
            var add = storage.AddId(itemId, qty);
            if (add.Moved > 0) inv.Items.Remove(itemId, add.Moved);
            Rebuild();
        }

        // ---- Stable: move creatures between following you and housed at home ----
        private void RebuildStable()
        {
            UiTheme.Button(_content, "< Back", BackToMain, 200, 40);
            OverlayUi.Header(_content, "Creature Stable");

            var stable = HomeStationsController.Instance != null ? HomeStationsController.Instance.Stable : null;
            if (stable == null) { OverlayUi.Body(_content, "Stable unavailable.", 18); return; }

            OverlayUi.Body(_content, "Housed: " + stable.StoredCreatureRecordIds.Count + " / " + stable.Capacity, 18, Dim);
            UiTheme.Button(_content, "Stable your active follower", () =>
            {
                bool ok = stable.StoreFirstAvailable();
                GameHud.Instance?.Toast(ok ? "Housed a creature in the stable." : "No follower to house (or stable full).");
                if (ok) AudioController.Instance?.Confirm();
                Rebuild();
            }, 660, 44);
            UiTheme.Button(_content, "Release one to follow you", () =>
            {
                bool ok = stable.ReleaseFirstStored();
                GameHud.Instance?.Toast(ok ? "Released a creature to follow you." : "No creature housed.");
                if (ok) AudioController.Instance?.Confirm();
                Rebuild();
            }, 660, 44);
        }

        private void BackToMain() { _mode = Mode.Main; Rebuild(); }

        private void DoBuild(HomeAddition a) { HousingController.Instance?.TryBuild(a); Rebuild(); }

        private void DoHarvest()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return;
            int amount = inv.Garden.Harvest();
            if (amount > 0)
            {
                inv.AddCurrency(Currency.Silver, amount);
                GameHud.Instance?.Toast("Harvested " + amount + " silver from your garden.");
                AudioController.Instance?.Confirm();
            }
            else GameHud.Instance?.Toast("Nothing ready to harvest yet.");
            Rebuild();
        }

        // Hand off to the relevant panel (and close this one so they don't stack).
        private void OpenCrafting() { Hide(); Object.FindAnyObjectByType<CraftingViewer>()?.Open(); }
        private void OpenEquipment() { Hide(); Object.FindAnyObjectByType<EquipmentViewer>()?.Open(); }
        private void OpenWardrobe() { Hide(); Object.FindAnyObjectByType<WardrobeViewer>()?.Open(); }
    }
}
