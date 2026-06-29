using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Enemy AI/Combat Profile", fileName = "EnemyCombatProfile")]
    public sealed class EnemyCombatProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string profileId = "";
        [SerializeField] private string displayName = "Enemy Profile";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Archetype")]
        [SerializeField] private EnemyAiArchetype archetype = EnemyAiArchetype.BasicMelee;
        [SerializeField] private AbilityElementType preferredElement = AbilityElementType.Neutral;

        [Header("Perception")]
        [SerializeField] private float sightRange = 14f;
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float rangedAttackRange = 12f;
        [SerializeField] private float loseTargetRange = 24f;
        [SerializeField] private float hearingRange = 8f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float strafeSpeed = 2.5f;
        [SerializeField] private float fleeSpeed = 5f;
        [SerializeField] private float stoppingDistance = 1.6f;
        [SerializeField] private bool canStrafe = false;

        [Header("Combat")]
        [SerializeField] private float attackCooldownSeconds = 1.5f;
        [SerializeField] private float rangedCooldownSeconds = 2.5f;
        [SerializeField] private float lowHealthFleePercent = 0.2f;
        [SerializeField] private bool fleeWhenLowHealth = false;
        [SerializeField] private bool useWeaknesses = true;
        [SerializeField] private bool canAttackBoats = false;
        [SerializeField] private bool canAttackCreatures = false;

        [Header("Boss Lite")]
        [SerializeField] private bool usesBossPhases = false;

        public string ProfileId => string.IsNullOrWhiteSpace(profileId) ? name : profileId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ProfileId : displayName;
        public string Description => description;
        public EnemyAiArchetype Archetype => archetype;
        public AbilityElementType PreferredElement => preferredElement;
        public float SightRange => Mathf.Max(0.1f, sightRange);
        public float AttackRange => Mathf.Max(0.1f, attackRange);
        public float RangedAttackRange => Mathf.Max(0.1f, rangedAttackRange);
        public float LoseTargetRange => Mathf.Max(SightRange, loseTargetRange);
        public float HearingRange => Mathf.Max(0.1f, hearingRange);
        public float PatrolSpeed => Mathf.Max(0f, patrolSpeed);
        public float ChaseSpeed => Mathf.Max(0f, chaseSpeed);
        public float StrafeSpeed => Mathf.Max(0f, strafeSpeed);
        public float FleeSpeed => Mathf.Max(0f, fleeSpeed);
        public float StoppingDistance => Mathf.Max(0f, stoppingDistance);
        public bool CanStrafe => canStrafe;
        public float AttackCooldownSeconds => Mathf.Max(0f, attackCooldownSeconds);
        public float RangedCooldownSeconds => Mathf.Max(0f, rangedCooldownSeconds);
        public float LowHealthFleePercent => Mathf.Clamp01(lowHealthFleePercent);
        public bool FleeWhenLowHealth => fleeWhenLowHealth;
        public bool UseWeaknesses => useWeaknesses;
        public bool CanAttackBoats => canAttackBoats;
        public bool CanAttackCreatures => canAttackCreatures;
        public bool UsesBossPhases => usesBossPhases;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                profileId = name;
            }

            sightRange = Mathf.Max(0.1f, sightRange);
            attackRange = Mathf.Max(0.1f, attackRange);
            rangedAttackRange = Mathf.Max(0.1f, rangedAttackRange);
            loseTargetRange = Mathf.Max(sightRange, loseTargetRange);
            hearingRange = Mathf.Max(0.1f, hearingRange);
            patrolSpeed = Mathf.Max(0f, patrolSpeed);
            chaseSpeed = Mathf.Max(0f, chaseSpeed);
            strafeSpeed = Mathf.Max(0f, strafeSpeed);
            fleeSpeed = Mathf.Max(0f, fleeSpeed);
            stoppingDistance = Mathf.Max(0f, stoppingDistance);
            attackCooldownSeconds = Mathf.Max(0f, attackCooldownSeconds);
            rangedCooldownSeconds = Mathf.Max(0f, rangedCooldownSeconds);
            lowHealthFleePercent = Mathf.Clamp01(lowHealthFleePercent);
        }
    }
}
