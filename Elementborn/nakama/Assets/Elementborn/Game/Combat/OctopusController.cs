using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// An underwater ambusher. It closes on the player and, in range, <em>grabs</em> — dealing a little damage
    /// and, more importantly, applying a Control status (which interrupts casting and pins movement) while
    /// forcing the victim to hold their breath, so even a water user starts to drown in its grip. It has its
    /// own <see cref="Damageable"/>, so it can be killed; being stunned/controlled stops it too. Built to sit
    /// on a placeholder mesh; the real organic model is an artist's job.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class OctopusController : MonoBehaviour
    {
        [SerializeField] private float visionRange = 14f;
        [SerializeField] private float grabRange = 2.2f;
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float grabHold = 1.4f;       // Control duration (interrupt + pin)
        [SerializeField] private float suffocateSeconds = 2.5f;
        [SerializeField] private float grabDamage = 6f;
        [SerializeField] private float cooldown = 3f;
        [SerializeField] private float retargetInterval = 0.5f;

        private Damageable _self;
        private Transform _player;
        private IDamageable _playerDamage;
        private UnderwaterController _playerWater;
        private float _cd;
        private float _retarget;

        private void Awake() => _self = GetComponent<Damageable>();
        private void Start() => Acquire();

        private void Acquire()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            _player = p != null ? p.transform : null;
            _playerDamage = p != null ? p.GetComponentInParent<IDamageable>() : null;
            _playerWater = p != null ? p.GetComponentInParent<UnderwaterController>() : null;
        }

        private void Update()
        {
            if (_self.IsStunned || _self.IsControlled) return; // it can be frozen/gripped too
            if (_cd > 0f) _cd -= Time.deltaTime;

            _retarget -= Time.deltaTime;
            if (_player == null && _retarget <= 0f) { Acquire(); _retarget = retargetInterval; }
            if (_player == null) return;

            Vector3 to = _player.position - transform.position;
            float d = to.magnitude;
            if (d > visionRange) return;

            Vector3 dir = d > 0.001f ? to / d : transform.forward;
            transform.forward = dir;

            if (d > grabRange)
                transform.position += dir * (moveSpeed * _self.SpeedMultiplier * Time.deltaTime);
            else if (_cd <= 0f)
                Grab();
        }

        private void Grab()
        {
            _cd = cooldown;
            _playerDamage?.Apply(new DamageInfo(grabDamage, Element.Water));
            _playerDamage?.ApplyStatus(new StatusEffect(StatusKind.Control, 1f, grabHold)); // interrupt + pin
            _playerWater?.Suffocate(suffocateSeconds);                                       // and drown them
            if (_player != null) FactionMember.RegisterHit(_player.gameObject, gameObject);
        }
    }
}
