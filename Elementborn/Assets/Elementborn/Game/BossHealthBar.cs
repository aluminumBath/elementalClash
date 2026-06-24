using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A screen-anchored boss health bar across the top of the screen: the boss's name over a wide bar that drains
    /// as it takes damage (fraction from the unit-tested <see cref="HealthBarState"/>). Self-bootstrapping and
    /// reusable — any boss calls <see cref="Engage"/> when its fight begins; the bar tracks that boss each frame and
    /// hides itself when the boss dies, is destroyed, or another boss engages. Built from the shared
    /// <see cref="UiTheme"/> widgets, so it inherits the project's UI look and TMP/legacy fallback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossHealthBar : MonoBehaviour
    {
        private static BossHealthBar _instance;

        private Canvas _canvas;
        private RectTransform _fill;
        private UiLabel _name;
        private Damageable _boss;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("BossHealthBarService");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<BossHealthBar>();
            _instance.Build();
            _instance.Show(false);
        }

        /// <summary>Show the bar for a boss and start tracking its health. Pass a display name.</summary>
        public static void Engage(Damageable boss, string bossName)
        {
            if (_instance == null) Bootstrap();
            _instance._boss = boss;
            if (_instance._name != null) _instance._name.text = string.IsNullOrEmpty(bossName) ? "Boss" : bossName;
            _instance.Show(boss != null && boss.Health != null && !boss.Health.IsDead);
        }

        /// <summary>Hide the bar and stop tracking (e.g., the player left the arena).</summary>
        public static void Disengage()
        {
            if (_instance == null) return;
            _instance._boss = null;
            _instance.Show(false);
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("BossHealthBar", sortOrder: 8);
            DontDestroyOnLoad(_canvas.gameObject); // persist with the service so it never dangles across scenes

            var container = new GameObject("BossBar", typeof(RectTransform));
            container.transform.SetParent(_canvas.transform, false);
            var cr = (RectTransform)container.transform;
            cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 1f);
            cr.anchoredPosition = new Vector2(0f, -24f);
            cr.sizeDelta = new Vector2(820f, 70f);

            _name = UiTheme.Label(container.transform, "Boss", 28, UiTheme.TextColor, TextAnchor.MiddleCenter);
            var nr = _name.Rect;
            nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(1f, 1f); nr.pivot = new Vector2(0.5f, 1f);
            nr.sizeDelta = new Vector2(0f, 32f); nr.anchoredPosition = Vector2.zero;

            var track = new GameObject("Track", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(container.transform, false);
            var tr = (RectTransform)track.transform;
            tr.anchorMin = new Vector2(0f, 0f); tr.anchorMax = new Vector2(1f, 0f); tr.pivot = new Vector2(0.5f, 0f);
            tr.sizeDelta = new Vector2(0f, 28f); tr.anchoredPosition = new Vector2(0f, 2f);
            track.GetComponent<Image>().color = UiTheme.TrackColor;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(track.transform, false);
            _fill = (RectTransform)fillGo.transform;
            _fill.anchorMin = new Vector2(0f, 0f); _fill.anchorMax = new Vector2(1f, 1f); _fill.pivot = new Vector2(0f, 0.5f);
            _fill.offsetMin = Vector2.zero; _fill.offsetMax = Vector2.zero;
            fillGo.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.22f); // boss red
        }

        private void Show(bool on)
        {
            if (_canvas != null && _canvas.gameObject.activeSelf != on) _canvas.gameObject.SetActive(on);
        }

        private void Update()
        {
            if (_boss == null || _boss.Health == null) { Show(false); return; }
            if (_boss.Health.IsDead) { _boss = null; Show(false); return; }
            if (_fill == null) return;

            float frac = HealthBarState.Fraction(_boss.Health.Current, _boss.Health.Max);
            _fill.anchorMax = new Vector2(frac, 1f);
        }
    }
}
