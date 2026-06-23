using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// A small always-on HUD bar (bottom-left) showing your level and a fill toward the next level, so leveling
    /// is visible during play without opening the character screen. Updates live. Put one on a bootstrap object
    /// (the scene adds it).
    /// </summary>
    public sealed class ProgressionHud : MonoBehaviour
    {
        private Canvas _canvas;
        private UiLabel _label;
        private RectTransform _fill;
        private bool _hooked;

        private void Awake() => Build();

        private void Start()
        {
            Hook();
            Refresh();
        }

        private void OnDestroy() => Unhook();

        private void Hook()
        {
            if (_hooked || ProgressionController.Instance == null) return;
            ProgressionController.Instance.Changed += Refresh;
            _hooked = true;
        }

        private void Unhook()
        {
            if (!_hooked || ProgressionController.Instance == null) return;
            ProgressionController.Instance.Changed -= Refresh;
            _hooked = false;
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("ProgressionHud", sortOrder: 9);

            var box = new GameObject("XpBar", typeof(RectTransform));
            box.transform.SetParent(_canvas.transform, false);
            var br = (RectTransform)box.transform;
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(0f, 0f);
            br.sizeDelta = new Vector2(280f, 40f);
            br.anchoredPosition = new Vector2(24f, 24f);

            _label = UiTheme.Label(box.transform, "Lv 1", 22, UiTheme.TextColor, TextAnchor.MiddleLeft);
            var lr = _label.Rect;
            lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(0f, 1f); lr.pivot = new Vector2(0f, 0.5f);
            lr.sizeDelta = new Vector2(70f, 0f); lr.anchoredPosition = Vector2.zero;

            var track = new GameObject("Track", typeof(RectTransform));
            track.transform.SetParent(box.transform, false);
            var tr = (RectTransform)track.transform;
            tr.anchorMin = new Vector2(0f, 0.25f); tr.anchorMax = new Vector2(1f, 0.75f);
            tr.offsetMin = new Vector2(78f, 0f); tr.offsetMax = new Vector2(0f, 0f);
            track.AddComponent<Image>().color = UiTheme.TrackColor;

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(track.transform, false);
            _fill = (RectTransform)fill.transform;
            _fill.anchorMin = new Vector2(0f, 0f);
            _fill.anchorMax = new Vector2(0f, 1f);   // x set in Refresh
            _fill.offsetMin = Vector2.zero; _fill.offsetMax = Vector2.zero;
            fill.AddComponent<Image>().color = UiTheme.ButtonColor;
        }

        private void Refresh()
        {
            if (!_hooked) Hook();
            var pc = ProgressionController.Instance;
            if (pc == null || _label == null || _fill == null) return;
            var p = pc.Progression;
            _label.text = "Lv " + p.Level;
            float ratio = p.XpToNext > 0 ? Mathf.Clamp01((float)p.Xp / p.XpToNext) : 0f;
            _fill.anchorMax = new Vector2(ratio, 1f);
            _fill.offsetMax = Vector2.zero;
        }
    }
}
