using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Ambient decorative fish. Drifts with gentle randomised heading changes (via the pure
    /// <see cref="AmbientWander"/>), stays within a home radius, keeps mostly horizontal, and darts away if the
    /// player gets close. No combat — purely for life and atmosphere underwater. Spawned by
    /// <see cref="FishSpawner"/>.
    /// </summary>
    public sealed class FishWanderer : MonoBehaviour
    {
        [SerializeField] private float speed = 1.2f;
        [SerializeField] private float radius = 8f;
        [SerializeField] private float turnInterval = 1.5f;
        [SerializeField] private float fleeRadius = 3f;

        private Vector3 _home;
        private Vector3 _heading;
        private float _timer;
        private Transform _player;

        public void Init(Vector3 home, float homeRadius)
        {
            _home = home;
            radius = homeRadius;
        }

        private void Start()
        {
            if (_home == Vector3.zero) _home = transform.position;
            _heading = Random.onUnitSphere;
            _heading.y *= 0.4f;
            _heading = _heading.normalized;

            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _timer -= dt;

            Vector3 nudge = Vector3.zero;
            if (_timer <= 0f)
            {
                nudge = Random.onUnitSphere * 0.7f;
                _timer = turnInterval * Random.Range(0.6f, 1.4f);
            }
            if (_player != null)
            {
                Vector3 away = transform.position - _player.position;
                if (away.magnitude < fleeRadius) nudge += away.normalized * 1.5f;
            }

            _heading = AmbientWander.Steer(transform.position, _heading, _home, radius, nudge);
            _heading.y = Mathf.Clamp(_heading.y, -0.5f, 0.5f); // mostly horizontal

            transform.position += _heading * (speed * dt);
            if (_heading.sqrMagnitude > 0.0001f) transform.forward = _heading;
        }
    }
}
