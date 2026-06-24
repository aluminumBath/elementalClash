using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Runs a quick-time "complex move": shows a centered prompt for each <see cref="QteAction"/> in a randomly
    /// generated <see cref="QuickTimeSequence"/>, reads the matching button (keyboard arrows/WASD or gamepad
    /// d-pad/face), ticks the reaction-time window, and fires success/failure callbacks. Self-bootstrapping; start
    /// one with <see cref="Begin"/>.
    ///
    /// NOTE (needs product input): which move/condition begins a QTE (a charged special, a finisher on a staggered
    /// enemy, …) and the VR controller button mapping are design hooks — call <see cref="Begin"/> wherever the
    /// complex move should live. VR has no button map yet (keyboard + gamepad only).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuickTimeController : MonoBehaviour
    {
        private static QuickTimeController _instance;
        public static bool IsActive => _instance != null && _instance._seq != null;

        private QuickTimeSequence _seq;
        private Action _onSuccess;
        private Action _onFail;

        private Canvas _canvas;
        private UiLabel _prompt;
        private UiLabel _info;
        private RectTransform _timer;

        private sealed class UnityRng : IRandomSource
        {
            public double NextUnit() => UnityEngine.Random.value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("QuickTimeController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<QuickTimeController>();
            _instance.Build();
            _instance.ShowPanel(false);
        }

        /// <summary>Begin a quick-time move of <paramref name="length"/> steps; callbacks fire when it finishes.</summary>
        public static void Begin(int length, float windowSeconds, Action onSuccess, Action onFail = null)
        {
            if (_instance == null) Bootstrap();
            _instance.BeginInternal(length, windowSeconds, onSuccess, onFail);
        }

        private void BeginInternal(int length, float windowSeconds, Action onSuccess, Action onFail)
        {
            _seq = QuickTimeSequence.Generate(length, windowSeconds, new UnityRng());
            _onSuccess = onSuccess;
            _onFail = onFail;
            ShowPanel(true);
            Refresh();
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("QuickTimeController", sortOrder: 30);
            DontDestroyOnLoad(_canvas.gameObject);

            var panel = UiTheme.Panel(_canvas.transform, new Color(0.05f, 0.06f, 0.09f, 0.85f));
            var pr = panel.rectTransform;
            pr.anchorMin = pr.anchorMax = pr.pivot = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = new Vector2(0f, -110f);
            pr.sizeDelta = new Vector2(360f, 210f);

            _prompt = UiTheme.Label(panel.transform, "", 96, Color.white, TextAnchor.MiddleCenter);
            var qr = _prompt.Rect; qr.anchorMin = new Vector2(0f, 0.32f); qr.anchorMax = new Vector2(1f, 1f);
            qr.offsetMin = Vector2.zero; qr.offsetMax = Vector2.zero;

            _info = UiTheme.Label(panel.transform, "", 22, UiTheme.TextColor, TextAnchor.MiddleCenter);
            var ir = _info.Rect; ir.anchorMin = new Vector2(0f, 0.16f); ir.anchorMax = new Vector2(1f, 0.32f);
            ir.offsetMin = Vector2.zero; ir.offsetMax = Vector2.zero;

            var track = new GameObject("Timer", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(panel.transform, false);
            var trk = (RectTransform)track.transform;
            trk.anchorMin = new Vector2(0.1f, 0.06f); trk.anchorMax = new Vector2(0.9f, 0.13f);
            trk.offsetMin = Vector2.zero; trk.offsetMax = Vector2.zero;
            track.GetComponent<Image>().color = UiTheme.TrackColor;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, false);
            _timer = (RectTransform)fill.transform;
            _timer.anchorMin = new Vector2(0f, 0f); _timer.anchorMax = new Vector2(1f, 1f);
            _timer.pivot = new Vector2(0f, 0.5f); _timer.offsetMin = Vector2.zero; _timer.offsetMax = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.95f, 0.8f, 0.25f);
        }

        private void ShowPanel(bool on)
        {
            if (_canvas != null && _canvas.gameObject.activeSelf != on) _canvas.gameObject.SetActive(on);
        }

        /// <summary>
        /// Feed a quick-time press from any source — keyboard/gamepad polling here, or a VR move/gesture detector
        /// calling this. Which VR move maps to which <see cref="QteAction"/> is decided when the VR moves are
        /// authored (see VR_INPUT_MAP.md); this is the seam they call into.
        /// </summary>
        public static void SubmitAction(QteAction action)
        {
            if (_instance != null) _instance.Apply(action);
        }

        private void Apply(QteAction action)
        {
            if (_seq == null) return;
            var result = _seq.Press(action);
            if (result == QtePress.Completed) Finish(true);
            else if (result == QtePress.Wrong) Finish(false);
            else if (result == QtePress.Correct) Refresh();
        }

        private void Update()
        {
            if (_seq == null) return;

            QteAction? pressed = ReadAction();
            if (pressed.HasValue) Apply(pressed.Value);
            if (_seq == null) return; // a press may have finished the sequence

            if (_seq.Tick(Time.deltaTime)) { Finish(false); return; } // window expired

            if (_timer != null) _timer.anchorMax = new Vector2(1f - _seq.Progress01, 1f);
        }

        private void Finish(bool success)
        {
            var ok = _onSuccess;
            var fail = _onFail;
            _seq = null; _onSuccess = null; _onFail = null;
            ShowPanel(false);
            if (success) ok?.Invoke(); else fail?.Invoke();
        }

        private void Refresh()
        {
            if (_seq == null) return;
            if (_prompt != null) _prompt.text = Glyph(_seq.Current);
            if (_info != null) _info.text = "Step " + (_seq.Index + 1) + " / " + _seq.Length;
            if (_timer != null) _timer.anchorMax = new Vector2(1f, 1f);
        }

        private static string Glyph(QteAction a)
        {
            switch (a)
            {
                case QteAction.North: return "\u25B2"; // up triangle
                case QteAction.South: return "\u25BC"; // down triangle
                case QteAction.East: return "\u25B6";  // right triangle
                default: return "\u25C0";               // left triangle (West)
            }
        }

        private static QteAction? ReadAction()
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) return QteAction.North;
                if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) return QteAction.South;
                if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) return QteAction.East;
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) return QteAction.West;
            }
            var gp = Gamepad.current;
            if (gp != null)
            {
                if (gp.dpad.up.wasPressedThisFrame || gp.buttonNorth.wasPressedThisFrame) return QteAction.North;
                if (gp.dpad.down.wasPressedThisFrame || gp.buttonSouth.wasPressedThisFrame) return QteAction.South;
                if (gp.dpad.right.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame) return QteAction.East;
                if (gp.dpad.left.wasPressedThisFrame || gp.buttonWest.wasPressedThisFrame) return QteAction.West;
            }
            return null;
        }
    }
}
