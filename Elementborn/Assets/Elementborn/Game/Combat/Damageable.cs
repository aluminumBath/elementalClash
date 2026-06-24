using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Gives a GameObject hit points, status effects, and knockback, bridging the pure-logic
    /// Core types to the scene. Controllers (enemy, player) read <see cref="SpeedMultiplier"/>,
    /// <see cref="IsStunned"/>, <see cref="IsControlled"/>, and <see cref="KnockbackVelocity"/>
    /// and fold them into their own movement — no Rigidbody required.
    /// </summary>
    public sealed class Damageable : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [Tooltip("Enemies destroy on death; turn off for the player (a RespawnController handles it).")]
        [SerializeField] private bool destroyOnDeath = true;

        public bool DestroyOnDeath { get => destroyOnDeath; set => destroyOnDeath = value; }

        private DamageImmunity _immunity = DamageImmunity.None;
        private float _shieldEndTime;
        private float _shieldReduction;
        private float _damageReduction; // persistent (e.g. faction defense perk); negative = takes more
        private Element _lastSource = Element.Fire;

        public Health Health { get; private set; }
        public StatusController Status { get; private set; }
        public KnockbackController Knockback { get; private set; }

        public float SpeedMultiplier => Status?.SpeedMultiplier ?? 1f;
        public bool IsStunned => Status?.IsStunned ?? false;
        public bool IsControlled => Status?.IsControlled ?? false;
        public Vector3 KnockbackVelocity => Knockback?.Velocity ?? Vector3.zero;

        private void Awake()
        {
            Health = new Health(maxHealth);
            Status = new StatusController();
            Knockback = new KnockbackController();
            Health.Died += HandleDeath;
        }

        private void Update()
        {
            Status.Tick(Time.deltaTime);
            Knockback.Tick(Time.deltaTime);
            if (Status.BurnDamagePerSecond > 0f)
                Health.Apply(new DamageInfo(Status.BurnDamagePerSecond * Time.deltaTime, Element.Fire));
        }

        public void Apply(DamageInfo damage)
        {
            if (_immunity.Blocks(damage)) return;
            if (Time.time < _shieldEndTime && _shieldReduction > 0f)
                damage = new DamageInfo(damage.Amount * (1f - _shieldReduction), damage.Source, damage.Variant);
            if (_damageReduction != 0f)
                damage = new DamageInfo(Mathf.Max(0f, damage.Amount * (1f - _damageReduction)), damage.Source, damage.Variant);
            _lastSource = damage.Source;
            if (damage.Amount >= 1f) AudioController.Instance?.PlayImpact(damage.Source, transform.position);
            Health.Apply(damage);
            if (damage.Amount >= 1f) CombatFeedback.RaiseHit(transform.position, damage.Amount, damage.Source);
        }

        /// <summary>A standing incoming-damage modifier (faction defense perk). Positive cuts damage, negative
        /// amplifies it; clamped to a sane band.</summary>
        public void SetDamageReduction(float reduction01) => _damageReduction = Mathf.Clamp(reduction01, -0.9f, 0.95f);

        /// <summary>Briefly reduce incoming direct damage — the Defend / barrier ability.</summary>
        public void Shield(float seconds, float reduction01)
        {
            _shieldEndTime = Time.time + Mathf.Max(0f, seconds);
            _shieldReduction = Mathf.Clamp01(reduction01);
        }

        /// <summary>Make this creature immune to certain damage (used by companions).</summary>
        public void SetImmunity(DamageImmunity immunity) => _immunity = immunity;
        public void ApplyStatus(StatusEffect status) => Status.Add(status);
        public void ApplyKnockback(Vector3 impulse) => Knockback.Add(impulse);

        /// <summary>Sets the maximum <em>in place</em> — preserves the Health object so existing Died/Damaged
        /// subscribers (respawn, scoring, weapon reactions) stay valid. Refills to full, matching prior behaviour.</summary>
        public void SetMaxHealth(float max)
        {
            if (Health == null)
            {
                Health = new Health(max);
                Health.Died += HandleDeath;
            }
            else
            {
                Health.SetMax(max);
            }
        }

        private void HandleDeath()
        {
            AudioController.Instance?.PlayImpact(_lastSource, transform.position);
            CombatFeedback.RaiseDefeated(transform.position, _lastSource);
            if (destroyOnDeath) Destroy(gameObject);
        }
    }
}
