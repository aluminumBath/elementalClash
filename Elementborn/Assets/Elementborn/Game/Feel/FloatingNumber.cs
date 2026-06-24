using UnityEngine;
using TMPro;
using Elementborn.Core;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// A single floating damage number. Billboards toward the camera, climbs and fades, and self-destroys at the
    /// end of its life (matching the project's <see cref="TransientLight"/> spawn-and-destroy style). All motion
    /// comes from the unit-tested <see cref="DamagePopup"/>; this component just drives a TextMeshPro. Spawned and
    /// configured by <see cref="DamageNumbers"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FloatingNumber : MonoBehaviour
    {
        private const float Lifetime = 0.9f;
        private const float RiseDistance = 1.1f;
        private const float HeadOffset = 1.2f; // start above the hit point, not at its feet

        private TMP_Text _tmp;
        private Camera _cam;
        private Vector3 _origin;
        private Color _color;
        private float _baseScale;
        private float _t;

        /// <summary>Begin the popup. The TMP text/colour/size are already set by the spawner.</summary>
        public void Play(Vector3 worldPos, float baseScale)
        {
            _tmp = GetComponent<TMP_Text>();
            _color = _tmp != null ? _tmp.color : Color.white;
            _origin = worldPos + Vector3.up * HeadOffset;
            _baseScale = baseScale;
            _cam = Camera.main;
            _t = 0f;
            transform.position = _origin;
            transform.localScale = Vector3.one * (_baseScale * 0.6f);
        }

        private void LateUpdate()
        {
            _t += Time.deltaTime;
            DamagePopup.Evaluate(_t, Lifetime, out float rise, out float alpha, out float scale);

            transform.position = _origin + Vector3.up * (rise * RiseDistance);
            transform.localScale = Vector3.one * (_baseScale * scale);

            if (_tmp != null)
            {
                Color c = _color;
                c.a = alpha;
                _tmp.color = c;
            }

            if (_cam == null) _cam = Camera.main;
            if (_cam != null) transform.rotation = _cam.transform.rotation; // face the viewer, upright

            if (_t >= Lifetime) Destroy(gameObject);
        }
    }
}
