using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>The title screen — the game's front door, shown by <see cref="GameFlowController"/> before any
    /// stage runs. Four actions: New Game (into character creation), Continue (load the saved journey; disabled
    /// when none exists), Settings (opens the shared settings overlay on top), and Quit. Built on the same
    /// world-space <see cref="OverlayUi"/> panel as every other screen, so it works in VR and on flat/desktop
    /// from one code path. The owning <see cref="GameFlowController"/> tears this down (destroying its child
    /// canvas with it) the moment New Game or Continue is chosen.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private System.Action _onNewGame, _onContinue, _onSettings, _onQuit;
        private bool _hasSave;
        private Canvas _canvas;

        /// <summary>Build and show the menu. <paramref name="hasSave"/> gates the Continue button.</summary>
        public void Show(bool hasSave, System.Action onNewGame, System.Action onContinue,
                         System.Action onSettings, System.Action onQuit)
        {
            _hasSave = hasSave;
            _onNewGame = onNewGame;
            _onContinue = onContinue;
            _onSettings = onSettings;
            _onQuit = onQuit;
            Build();
        }

        private void Update()
        {
            // The menu owns the cursor while it's up. The settings overlay's close handler relocks the cursor
            // for gameplay, so re-assert visibility here — harmless in VR (no system cursor), needed on desktop.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Build()
        {
            // Esc / the corner Close button exits the game — there's nowhere to back out to from the title.
            var panel = OverlayUi.Panel("MainMenuCanvas", "ELEMENTBORN", 220,
                new Vector2(640, 700), () => _onQuit?.Invoke());
            _canvas = panel.canvas;
            _canvas.transform.SetParent(transform, false); // so destroying this GameObject cleans up the canvas
            var content = panel.content;

            OverlayUi.Body(content, "Channel the elements. Mend the Convergence.", 20,
                new Color(0.72f, 0.80f, 0.88f));
            OverlayUi.Body(content, " ", 8); // breathing room before the buttons

            UiTheme.Button(content, "New Game", () => _onNewGame?.Invoke(), 540, 62);

            var cont = UiTheme.Button(content, "Continue", () => _onContinue?.Invoke(), 540, 62);
            if (!_hasSave)
            {
                cont.interactable = false;
                OverlayUi.Body(content, "(No saved journey yet)", 16, new Color(0.62f, 0.62f, 0.68f));
            }

            UiTheme.Button(content, "Save Slots", () => SaveSlotController.EnsureInstance().Show(), 540, 62);
            UiTheme.Button(content, "Settings", () => _onSettings?.Invoke(), 540, 62);
            UiTheme.Button(content, "How to Play", ShowHowToPlay, 540, 62);
            UiTheme.Button(content, "Credits", ShowCredits, 540, 62);
            UiTheme.Button(content, "Quit", () => _onQuit?.Invoke(), 540, 62);

            AudioController.Instance?.Confirm();
        }

        private void ShowHowToPlay() => ShowOverlay("HowToPlayCanvas", "How to Play", c =>
        {
            OverlayUi.Body(c, "Channel the four elements — Fire, Water, Earth, Air — to mend the Convergence.", 18);
            OverlayUi.Header(c, "Controls");
            OverlayUi.Body(c, "Move: WASD / left stick      Look: mouse / right stick", 16);
            OverlayUi.Body(c, "Channel & attack: hand triggers (mouse buttons on flat)", 16);
            OverlayUi.Body(c, "Equipment: V      Crafting: B      Summon Beacon: U", 16);
            OverlayUi.Body(c, "Wardrobe: J      Home: H      Map: M      Achievements: K", 16);
            OverlayUi.Body(c, "Close any panel: Esc", 16);
        });

        private void ShowCredits() => ShowOverlay("CreditsCanvas", "Credits", c =>
        {
            OverlayUi.Header(c, "ELEMENTBORN");
            OverlayUi.Body(c, "An original elemental-combat RPG.", 18);
            OverlayUi.Body(c, "Design & Development by Steele.", 16);
            OverlayUi.Body(c, "Built with Unity 6 and the Universal Render Pipeline.", 16);
            OverlayUi.Body(c, "Thank you for playing.", 16);
        });

        // A stacked overlay with its own Close button; closing destroys just this overlay's canvas.
        private void ShowOverlay(string canvasName, string title, System.Action<Transform> fill)
        {
            Canvas cv = null;
            var p = OverlayUi.Panel(canvasName, title, 230, new Vector2(640, 720), () => { if (cv != null) Destroy(cv.gameObject); });
            cv = p.canvas;
            fill(p.content);
            AudioController.Instance?.Confirm();
        }
    }
}
