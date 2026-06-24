using UnityEngine;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// One piece of a defeat shatter: flies along its launch velocity under gravity, tumbles, and shrinks to
    /// nothing (an asset-free "dissolve"), then destroys itself. Spawned by <see cref="DeathPoof"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Shard : MonoBehaviour
    {
        private Vector3 _vel;
        private Vector3 _spin;
        private Vector3 _baseScale;
        private float _life;
        private float _t;

        public void Launch(Vector3 velocity, float life)
        {
            _vel = velocity;
            _life = Mathf.Max(0.05f, life);
            _t = 0f;
            _baseScale = transform.localScale;
            _spin = new Vector3(Random.value, Random.value, Random.value) * 540f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _t += dt;
            _vel += Vector3.up * (-9.81f * dt);
            transform.position += _vel * dt;
            transform.Rotate(_spin * dt, Space.Self);

            float k = 1f - Mathf.Clamp01(_t / _life);
            transform.localScale = _baseScale * k;

            if (_t >= _life) Destroy(gameObject);
        }
    }
}
