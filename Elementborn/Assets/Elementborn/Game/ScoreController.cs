using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Holds the run's <see cref="ScoreSystem"/> and shows a small HUD (score + combo). Enemies award
    /// score through <see cref="Instance"/> on death. Drop on any object; it builds its own HUD canvas
    /// via <see cref="UiTheme"/>.
    /// </summary>
    public sealed class ScoreController : MonoBehaviour
    {
        public static ScoreController Instance { get; private set; }
        public ScoreSystem Score { get; } = new ScoreSystem();

        private UiLabel _scoreText;
        private UiLabel _comboText;

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

        /// <summary>A clean dodge of a telegraphed attack: small points, and it keeps the combo alive.</summary>
        public const int DodgePoints = 30;
        public int RegisterDodge() => Score.AddKill(DodgePoints);

        private void BuildHud()
        {
            var canvas = UiTheme.Canvas("ScoreHud", sortOrder: 10);
            canvas.transform.SetParent(transform, false);

            _scoreText = Place(UiTheme.Label(canvas.transform, "Score 0", 28, Color.white, TextAnchor.UpperRight),
                new Vector2(-24, -20));
            _comboText = Place(UiTheme.Label(canvas.transform, "", 22, new Color(1f, 0.85f, 0.4f), TextAnchor.UpperRight),
                new Vector2(-24, -58));
        }

        private static UiLabel Place(UiLabel lbl, Vector2 pos)
        {
            var rt = lbl.Rect;
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(360, 40);
            rt.anchoredPosition = pos;
            return lbl;
        }
    }
}
