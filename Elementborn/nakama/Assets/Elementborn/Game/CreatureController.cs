using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A wild creature: it wanders peacefully (faction Neutral, so enemies leave it be), but fights back
    /// against anything that attacks it — which is how you weaken one to tame it. Locomotion follows its
    /// kind: ground beasts walk the terrain, water creatures swim at the surface, flyers hover. Pairs with
    /// a <see cref="Tameable"/> and <see cref="Damageable"/>; give the prefab a collider so hits register.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    [RequireComponent(typeof(FactionMember))]
    [RequireComponent(typeof(Tameable))]
    public sealed class CreatureController : MonoBehaviour
    {
        [SerializeField] private CreatureKind kind = CreatureKind.Horse;

        /// <summary>This creature's kind (so its mix-attack controller, etc., can look up its data).</summary>
        public CreatureKind Kind => kind;
        [SerializeField] private float visionRange = 22f;
        [SerializeField] private float wanderRadius = 8f;
        [SerializeField] private float repathInterval = 3f;
        [SerializeField] private float retargetInterval = 0.5f;
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float attackCooldown = 1.6f;
        [SerializeField] private float flyAltitude = 6f;
        [SerializeField] private float waterLevel = 0f;

        private CreatureCombatStats _stats;
        private LocomotionType _locomotion;
        private Element? _element;
        private Damageable _self;
        private FactionMember _faction;
        private Tameable _tameable;

        private Vector3 _home;
        private Vector3 _wanderTarget;
        private Transform _target;
        private IDamageable _targetDamageable;
        private float _repathTimer;
        private float _retargetTimer;
        private float _attackTimer;

        public void Configure(CreatureKind newKind)
        {
            kind = newKind;
            Apply();
        }

        private void Awake()
        {
            _self = GetComponent<Damageable>();
            _faction = GetComponent<FactionMember>();
            _tameable = GetComponent<Tameable>();
            _home = transform.position;
            _wanderTarget = _home;
        }

        private void Start()
        {
            Apply();
            if (_self != null && _self.Health != null) _self.Health.Died += OnDefeated;
        }

        private void OnDefeated() => QuestEvents.RaiseCreatureDefeated(kind.ToString());

        private void OnDestroy()
        {
            if (_self != null && _self.Health != null) _self.Health.Died -= OnDefeated;
        }

        private void Apply()
        {
            _stats = CreatureCombat.For(kind);
            _locomotion = Locomotion.For(kind);
            _element = CreatureCatalog.For(kind).Element;

            if (_tameable != null) _tameable.SetKind(kind);
            if (_faction != null) _faction.Configure(Faction.Neutral, _element); // peaceful until provoked
            if (_self != null) _self.SetMaxHealth(_stats.MaxHealth);
        }

        private void Update()
        {
            _retargetTimer -= Time.deltaTime;
            if (_retargetTimer <= 0f)
            {
                var hostile = _faction != null ? _faction.FindNearestHostile(visionRange) : null; // only when provoked
                _target = hostile != null ? hostile.transform : null;
                _targetDamageable = hostile != null ? hostile.GetComponentInParent<IDamageable>() : null;
                _retargetTimer = retargetInterval;
            }

            Vector3 pos = transform.position;
            Vector3 horizontal;

            if (_target != null)
                horizontal = Pursue(ref pos);
            else
                horizontal = Wander(ref pos);

            pos += horizontal * (_stats.MoveSpeed * Time.deltaTime);
            pos.y = HeightAt(pos);
            transform.position = pos;
        }

        private Vector3 Pursue(ref Vector3 pos)
        {
            Vector3 to = _target.position - pos;
            to.y = 0f;
            float dist = to.magnitude;
            Vector3 dir = dist > 0.001f ? to / dist : transform.forward;
            transform.forward = dir;

            if (dist <= attackRange)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                {
                    _targetDamageable?.Apply(new DamageInfo(_stats.Damage, _element ?? Element.Earth));
                    FactionMember.RegisterHit(_target.gameObject, gameObject);
                    _attackTimer = attackCooldown;
                }
                return Vector3.zero;
            }
            return dir;
        }

        private Vector3 Wander(ref Vector3 pos)
        {
            _repathTimer -= Time.deltaTime;
            Vector3 to = _wanderTarget - pos;
            to.y = 0f;

            if (_repathTimer <= 0f || to.magnitude < 0.6f)
            {
                Vector2 r = Random.insideUnitCircle * wanderRadius;
                _wanderTarget = _home + new Vector3(r.x, 0f, r.y);
                _repathTimer = repathInterval;
                return Vector3.zero;
            }

            Vector3 dir = to.normalized;
            transform.forward = dir;
            return dir * 0.5f; // amble at half speed when calm
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
