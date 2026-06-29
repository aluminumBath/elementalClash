using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Applies a sensible frame-rate target for the platform at boot: Quest standalone (Android) is pinned
    /// to its 72 Hz refresh with vSync off so the target governs, while PCVR and flat PC are left to their own
    /// compositor/vSync. Exposes the FPS budget the overlay grades against. Real per-device tuning (render scale,
    /// fixed foveation, quality tiers) is profiled on hardware on top of this baseline.</summary>
    public sealed class PerformanceController : MonoBehaviour
    {
        public static PerformanceController Instance { get; private set; }

        public int TargetFps { get; private set; }
        public int BudgetFps { get; private set; } = 60;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Apply();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Apply()
        {
            bool questStandalone = Application.platform == RuntimePlatform.Android;
            QualitySettings.vSyncCount = 0;               // let targetFrameRate govern
            TargetFps = questStandalone ? 72 : -1;        // -1 = platform default elsewhere
            BudgetFps = questStandalone ? 72 : 60;        // the "smooth" line the overlay grades against
            Application.targetFrameRate = TargetFps;
        }
    }

    /// <summary>A dev frame-time overlay (toggle with F3) for profiling: live FPS plus average and peak frame time
    /// over a rolling window, colour-graded green/amber/red against the platform's FPS budget. The window math is the
    /// pure, unit-tested <see cref="FrameStats"/>; this just feeds it <c>unscaledDeltaTime</c> and renders a
    /// non-interactive screen-space readout (no input capture, built lazily so it costs nothing while hidden).</summary>
    public sealed class PerformanceHud : MonoBehaviour
    {
        private static readonly Color Good = new Color(0.45f, 1f, 0.5f);
        private static readonly Color Warn = new Color(1f, 0.82f, 0.25f);
        private static readonly Color Bad = new Color(1f, 0.42f, 0.42f);

        private readonly FrameStats _stats = new FrameStats(120);
        private bool _visible;
        private GameObject _canvasGo;
        private TextMeshProUGUI _text;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f3Key.wasPressedThisFrame) Toggle();

            _stats.Push(Time.unscaledDeltaTime * 1000.0);
            if (_visible && _text != null) UpdateText();
        }

        private void Toggle()
        {
            _visible = !_visible;
            if (_visible) Ensure();
            if (_canvasGo != null) _canvasGo.SetActive(_visible);
        }

        private void Ensure()
        {
            if (_canvasGo != null) return;
            _canvasGo = new GameObject("PerfHudCanvas");
            var canvas = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            var font = Elementborn.Game.ElementbornTmpFontUtility.GetDefaultFontAsset();
            if (font == null) return; // asset-free guard: no font, no readout (canvas stays empty)

            var t = new GameObject("PerfText");
            t.transform.SetParent(_canvasGo.transform, false);
            _text = t.AddComponent<TextMeshProUGUI>();
            _text.font = font;
            _text.fontSize = 26f;
            _text.raycastTarget = false; // never intercept input
            _text.alignment = TextAlignmentOptions.TopRight;
            var rt = _text.rectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-16f, -12f);
            rt.sizeDelta = new Vector2(460f, 80f);
        }

        private void UpdateText()
        {
            double fps = _stats.Fps;
            int budget = PerformanceController.Instance != null ? PerformanceController.Instance.BudgetFps : 60;
            _text.color = fps >= budget - 2 ? Good : (fps >= budget * 0.8 ? Warn : Bad);
            _text.text = string.Format("{0:0} FPS    {1:0.0} ms avg    {2:0.0} ms peak", fps, _stats.AverageMs, _stats.MaxMs);
        }
    }
}
