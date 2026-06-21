using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A configurable enemy. An archetype (Grunt / Brute / Runner / Archer / Elementalist) sets its
    /// health, speed, damage, range, cooldown, and score value. It targets the nearest character its
    /// <see cref="FactionMember"/> is hostile to (re-scanned periodically), so a fire fighter chases
    /// other-element channelers and bandits but ignores its own element and peaceful folk — until
    /// something attacks it. Melee kinds close in; ranged kinds hold distance and kite. Movement reads
    /// its <see cref="Damageable"/> (slowed while iced, frozen while stunned/gripped, shoved by knockback).
    /// On death it awards score. Assign a Target to override the faction scan.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [RequireComponent(typeof(FactionMember))]
    [RequireComponent(typeof(CharacterController))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyKind kind = EnemyKind.Grunt;
        [SerializeField] private Transform target;                 // optional manual override
        [SerializeField] private float visionRange = 30f;
        [SerializeField] private float retargetInterval = 0.5f;
        [SerializeField] private float gravity = -9.81f;

        private EnemyStats _stats;
        private Damageable _self;
        private FactionMember _faction;
        private CharacterController _cc;
        private IDamageable _targetDamageable;
        private bool _autoTarget;
        private float _retargetTimer;
        private float _attackTimer;
        private float _verticalVelocity;
        private bool _scored;

        /// <summary>Sets the archetype (used by the spawner to pick by region danger/biome).</summary>
        public void Configure(EnemyKind newKind)
        {
            kind = newKind;
            _stats = EnemyArchetypes.For(kind);
            if (_self != null) _self.SetMaxHealth(_stats.MaxHealth);
        }

        /// <summary>Sets archetype plus allegiance/element (element-typed Wild enemies vs weapon bandits).</summary>
        public void Configure(EnemyKind newKind, Faction faction, Element? element)
        {
            Configure(newKind);
            var fm = _faction != null ? _faction : GetComponent<FactionMember>();
            fm?.Configure(faction, element);
        }

        private void Awake()
        {
            _self = GetComponent<Damageable>();
            _cc = GetComponent<CharacterController>();
            _faction = GetComponent<FactionMember>();
        }

        private void Start()
        {
            _stats = EnemyArchetypes.For(kind);
            _self.SetMaxHealth(_stats.MaxHealth);
            _self.Health.Died += OnDied;

            _autoTarget = target == null;
            if (!_autoTarget) _targetDamageable = target.GetComponentInParent<IDamageable>();
        }

        private void OnDied()
        {
            if (_scored) return;
            _scored = true;
            ScoreController.Instance?.AddKill(_stats.ScoreValue);

            var inv = PlayerInventory.Instance;
            if (inv != null)
            {
                inv.AddCurrency(Currency.Silver, Mathf.Max(1, _stats.ScoreValue / 10));
                if (Random.value < 0.08f) inv.AddCurrency(Currency.Ruby, 1);
            }
        }

        private void Acquire()
        {
            var hostile = _faction != null ? _faction.FindNearestHostile(visionRange) : null;
            target = hostile != null ? hostile.transform : null;
            _targetDamageable = hostile != null ? hostile.GetComponentInParent<IDamageable>() : null;
        }

        private void Update()
        {
            if (_autoTarget)
            {
                _retargetTimer -= Time.deltaTime;
                if (_retargetTimer <= 0f) { Acquire(); _retargetTimer = retargetInterval; }
            }

            Vector3 horizontal = Vector3.zero;
            bool frozen = _self.IsStunned || _self.IsControlled;

            if (!frozen && target != null)
            {
                Vector3 toTarget = target.position - transform.position;
                toTarget.y = 0f;
                float distance = toTarget.magnitude;
                Vector3 dir = distance > 0.001f ? toTarget / distance : transform.forward;
                transform.forward = dir;

                float preferred = _stats.IsRanged ? _stats.AttackRange * 0.8f : _stats.AttackRange;

                if (distance > preferred)
                    horizontal = dir * (_stats.MoveSpeed * _self.SpeedMultiplier);
                else if (_stats.IsRanged && distance < preferred * 0.5f)
                    horizontal = -dir * (_stats.MoveSpeed * 0.6f * _self.SpeedMultiplier); // kite back

                if (distance <= _stats.AttackRange)
                {
                    _attackTimer -= Time.deltaTime;
                    if (_attackTimer <= 0f)
                    {
                        _targetDamageable?.Apply(new DamageInfo(_stats.Damage, Element.Earth));
                        if (target != null) FactionMember.RegisterHit(target.gameObject, gameObject);
                        _attackTimer = _stats.AttackCooldown;
                    }
                }
            }

            // Knockback applies even while frozen — a stunned enemy can still be shoved back.
            horizontal += _self.KnockbackVelocity;

            if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = horizontal;
            motion.y = _verticalVelocity;
            _cc.Move(motion * Time.deltaTime);
        }
    }
}
