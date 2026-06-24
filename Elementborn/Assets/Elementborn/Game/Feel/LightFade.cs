using UnityEngine;

namespace Elementborn.Game.Feel
{
    /// <summary>Fades a <see cref="Light"/>'s intensity to zero over its lifetime, then destroys the GameObject.
    /// Attached by <see cref="TransientLight"/>.</summary>
    [DisallowMultipleComponent]
    public sealed class LightFade : MonoBehaviour
    {
        private Light _light;
        private float _start, _life, _t;

        public void Begin(Light light, float life)
        {
            _light = light;
            _start = light != null ? light.intensity : 0f;
            _life = Mathf.Max(0.01f, life);
            _t = 0f;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(_t / _life);
            if (_light != null) _light.intensity = _start * k;
            if (_t >= _life) Destroy(gameObject);
        }
    }
}
