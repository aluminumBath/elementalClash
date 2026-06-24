using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The achievements panel (default key <b>K</b>; also opened from the VR hub): a checklist of every
    /// achievement with its progress and unlocked state, read from <see cref="AchievementController"/>. Built via
    /// <see cref="OverlayUi"/>, so it's world-space in VR. The bootstrap scene adds one.</summary>
    public sealed class AchievementsViewer : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.K;

        private static readonly Color Done = new Color(0.55f, 0.95f, 0.55f, 1f);
        private static readonly Color Pending = new Color(0.80f, 0.82f, 0.86f, 1f);

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
            var p = OverlayUi.Panel("AchievementsCanvas", "Achievements", 56, new Vector2(700, 760), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var prog = AchievementController.Instance != null ? AchievementController.Instance.Progress : null;
            int unlocked = prog != null ? prog.UnlockedCount : 0;
            OverlayUi.Header(_content, unlocked + " / " + AchievementCatalog.All.Count + " unlocked");

            foreach (var a in AchievementCatalog.All)
            {
                bool done = prog != null && prog.IsUnlocked(a);
                int cur = prog != null ? prog.Progress(a) : 0;
                string line = (done ? "[x] " : "[ ] ") + a.Name + " — " + a.Description + "  (" + cur + "/" + a.Target + ")";
                OverlayUi.Body(_content, line, 18, done ? Done : Pending);
            }
        }
    }
}
