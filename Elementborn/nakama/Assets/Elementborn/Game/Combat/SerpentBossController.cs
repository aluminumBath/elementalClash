using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The water serpent — a unique boss that guards something good. It pursues to a preferred range and
    /// fights with telegraphed water and ice attacks plus a close tail-swipe (dodge by backing off during the
    /// windup). It hardens through phases as its health falls (<see cref="BossPhases"/>): calmer water pressure
    /// → water + slowing ice → a fast frenzy. On death it reveals its guarded reward and pays out. Sits on a
    /// placeholder mesh; the serpent model itself is an artist's job. Reuses <see cref="Damageable"/> for HP,
    /// status, and knockback, so it's slowed/frozen/shoved by the player's kit like anything else.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class SerpentBossController : MonoBehaviour
    {
        [Header("Body")]
        [SerializeField] private float bossHealth = 600f;
        [SerializeField] private float visionRange = 40f;
        [SerializeField] private float preferredRange = 9f;
        [SerializeField] private float moveSpeed = 4f;

        [Header("Attacks")]
        [SerializeField] private float baseAttackInterval = 2.2f;
        [SerializeField] private float telegraph = 0.8f;
        [SerializeField] private float waterDamage = 14f;
        [SerializeField] private float iceDamage = 10f;
        [SerializeField] private float iceSlow = 0.5f;
        [SerializeField] private float iceSlowDuration = 2.5f;
        [SerializeField] private float tailRange = 4f;
        [SerializeField] private float tailDamage = 22f;
        [SerializeField] private float tailKnockback = 12f;

        [Header("Reward (revealed on defeat)")]
        [SerializeField] private GameObject rewardOnDefeat;
        [SerializeField] private int rubyReward = 5;
        [SerializeField] private int scoreReward = 2000;

        private Damageable _self;
        private Transform _player;
        private IDamageable _playerDamage;
        private float _attackTimer;
        private bool _winding;
        private float _windTimer;
        private System.Action _pending;
        private bool _dead;

        private void Awake()
        {
            _self = GetComponent<Damageable>();
            if (rewardOnDefeat != null) rewardOnDefeat.SetActive(false); // locked behind the boss
        }

        private void Start()
        {
            _self.SetMaxHealth(bossHealth);
            _self.Health.Died += OnDied;
            _attackTimer = baseAttackInterval;
            Acquire();
        }

        private void Acquire()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            _player = p != null ? p.transform : null;
            _playerDamage = p != null ? p.GetComponentInParent<IDamageable>() : null;
        }

        private void Update()
        {
            if (_dead || _self.IsStunned || _self.IsControlled) return;
            if (_player == null) { Acquire(); return; }

            Vector3 to = _player.position - transform.position;
            float d = to.magnitude;
            if (d > visionRange) return;

            Vector3 flat = to; flat.y = 0f;
            Vector3 dir = flat.sqrMagnitude > 0.001f ? flat.normalized : transform.forward;
            transform.forward = dir;

            BossPhase phase = BossPhases.For(_self.Health.Current / Mathf.Max(1f, _self.Health.Max));

            if (_winding)
            {
                _windTimer -= Time.deltaTime;
                if (_windTimer <= 0f)
                {
                    _pending?.Invoke();
                    _pending = null;
                    _winding = false;
                    _attackTimer = BossPhases.AttackInterval(phase, baseAttackInterval);
                }
                return; // commit to the attack: hold position during the windup
            }

            if (d > preferredRange * 1.1f)
                transform.position += dir * (moveSpeed * _self.SpeedMultiplier * Time.deltaTime);
            else if (d < preferredRange * 0.6f)
                transform.position -= dir * (moveSpeed * 0.5f * Time.deltaTime);

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f) ChooseAttack(phase, d);
        }

        private void ChooseAttack(BossPhase phase, float distance)
        {
            _winding = true;
            _windTimer = phase == BossPhase.Frenzy ? telegraph * 0.7f : telegraph;

            if (distance <= tailRange && (phase == BossPhase.Frenzy || Random.value < 0.4f))
            {
                _pending = TailSwipe; // dodgeable: back off during the windup
                return;
            }
            if (phase == BossPhase.Calm)
                _pending = WaterBarrage;
            else
                _pending = Random.value < 0.5f ? (System.Action)WaterBarrage : IceShard;
        }

        private void WaterBarrage() => _playerDamage?.Apply(new DamageInfo(waterDamage, Element.Water));

        private void IceShard()
        {
            _playerDamage?.Apply(new DamageInfo(iceDamage, Element.Water));
            _playerDamage?.ApplyStatus(new StatusEffect(StatusKind.Slow, iceSlow, iceSlowDuration));
        }

        private void TailSwipe()
        {
            if (_player == null) return;
            Vector3 to = _player.position - transform.position;
            if (to.magnitude > tailRange * 1.15f) return; // dodged
            _playerDamage?.Apply(new DamageInfo(tailDamage, Element.Water));
            Vector3 push = to; push.y = 0.2f;
            _playerDamage?.ApplyKnockback(push.normalized * tailKnockback);
        }

        private void OnDied()
        {
            if (_dead) return;
            _dead = true;
            if (rewardOnDefeat != null) rewardOnDefeat.SetActive(true); // the prize appears
            PlayerInventory.Instance?.AddCurrency(Currency.Ruby, rubyReward);
            ScoreController.Instance?.AddKill(scoreReward);
        }
    }
}
