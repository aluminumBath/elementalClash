using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using Elementborn.Game.Social;

namespace Elementborn.Game
{
    /// <summary>
    /// The single VR menu hub (with a desktop fallback). Pressing the left-hand <b>menu</b> button on a headset —
    /// or <b>Tab</b> on desktop — opens a panel with one button per overlay, each opening it through the overlay's
    /// public <c>Open()</c>. This is the VR entry point to Quests / Inventory / Grimoire / Map / Social / Character
    /// / Settings, since those panels are otherwise keyboard-only. Built via <see cref="OverlayUi"/> (so it's
    /// world-space in VR through <see cref="VrCanvasAdapter"/>); the bootstrap VR rig adds one.
    /// </summary>
    public sealed class VrOverlayHub : MonoBehaviour
    {
        [SerializeField] private XRNode menuHand = XRNode.LeftHand;
        [SerializeField] private Key desktopKey = Key.Tab;

        private Canvas _canvas;
        private bool _open;
        private bool _prevMenu;

        private void Awake() { Build(); Hide(); }

        private void Update()
        {
            // VR: left-hand menu button, rising edge (legacy XR input, matching VrInteractInput).
            var d = InputDevices.GetDeviceAtXRNode(menuHand);
            bool menuNow = d.isValid && d.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool m) && m;
            if (menuNow && !_prevMenu) Toggle();
            _prevMenu = menuNow;

            // Desktop fallback (and Esc to close).
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb[desktopKey].wasPressedThisFrame) Toggle();
                else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
            }
        }

        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; if (_canvas != null) _canvas.gameObject.SetActive(true); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Build()
        {
            var p = OverlayUi.Panel("VrOverlayHubCanvas", "Menu", 58, new Vector2(580, 800), Hide);
            _canvas = p.canvas;
            var content = p.content;

            Entry(content, "Quests",    () => FindObjectOfType<QuestLogController>()?.Open());
            Entry(content, "Inventory", () => FindObjectOfType<InventoryController>()?.Open());
            Entry(content, "Grimoire",  () => FindObjectOfType<GrimoireController>()?.Open());
            Entry(content, "Map",       () => FindObjectOfType<MapViewerController>()?.Open());
            Entry(content, "Social",    () => FindObjectOfType<SocialMenuController>()?.Open());
            Entry(content, "Character", () => FindObjectOfType<CharacterScreenController>()?.Open());
            Entry(content, "Settings",  () => FindObjectOfType<SettingsController>()?.Open());
            Entry(content, "Achievements", () => FindObjectOfType<AchievementsViewer>()?.Open());
            Entry(content, "Crafting",  () => FindObjectOfType<CraftingViewer>()?.Open());
            Entry(content, "Summon Beacon", () => FindObjectOfType<SummonViewer>()?.Open());
            Entry(content, "Save / Load", () => FindObjectOfType<SaveSlotController>()?.Show());

            OverlayUi.Header(content, "Actions");
            Entry(content, "Element Travel",   () => FindObjectOfType<ElementTravelController>()?.Toggle());
            Entry(content, "Summon Mount",     () => FindObjectOfType<MountSummoner>()?.Toggle());
            Entry(content, "Summon Companion", () => FindObjectOfType<CompanionSummoner>()?.Toggle());
        }

        private void Entry(Transform parent, string label, System.Action open)
        {
            // Close the hub, then open the chosen overlay. UiTheme.Button works with the EventSystem (and, in VR,
            // the XRI ray once its raycaster is on the rig).
            UiTheme.Button(parent, label, () => { Hide(); open?.Invoke(); }, 480, 42);
        }
    }
}
