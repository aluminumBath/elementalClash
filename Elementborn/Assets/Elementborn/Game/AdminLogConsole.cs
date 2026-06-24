using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A lightweight in-game log console for admins/QA: it captures everything sent to Unity's log
    /// (<see cref="Application.logMessageReceived"/> — every Debug.Log/Warning/Error across the game) into a pure
    /// <see cref="LogRing"/> and renders the most recent lines in a screen-space panel, colour-coded by severity.
    ///
    /// Self-bootstrapping; capture is always on (cheap), the panel is hidden until toggled. Toggle with
    /// <c>F2</c> on a keyboard, or call <see cref="Toggle"/> / <see cref="SetVisible"/> from any admin menu. Gated
    /// by <see cref="AdminUnlocked"/> (defaults on in the editor and development builds); flip it from your
    /// admin/role check to allow it in production.
    ///
    /// NOTE (needs product input): there is no headset-friendly toggle yet (Quest has no F2) and gameplay
    /// telemetry (the GameEvents bus) is not fed in — both are easy follow-ups, wired through <see cref="Push"/>
    /// and <see cref="Toggle"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdminLogConsole : MonoBehaviour
    {
        public static bool AdminUnlocked = Application.isEditor || Debug.isDebugBuild;

        private const int Capacity = 250;
        private const int VisibleLines = 26;

        private static AdminLogConsole _instance;
        private readonly LogRing _ring = new LogRing(Capacity);
        private Canvas _canvas;
        private UiLabel _text;
        private bool _visible;
        private bool _dirty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("AdminLogConsole");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AdminLogConsole>();
        }

        /// <summary>Add a line directly (e.g., to surface gameplay telemetry alongside Unity logs).</summary>
        public static void Push(string line, LogSeverity severity = LogSeverity.Info)
        {
            if (_instance == null) Bootstrap();
            _instance._ring.Add(line, severity);
            _instance._dirty = true;
        }

        public static void Toggle() { if (_instance != null) _instance.SetVisibleInternal(!_instance._visible); }
        public static void SetVisible(bool on) { if (_instance != null) _instance.SetVisibleInternal(on); }

        private void Awake() { Build(); }

        private void OnEnable() { Application.logMessageReceived += OnLog; }
        private void OnDisable() { Application.logMessageReceived -= OnLog; }

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            LogSeverity sev = type == LogType.Warning ? LogSeverity.Warning
                            : (type == LogType.Log ? LogSeverity.Info : LogSeverity.Error);
            _ring.Add(condition, sev);
            _dirty = true;
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("AdminLogConsole", sortOrder: 5000); // above gameplay HUD
            DontDestroyOnLoad(_canvas.gameObject);

            var panel = UiTheme.Panel(_canvas.transform, new Color(0f, 0f, 0f, 0.78f));
            var pr = panel.rectTransform;
            pr.anchorMin = new Vector2(0f, 0f); pr.anchorMax = new Vector2(0.5f, 1f);
            pr.offsetMin = new Vector2(8f, 8f); pr.offsetMax = new Vector2(-8f, -8f);

            _text = UiTheme.Label(panel.transform, "", 16, UiTheme.TextColor, TextAnchor.LowerLeft);
            var tr = _text.Rect;
            tr.anchorMin = new Vector2(0f, 0f); tr.anchorMax = new Vector2(1f, 1f);
            tr.offsetMin = new Vector2(12f, 10f); tr.offsetMax = new Vector2(-12f, -10f);

            _canvas.gameObject.SetActive(false);
        }

        private void SetVisibleInternal(bool on)
        {
            if (!AdminUnlocked) on = false;
            _visible = on;
            if (_canvas != null) _canvas.gameObject.SetActive(on);
            if (on) _dirty = true;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (AdminUnlocked && kb != null && kb.f2Key.wasPressedThisFrame) SetVisibleInternal(!_visible);

            if (_visible && _dirty) { Rebuild(); _dirty = false; }
        }

        private void Rebuild()
        {
            if (_text == null) return;
            int count = _ring.Count;
            int from = count > VisibleLines ? count - VisibleLines : 0;
            var sb = new StringBuilder(VisibleLines * 48);
            for (int i = from; i < count; i++)
            {
                sb.Append(HexFor(_ring.GetSeverity(i)));
                sb.Append(_ring.GetLine(i));
                sb.Append("</color>\n");
            }
            _text.text = sb.ToString();
        }

        private static string HexFor(LogSeverity sev)
        {
            switch (sev)
            {
                case LogSeverity.Warning: return "<color=#F2C84B>";
                case LogSeverity.Error: return "<color=#E5534B>";
                default: return "<color=#C8C8C8>";
            }
        }
    }
}
