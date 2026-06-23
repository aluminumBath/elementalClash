using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Gives a creature its two-element signature attack. If the creature has a mix attack (the exotic apex
    /// creatures, rocs, thunderbirds), it unleashes it at the player when in range, on a cooldown — these are
    /// dangerous beasts, so they strike when you get close. Reads the kind from the <see cref="CreatureController"/>
    /// it sits beside; if the kind has no mix attack it quietly disables itself. Being stunned/controlled stops it.
    /// </summary>
    [RequireComponent(typeof(CreatureController))]
    public sealed class CreatureMixAttackController : MonoBehaviour
    {
        [SerializeField] private float cooldown = 4f;

        private CreatureController _creature;
        private Damageable _self;
        private Transform _player;
        private IDamageable _playerDamage;
        private MixAttack _attack;
        private float _cd;
        private float _retarget;

        private void Awake()
        {
            _creature = GetComponent<CreatureController>();
            _self = GetComponentInParent<Damageable>();
        }

        private void Start()
        {
            _attack = _creature != null ? CreatureMixAttacks.For(_creature.Kind) : MixAttack.None;
            if (_attack.IsNone) { enabled = false; return; }
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
            if (_self != null && (_self.IsStunned || _self.IsControlled)) return;
            if (_cd > 0f) _cd -= Time.deltaTime;

            _retarget -= Time.deltaTime;
            if (_player == null && _retarget <= 0f) { Acquire(); _retarget = 0.5f; }
            if (_player == null || _cd > 0f) return;

            if (Vector3.Distance(_player.position, transform.position) <= _attack.Range)
            {
                _playerDamage?.Apply(new DamageInfo(_attack.Damage, _attack.Primary));
                if (!_attack.Status.IsEmpty) _playerDamage?.ApplyStatus(_attack.Status);
                if (_attack.Knockback > 0f)
                    _playerDamage?.ApplyKnockback((_player.position - transform.position).normalized * _attack.Knockback);
                AudioController.Instance?.PlayImpact(_attack.Primary, _player.position);
                _cd = cooldown;
            }
        }
    }
}
