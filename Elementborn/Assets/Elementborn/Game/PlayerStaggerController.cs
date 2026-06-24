using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The player's poise/stagger. Incoming hits build a pure <see cref="Poise"/> meter shown on a slim HUD bar;
    /// when it breaks the player is briefly stunned — which <see cref="PlayerCombatController"/> already honours, so
    /// the stagger interrupts your offense — and the screen flashes red with a "STAGGERED!" warning. Reuses the same
    /// Core meter as enemies but never registers as a finisher target. Self-bootstrapping; finds the player by tag.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerStaggerController : MonoBehaviour
    {
        private const float PoiseMax = 80f;
        private const float RegenPerSecond = 16f;
        private const float RegenDelay = 2.5f;
        private const float StaggerSeconds = 1.2f;
        private const float FlashSeconds = 0.4f;

        private static readonly Color TrackColor = new Color(0.10f, 0.10f, 0.13f, 0.70f);
        private static readonly Color PoiseColor = new Color(0.95f, 0.65f, 0.20f);   // amber: danger building
        private static readonly Color StaggerColor = new Color(0.95f, 0.25f, 0.20f); // red: broken

        private static PlayerStaggerController _instance;

        private Damageable _body;
        private Poise _poise;
        private float _staggerTimer;
        private float _flashTimer;

        private Canvas _canvas;
        private RectTransform _barRoot;
        private RectTransform _fill;
        private Image _fillImage;
        private Image _flash;
        private UiLabel _label;
        private int _tier = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("PlayerStaggerController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayerStaggerController>();
            _instance._poise = new Poise(PoiseMax, RegenPerSecond, RegenDelay);
            _instance.Build();
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("PlayerPoise", sortOrder: 9);
            DontDestroyOnLoad(_canvas.gameObject);

            var vr = _canvas.gameObject.AddComponent<VrHudAnchor>();
            vr.viewOffset = new Vector3(0f, -0.40f, 1.5f); vr.worldSize = new Vector2(560f, 360f);

            _flash = UiTheme.Panel(_canvas.transform, new Color(0.85f, 0.10f, 0.10f, 0f), "overlay_dim");
            var fr = _flash.rectTransform;
            fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;

            var track = UiTheme.Panel(_canvas.transform, TrackColor);
            _barRoot = track.rectTransform;
            _barRoot.anchorMin = _barRoot.anchorMax = _barRoot.pivot = new Vector2(0.5f, 0f);
            _barRoot.anchoredPosition = new Vector2(0f, 44f);
            _barRoot.sizeDelta = new Vector2(360f, 14f);

            _fillImage = UiTheme.Panel(track.transform, PoiseColor);
            _fill = _fillImage.rectTransform;
            _fill.anchorMin = new Vector2(0f, 0f);
            _fill.anchorMax = new Vector2(0f, 1f); // width driven by anchorMax.x each frame
            _fill.pivot = new Vector2(0f, 0.5f);
            _fill.offsetMin = Vector2.zero; _fill.offsetMax = Vector2.zero;

            _label = UiTheme.Label(_canvas.transform, "STAGGERED!", 34, StaggerColor, TextAnchor.MiddleCenter);
            var lr = _label.Rect;
            lr.anchorMin = lr.anchorMax = lr.pivot = new Vector2(0.5f, 0.5f);
            lr.anchoredPosition = new Vector2(0f, 120f);
            lr.sizeDelta = new Vector2(500f, 60f);

            _barRoot.gameObject.SetActive(false);
            _label.Rect.gameObject.SetActive(false);
        }

        private bool Acquire()
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged == null) return false;
            _body = tagged.GetComponentInParent<Damageable>();
            if (_body == null || _body.Health == null) { _body = null; return false; }
            _body.Health.Damaged += OnDamaged;
            return true;
        }

        private void OnDamaged(DamageInfo info)
        {
            if (info.Amount < 1f) return;     // ignore chip/DoT
            if (_staggerTimer > 0f) return;   // already broken; ride out the window
            if (_poise.AddHit(info.Amount)) Break();
        }

        private void Break()
        {
            _staggerTimer = StaggerSeconds;
            _flashTimer = FlashSeconds;
            _body.ApplyStatus(new StatusEffect(StatusKind.Stun, 0f, StaggerSeconds));
        }

        private void Update()
        {
            if (_body == null) { if (!Acquire()) return; }

            float dt = Time.deltaTime;
            bool staggered = _staggerTimer > 0f;
            if (staggered) { _staggerTimer -= dt; if (_staggerTimer <= 0f) staggered = false; }
            else _poise.Tick(dt);

            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                SetFlashAlpha(Mathf.Clamp01(_flashTimer / FlashSeconds) * 0.5f);
            }
            else SetFlashAlpha(0f);

            float pf = staggered ? 1f : _poise.Fraction;
            bool show = pf > 0.02f || staggered;
            if (_barRoot.gameObject.activeSelf != show) _barRoot.gameObject.SetActive(show);
            if (_label.Rect.gameObject.activeSelf != staggered) _label.Rect.gameObject.SetActive(staggered);
            if (!show) { _tier = -1; return; }

            _fill.anchorMax = new Vector2(Mathf.Clamp01(pf), 1f);

            int tier = staggered ? 1 : 0;
            if (tier != _tier)
            {
                _tier = tier;
                _fillImage.color = staggered ? StaggerColor : PoiseColor;
            }
        }

        private void SetFlashAlpha(float a)
        {
            if (_flash == null) return;
            var c = _flash.color;
            if (Mathf.Abs(c.a - a) < 1e-3f) return;
            c.a = a; _flash.color = c;
        }

        private void OnDestroy()
        {
            if (_body != null && _body.Health != null) _body.Health.Damaged -= OnDamaged;
        }
    }
}
