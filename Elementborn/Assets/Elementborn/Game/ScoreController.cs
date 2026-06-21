using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Holds the run's <see cref="ScoreSystem"/> and shows a small HUD (score + combo). Enemies award
    /// score through <see cref="Instance"/> on death. Drop on any object; it builds its own HUD canvas.
    /// </summary>
    public sealed class ScoreController : MonoBehaviour
    {
        public static ScoreController Instance { get; private set; }
        public ScoreSystem Score { get; } = new ScoreSystem();

        private Text _scoreText;
        private Text _comboText;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Score.ScoreChanged += s => { if (_scoreText != null) _scoreText.text = $"Score {s:n0}"; };
            Score.ComboChanged += c => { if (_comboText != null) _comboText.text = c > 1 ? $"Combo x{c}" : ""; };
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            BuildHud();
            _scoreText.text = $"Score {Score.Score:n0}";
            _comboText.text = "";
        }

        private void Update() => Score.Tick(Time.deltaTime);

        public int AddKill(int points) => Score.AddKill(points);
        public void ResetCombo() => Score.ResetCombo();

        private void BuildHud()
        {
            var canvasGo = new GameObject("ScoreHud", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);

            _scoreText = Label(canvasGo.transform, "Score 0", 28, new Vector2(-24, -20));
            _comboText = Label(canvasGo.transform, "", 22, new Vector2(-24, -58));
            _comboText.color = new Color(1f, 0.85f, 0.4f);
        }

        private static Text Label(Transform parent, string content, int size, Vector2 pos)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(360, 40);
            rt.anchoredPosition = pos;

            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.alignment = TextAnchor.UpperRight;
            t.color = Color.white;
            t.text = content;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
