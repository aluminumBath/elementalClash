using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A tamed companion fighting for you (faction Ally). It follows the player, then chases and attacks
    /// the nearest hostile with its kind's element — applying that kind's on-hit status (ice slows,
    /// lightning stuns, fire burns), shrugging off what it's immune to, and using its trick: the spider
    /// lays flammable <see cref="WebTrap"/>s, the cats and hound blink to flank, the phoenix is reborn
    /// once on death. Movement follows its locomotion type. Needs Damageable + FactionMember.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [RequireComponent(typeof(FactionMember))]
    public sealed class CompanionController : MonoBehaviour
    {
        [SerializeField] private CreatureKind kind = CreatureKind.Dog;
        [SerializeField] private float followDistance = 3.5f;
        [SerializeField] private float visionRange = 26f;
        [SerializeField] private float attackRange = 11f;
        [SerializeField] private float attackCooldown = 1.4f;
        [SerializeField] private float blinkCooldown = 6f;
        [SerializeField] private float webCooldown = 5f;
        [SerializeField] private float flyAltitude = 5f;
        [SerializeField] private float waterLevel = 0f;

        private CompanionProfile _profile;
        private CreatureCombatStats _combat;
        private LocomotionType _locomotion;
        private Damageable _self;
        private FactionMember _faction;
        private Transform _player;

        private Transform _target;
        private IDamageable _targetDamageable;
        private float _retargetTimer;
        private float _attackTimer;
        private float _blinkTimer;
        private float _webTimer;
        private bool _applied;
        private bool _reborn;

        public void Configure(CreatureKind newKind)
        {
            kind = newKind;
            if (_self == null) { _self = GetComponent<Damageable>(); _faction = GetComponent<FactionMember>(); }
            Apply();
        }

        private void Awake()
        {
            _self = GetComponent<Damageable>();
            _faction = GetComponent<FactionMember>();
        }

        private void Start()
        {
            if (!_applied) Apply();
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null) _player = tagged.transform;
        }

        private void Apply()
        {
            _profile = CompanionProfiles.For(kind);
            _combat = CreatureCombat.For(kind);
            _locomotion = Locomotion.For(kind);

            var info = CreatureCatalog.For(kind);
            _faction.Configure(Faction.Ally, info.Element);
            _self.SetMaxHealth(_combat.MaxHealth);
            _self.SetImmunity(_profile.Immunity);
            _self.DestroyOnDeath = false;          // we handle death (rebirth)
            _self.Health.Died -= OnDied;
            _self.Health.Died += OnDied;
            _applied = true;
        }

        private void OnDied()
        {
            if (_profile.CanRebirth && !_reborn)
            {
                _reborn = true;
                _self.Health.Revive();
            }
            else Destroy(gameObject);
        }

        private void Update()
        {
            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f)
            {
                var hostile = _faction.FindNearestHostile(visionRange);
                _target = hostile != null ? hostile.transform : null;
                _targetDamageable = hostile != null ? hostile.GetComponentInParent<IDamageable>() : null;
                _retargetTimer = 0.5f;
            }

            Vector3 pos = transform.position;
            Vector3 step = _target != null ? Fight(pos) : Follow(pos);

            pos += step * (_combat.MoveSpeed * Time.deltaTime);
            pos.y = HeightAt(pos);
            transform.position = pos;
        }

        private Vector3 Fight(Vector3 pos)
        {
            _blinkTimer -= Time.deltaTime;
            _webTimer -= Time.deltaTime;

            Vector3 to = _target.position - pos;
            to.y = 0f;
            float dist = to.magnitude;
            Vector3 dir = dist > 0.001f ? to / dist : transform.forward;
            transform.forward = dir;

            if (_profile.CanBlink && _blinkTimer <= 0f && dist > 3f)
            {
                Blink();
                _blinkTimer = blinkCooldown;
                return Vector3.zero;
            }

            if (dist <= attackRange)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _targetDamageable?.Apply(new DamageInfo(_combat.Damage, _profile.AttackElement, _profile.AttackVariant));
                    if (_profile.OnHitStatus != StatusKind.None)
                        _targetDamageable?.ApplyStatus(new StatusEffect(_profile.OnHitStatus, _profile.StatusMagnitude, _profile.StatusDuration));
                    FactionMember.RegisterHit(_target.gameObject, gameObject);
                    _attackTimer = attackCooldown;
                }

                if (_profile.CanWeb && _webTimer <= 0f)
                {
                    SpawnWeb(_target.position);
                    _webTimer = webCooldown;
                }
                return Vector3.zero;
            }
            return dir;
        }

        private Vector3 Follow(Vector3 pos)
        {
            if (_player == null) return Vector3.zero;
            Vector3 to = _player.position - pos;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist <= followDistance) return Vector3.zero;
            return to.normalized;
        }

        private void Blink()
        {
            // dive/dig and reappear flanking the target
            Vector2 r = Random.insideUnitCircle.normalized * 3f;
            Vector3 spot = _target.position + new Vector3(r.x, 0f, r.y);
            spot.y = HeightAt(spot);
            transform.position = spot;
        }

        private void SpawnWeb(Vector3 at)
        {
            var go = new GameObject("WebTrap");
            go.transform.position = new Vector3(at.x, HeightAt(at), at.z);
            go.AddComponent<WebTrap>().Configure(4f, 1.3f, 35f, 6f);
        }

        private float HeightAt(Vector3 pos)
        {
            switch (_locomotion)
            {
                case LocomotionType.Water: return waterLevel;
                case LocomotionType.Flying:
                    return TerrainHeight.Sample(pos) + flyAltitude;
                default:
                    return TerrainHeight.Sample(pos);
            }
        }
    }
}
