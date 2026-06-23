using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A rooted carnivorous plant. It doesn't move, but anyone who strays within reach is <em>grabbed</em> — a
    /// Control status (interrupts casting, pins movement) plus a bite of damage, on a cooldown. It has its own
    /// <see cref="Damageable"/>, so it can be cut down, and being stunned/controlled (e.g. frozen) stops it.
    /// Sits on a placeholder mesh; the real plant model is an artist's job.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class PlantFlytrap : MonoBehaviour
    {
        [SerializeField] private float grabRange = PlantTuning.SnaptrapRange;
        [SerializeField] private float grabHold = PlantTuning.SnaptrapHold;
        [SerializeField] private float grabDamage = PlantTuning.SnaptrapDamage;
        [SerializeField] private float cooldown = PlantTuning.SnaptrapCooldown;
        [SerializeField] private float retargetInterval = 0.5f;

        private Damageable _self;
        private Transform _player;
        private IDamageable _playerDamage;
        private float _cd;
        private float _retarget;

        private void Awake() => _self = GetComponent<Damageable>();
        private void Start() => Acquire();

        private void Acquire()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            _player = p != null ? p.transform : null;
            _playerDamage = p != null ? p.GetComponentInParent<IDamageable>() : null;
        }

        private void Update()
        {
            if (_self.IsStunned || _self.IsControlled) return;
            if (_cd > 0f) _cd -= Time.deltaTime;

            _retarget -= Time.deltaTime;
            if (_player == null && _retarget <= 0f) { Acquire(); _retarget = retargetInterval; }
            if (_player == null || _cd > 0f) return;

            float d = Vector3.Distance(_player.position, transform.position);
            if (d <= grabRange)
            {
                _playerDamage?.Apply(new DamageInfo(grabDamage, Element.Earth));
                _playerDamage?.ApplyStatus(new StatusEffect(StatusKind.Control, 1f, grabHold));
                AudioController.Instance?.PlayImpact(Element.Earth, transform.position);
                _cd = cooldown;
            }
        }
    }
}
