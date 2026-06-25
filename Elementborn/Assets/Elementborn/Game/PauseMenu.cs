using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Elementborn.Game
{
    /// <summary>In-world pause. Pressing Escape freezes the game (timeScale 0) and shows Resume / Settings / Main Menu
    /// / Quit. It only claims Escape when nothing else is open (<see cref="UiGate"/>) and only while you're actually in
    /// the world, so it never fights the other menus' own Escape-to-close. Resuming restores time and re-locks the
    /// cursor for play; Main Menu restores time and reboots the scene back to the title.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }
        public bool IsPaused { get; private set; }

        private GameObject _panel;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb[Key.Escape].wasPressedThisFrame) return;
            if (IsPaused) Resume();
            else if (!UiGate.IsBlocking && RigTeleporter.Rig != null) Pause();
        }

        private void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var panel = OverlayUi.Panel("PauseCanvas", "PAUSED", 240, new Vector2(520, 520), Resume);
            _panel = panel.canvas.gameObject;
            var c = panel.content;
            UiTheme.Button(c, "Resume", Resume, 360, 54);
            UiTheme.Button(c, "Settings", () => SettingsController.EnsureInstance().Open(), 360, 50);
            UiTheme.Button(c, "Main Menu", ToMenu, 360, 50);
            UiTheme.Button(c, "Quit", Quit, 360, 50);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (_panel != null) Destroy(_panel);
            _panel = null;
        }

        private void ToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Quit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
