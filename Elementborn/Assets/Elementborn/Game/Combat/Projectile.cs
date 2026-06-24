using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A flying ability projectile. Moves along its direction and, on first contact with an
    /// <see cref="IDamageable"/>, applies damage, any status effect, and knockback, then despawns.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Projectile : MonoBehaviour
    {
        private Vector3 _direction = Vector3.forward;
        private float _speed;
        private DamageInfo _damage;
        private StatusEffect _status;
        private float _knockback;
        private float _lifetime;
        private GameObject _impactEffect;
        private GameObject _owner;

        public void Initialize(AbilityOutcome outcome, float lifetime, GameObject impactEffect = null, GameObject owner = null)
        {
            _direction = outcome.Direction.sqrMagnitude > 0.0001f ? outcome.Direction.normalized : Vector3.forward;
            _speed = outcome.Speed;
            _damage = new DamageInfo(outcome.Damage, outcome.Element, outcome.Variant);
            _status = outcome.Status;
            _knockback = outcome.Knockback;
            _lifetime = lifetime;
            _impactEffect = impactEffect;
            _owner = owner;
            transform.forward = _direction;
        }

        private void Update()
        {
            transform.position += _direction * (_speed * Time.deltaTime);
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_owner != null && other.transform.root == _owner.transform.root) return; // pass through the caster

            var target = other.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.Apply(_damage);
                if (!_status.IsEmpty) target.ApplyStatus(_status);
                if (_knockback > 0f) target.ApplyKnockback(KnockbackImpulse.Directional(_direction, _knockback));
                FactionMember.RegisterHit(other.gameObject, _owner);
            }

            if (_impactEffect != null)
                Instantiate(_impactEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
