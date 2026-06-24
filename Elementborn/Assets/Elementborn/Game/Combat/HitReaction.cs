using System.Collections;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Per-target hit juice: when this object's <see cref="Damageable"/> takes a hit, its visual briefly squashes
    /// (scale pop — always visible) and its renderers flash toward white (best-effort tint via a
    /// MaterialPropertyBlock, so no materials are allocated and the original colours are restored exactly). It
    /// scales the model child rather than the root, leaving the CharacterController collider untouched. Asset-free;
    /// attach alongside a Damageable — <see cref="Combat.EnemyController"/> requires it automatically.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [DisallowMultipleComponent]
    public sealed class HitReaction : MonoBehaviour
    {
        [SerializeField] private float duration = 0.14f;
        [SerializeField] private float squash = 0.18f;
        [SerializeField] private Color flashColor = Color.white;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Damageable _dmg;
        private MaterialPropertyBlock _mpb;
        private Renderer[] _renderers;
        private Color[] _origBase;
        private Color[] _origColor;
        private Transform _visual;
        private Vector3 _visualBase;
        private bool _captured;
        private Coroutine _running;

        private void Awake()
        {
            _dmg = GetComponent<Damageable>();
            _mpb = new MaterialPropertyBlock();
        }

        private void OnEnable() { if (_dmg != null && _dmg.Health != null) _dmg.Health.Damaged += OnDamaged; }
        private void OnDisable() { if (_dmg != null && _dmg.Health != null) _dmg.Health.Damaged -= OnDamaged; }

        // Models are attached at runtime, so capture lazily on the first hit (not in Awake).
        private void Capture()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _origBase = new Color[_renderers.Length];
            _origColor = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mat = _renderers[i] != null ? _renderers[i].sharedMaterial : null;
                _origBase[i] = mat != null && mat.HasProperty(BaseColorId) ? mat.GetColor(BaseColorId) : Color.white;
                _origColor[i] = mat != null && mat.HasProperty(ColorId) ? mat.GetColor(ColorId) : Color.white;
            }
            _visual = transform.childCount > 0 ? transform.GetChild(0) : transform;
            _visualBase = _visual.localScale;
            _captured = true;
        }

        private void OnDamaged(DamageInfo info)
        {
            if (info.Amount < 1f) return; // ignore burn/DoT chip ticks so a burning target doesn't flicker
            if (!_captured || _renderers == null || _renderers.Length == 0) Capture();
            if (!gameObject.activeInHierarchy) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(React());
        }

        private IEnumerator React()
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = HitFeedback.SquashScale(t, duration, squash);
                // squash down, bulge out — springy pop
                _visual.localScale = new Vector3(_visualBase.x * (2f - k), _visualBase.y * k, _visualBase.z * (2f - k));
                SetFlash(1f - Mathf.Clamp01(t / duration));
                yield return null;
            }
            _visual.localScale = _visualBase;
            SetFlash(0f);
            _running = null;
        }

        private void SetFlash(float amount)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(BaseColorId, Color.Lerp(_origBase[i], flashColor, amount));
                _mpb.SetColor(ColorId, Color.Lerp(_origColor[i], flashColor, amount));
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
