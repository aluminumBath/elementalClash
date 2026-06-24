using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A floating health bar over an enemy. Asset-free: two <see cref="ToonPalette"/>-tinted quads (dark track +
    /// coloured fill) that billboard to the camera. It appears the moment the enemy takes damage and retracts a
    /// couple of seconds after the last hit (curves from the unit-tested <see cref="HealthBarState"/>); the fill
    /// drains from the left and shifts green → yellow → red. The bar is a <em>detached</em> object (not a child),
    /// so it never gets caught by <see cref="HitReaction"/>'s model squash, and it is destroyed with the enemy.
    /// Required automatically on every <see cref="Combat.EnemyController"/>.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [DisallowMultipleComponent]
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        private const float Width = 0.9f;
        private const float Height = 0.12f;
        private const float HeadHeight = 2.2f;
        private const float Hold = 2.5f;
        private const float Fade = 0.5f;

        private static readonly Color BgColor = new Color(0.08f, 0.08f, 0.10f);
        private static readonly Color Green = new Color(0.30f, 0.85f, 0.30f);
        private static readonly Color Yellow = new Color(0.95f, 0.82f, 0.20f);
        private static readonly Color Red = new Color(0.90f, 0.25f, 0.20f);
        private static readonly Color PoiseTrackColor = new Color(0.06f, 0.07f, 0.10f);
        private static readonly Color PoiseColor = new Color(0.60f, 0.85f, 1.00f); // building toward a break
        private static readonly Color StaggerColor = new Color(1.00f, 0.80f, 0.25f); // broken / stunned

        private Damageable _dmg;
        private Transform _barRoot;
        private Transform _fill;
        private MeshRenderer _fillRenderer;
        private Camera _cam;
        private float _lastFrac = 1f;
        private float _sinceDamage = 999f;
        private int _tier = -1;

        private StaggerController _stagger;
        private Transform _poiseFill;
        private MeshRenderer _poiseFillRenderer;
        private int _poiseTier = -1;

        private Transform _pip; // element-affinity tell, built lazily once the affinity is known

        private void Awake()
        {
            _dmg = GetComponent<Damageable>();
            _stagger = GetComponent<StaggerController>();
            BuildBar();
            SetActive(false);
        }

        private void BuildBar()
        {
            _barRoot = new GameObject("EnemyHealthBar").transform; // detached on purpose

            var bg = MakeQuad(BgColor);
            bg.SetParent(_barRoot, false);
            bg.localScale = new Vector3(Width, Height, 1f);
            bg.localPosition = Vector3.zero;

            _fill = MakeQuad(Green);
            _fill.SetParent(_barRoot, false);
            _fill.localScale = new Vector3(Width, Height * 0.78f, 1f);
            _fill.localPosition = new Vector3(0f, 0f, -0.02f); // sit in front of the track
            _fillRenderer = _fill.GetComponent<MeshRenderer>();

            if (_stagger != null)
            {
                var poiseTrack = MakeQuad(PoiseTrackColor);
                poiseTrack.SetParent(_barRoot, false);
                poiseTrack.localScale = new Vector3(Width * 0.92f, Height * 0.42f, 1f);
                poiseTrack.localPosition = new Vector3(0f, -Height * 0.95f, 0f);

                _poiseFill = MakeQuad(PoiseColor);
                _poiseFill.SetParent(_barRoot, false);
                _poiseFill.localScale = new Vector3(Width * 0.92f, Height * 0.32f, 1f);
                _poiseFill.localPosition = new Vector3(0f, -Height * 0.95f, -0.02f);
                _poiseFillRenderer = _poiseFill.GetComponent<MeshRenderer>();
            }
        }

        private static Transform MakeQuad(Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.GetComponent<MeshRenderer>().sharedMaterial = ToonPalette.Tinted(color);
            return go.transform;
        }

        private void SetActive(bool on)
        {
            if (_barRoot != null && _barRoot.gameObject.activeSelf != on) _barRoot.gameObject.SetActive(on);
        }

        private void LateUpdate()
        {
            if (_dmg == null || _dmg.Health == null) { SetActive(false); return; }

            float frac = HealthBarState.Fraction(_dmg.Health.Current, _dmg.Health.Max);
            if (frac < _lastFrac - 1e-4f) _sinceDamage = 0f; else _sinceDamage += Time.deltaTime;
            _lastFrac = frac;

            float alpha = HealthBarState.Alpha(_sinceDamage, Hold, Fade);
            bool show = alpha > 0.02f && frac > 0f;
            SetActive(show);
            if (!show) return;

            if (_cam == null) _cam = Camera.main;
            _barRoot.position = transform.position + Vector3.up * HeadHeight;
            if (_cam != null) _barRoot.rotation = _cam.transform.rotation;
            _barRoot.localScale = Vector3.one * alpha; // retract as it fades (opaque-safe)

            if (_pip == null && _dmg.Affinity.HasValue) // build the element pip once we know the affinity
            {
                _pip = MakeQuad(ElementColor.For(_dmg.Affinity.Value));
                _pip.SetParent(_barRoot, false);
                _pip.localScale = new Vector3(Height * 1.3f, Height * 1.3f, 1f);
                _pip.localPosition = new Vector3(-(Width * 0.5f) - Height * 0.9f, 0f, -0.02f); // just left of the track
            }

            _fill.localScale = new Vector3(Width * frac, Height * 0.78f, 1f);
            _fill.localPosition = new Vector3(-(Width * 0.5f) * (1f - frac), 0f, -0.02f); // drain from the left

            int tier = frac > 0.5f ? 0 : (frac > 0.25f ? 1 : 2);
            if (tier != _tier)
            {
                _tier = tier;
                _fillRenderer.sharedMaterial = ToonPalette.Tinted(tier == 0 ? Green : (tier == 1 ? Yellow : Red));
            }

            if (_stagger != null && _poiseFill != null)
            {
                bool staggered = _stagger.IsStaggered;
                float pf = staggered ? 1f : _stagger.PoiseFraction; // show full while broken/stunned
                float pw = Width * 0.92f;
                _poiseFill.localScale = new Vector3(pw * pf, Height * 0.32f, 1f);
                _poiseFill.localPosition = new Vector3(-(pw * 0.5f) * (1f - pf), -Height * 0.95f, -0.02f);

                int ptier = staggered ? 1 : 0;
                if (ptier != _poiseTier)
                {
                    _poiseTier = ptier;
                    _poiseFillRenderer.sharedMaterial = ToonPalette.Tinted(staggered ? StaggerColor : PoiseColor);
                }
            }
        }

        private void OnDestroy()
        {
            if (_barRoot != null) Destroy(_barRoot.gameObject);
        }
    }
}
