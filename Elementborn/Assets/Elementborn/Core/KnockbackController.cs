using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Holds a decaying knockback velocity so any controller-driven object (enemy, player)
    /// can be shoved without a physics Rigidbody. Add an impulse on hit; read
    /// <see cref="Velocity"/> each frame and fold it into movement. Pure logic, so tested directly.
    /// </summary>
    public sealed class KnockbackController
    {
        private Vector3 _velocity;

        public Vector3 Velocity => _velocity;
        public bool IsActive => _velocity.sqrMagnitude > 0.01f;
        /// <summary>Higher = knockback fades faster. 0-1 fraction removed per second, roughly.</summary>
        public float Damping { get; set; } = 6f;

        public void Add(Vector3 impulse) => _velocity += impulse;

        public void Tick(float deltaTime)
        {
            if (_velocity == Vector3.zero) return;
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, Mathf.Clamp01(Damping * deltaTime));
            if (_velocity.sqrMagnitude < 0.01f) _velocity = Vector3.zero;
        }
    }
}
