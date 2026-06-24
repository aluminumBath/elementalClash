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
    [RequireComponent(typeof(HitReaction))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyKind kind = EnemyKind.Grunt;
        [SerializeField] private Transform target;                 // optional manual override
        [SerializeField] private float visionRange = 30f;
        [SerializeField] private float retargetInterval = 0.5f;
        [SerializeField] private float gravity = -9.81f;
        [Tooltip("Windup before an attack lands, so it can be dodged. 0 = instant (default, world enemies).")]
        [SerializeField] private float telegraphTime = 0f;

        /// <summary>Raised when a telegraphed attack begins its windup (hook for VFX/audio).</summary>
        public event System.Action AttackTelegraphed;
        /// <summary>Raised when a telegraphed attack connects.</summary>
        public event System.Action AttackLanded;
        /// <summary>Raised when the target escaped the attack's range during the windup.</summary>
        public event System.Action AttackDodged;

        /// <summary>Give this enemy a dodgeable windup (used by the arena). 0 restores instant attacks.</summary>
        public void SetTelegraph(float seconds) => telegraphTime = Mathf.Max(0f, seconds);
        public bool IsWindingUp => _winding;

        private EnemyStats _stats;
        private Damageable _self;
        private FactionMember _faction;
        private CharacterController _cc;
        private IDamageable _targetDamageable;
        private bool _autoTarget;
        private float _retargetTimer;
        private float _attackTimer;
        private bool _winding;
        private float _windTimer;
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

        /// <summary>Configure from a registry id — built-in ("Grunt") or modded — so data-driven and modded
        /// enemies spawn through the same door. Falls back to a Grunt if the id is unknown.</summary>
        public void ConfigureById(string id, Faction faction, Element? element)
        {
            var def = EnemyRegistry.Get(id);
            if (def == null) { Configure(EnemyKind.Grunt, faction, element); return; }
            kind = def.BaseKind;
            _stats = def.Stats;
            if (_self != null) _self.SetMaxHealth(_stats.MaxHealth);
            var fm = _faction != null ? _faction : GetComponent<FactionMember>();
            fm?.Configure(faction, element ?? def.Element);
        }

        /// <summary>Force a specific target (the arena points every enemy at the player).</summary>
        public void SetTarget(Transform t)
        {
            target = t;
            _autoTarget = t == null;
            _targetDamageable = t != null ? t.GetComponentInParent<IDamageable>() : null;
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

                if (_winding)
                {
                    // Committed to a telegraphed attack: hold position, then strike (or whiff if dodged).
                    _windTimer -= Time.deltaTime;
                    if (_windTimer <= 0f) ResolveTelegraphedStrike(distance);
                }
                else
                {
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
                            if (telegraphTime > 0f)
                            {
                                _winding = true;
                                _windTimer = telegraphTime;
                                AttackTelegraphed?.Invoke();
                            }
                            else
                            {
                                Strike();
                                _attackTimer = _stats.AttackCooldown;
                            }
                        }
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

        private void ResolveTelegraphedStrike(float distance)
        {
            _winding = false;
            _attackTimer = _stats.AttackCooldown;

            // A little grace beyond exact range so a near-miss still reads as a fair hit.
            if (distance <= _stats.AttackRange * 1.15f)
            {
                Strike();
                AttackLanded?.Invoke();
            }
            else
            {
                AttackDodged?.Invoke();
                ScoreController.Instance?.RegisterDodge();
            }
        }

        private void Strike()
        {
            _targetDamageable?.Apply(new DamageInfo(_stats.Damage, Element.Earth));
            if (target != null) FactionMember.RegisterHit(target.gameObject, gameObject);
        }
    }
}
