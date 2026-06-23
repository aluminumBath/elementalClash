using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A toggled quest-log overlay (default key L) listing the quests you're tracking with per-objective progress
    /// and a "ready to turn in" marker. Refreshes live as objectives advance. Put one on a bootstrap object (the
    /// bootstrap scene adds it).
    /// </summary>
    public sealed class QuestLogController : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.L;

        private Canvas _canvas;
        private Transform _content;
        private bool _open;

        private void Awake()
        {
            var p = OverlayUi.Panel("QuestLogCanvas", "Quests", 55, new Vector2(820, 620), Hide);
            _canvas = p.canvas; _content = p.content;
            Hide();
        }

        private void Start()
        {
            if (QuestController.Instance != null) QuestController.Instance.Changed += Refresh;
        }

        private void OnDestroy()
        {
            if (QuestController.Instance != null) QuestController.Instance.Changed -= Refresh;
        }

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
            if (_canvas != null) _canvas.gameObject.SetActive(true);
            Rebuild();
        }

        private void Hide()
        {
            _open = false;
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        private void Refresh() { if (_open) Rebuild(); }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            var log = QuestController.Instance != null ? QuestController.Instance.Log : null;
            var tracked = log != null ? log.Tracked() : null;
            if (tracked == null || tracked.Count == 0)
            {
                OverlayUi.Body(_content, "No active quests. Talk to Willow, Kiana, or Parfa to pick one up.", 20);
                return;
            }

            foreach (var s in tracked)
            {
                bool ready = s.Status == QuestStatus.ReadyToTurnIn;
                OverlayUi.Header(_content, s.Definition.Title + (ready ? "    (ready to turn in)" : ""), 22);
                for (int i = 0; i < s.Definition.Objectives.Count; i++)
                {
                    var o = s.Definition.Objectives[i];
                    bool met = s.Progress[i] >= o.Required;
                    OverlayUi.Body(_content,
                        (met ? "[x] " : "[ ] ") + o.Description + "   (" + s.Progress[i] + "/" + o.Required + ")",
                        17, met ? new Color(0.52f, 0.80f, 0.52f, 1f) : new Color(0.78f, 0.80f, 0.86f, 1f));
                }
            }
        }
    }
}
