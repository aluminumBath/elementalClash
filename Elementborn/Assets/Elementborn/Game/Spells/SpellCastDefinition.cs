using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Spells/Spell Cast Definition", fileName = "SpellCast")]
    public sealed class SpellCastDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string spellId = "";
        [SerializeField] private string displayName = "Spell";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Links")]
        [SerializeField] private AbilityDefinition ability;
        [SerializeField] private CombatAttackDefinition attack;

        [Header("Casting")]
        [SerializeField] private SpellTargetingMode targetingMode = SpellTargetingMode.ForwardProjectile;
        [SerializeField] private SpellResourceType resourceType = SpellResourceType.Focus;
        [SerializeField] private float resourceCost = 10f;
        [SerializeField] private float castTimeSeconds = 0f;
        [SerializeField] private float cooldownSeconds = 2f;
        [SerializeField] private bool interruptible = true;
        [SerializeField] private bool queueable = true;

        [Header("Targeting")]
        [SerializeField] private float range = 18f;
        [SerializeField] private float radius = 3f;
        [SerializeField] private LayerMask targetMask = ~0;

        [Header("Execution")]
        [SerializeField] private ProjectileCombatEmitter projectilePrefab;
        [SerializeField] private GameObject aoeVisualPrefab;
        [SerializeField] private StatusEffectDefinition selfStatus;
        [SerializeField] private int skillPointRewardOnFirstCast = 0;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private string animationTrigger = "";
        [SerializeField] private string castVfxResourcePath = "";
        [SerializeField] private string castSfxResourcePath = "";

        public string SpellId => string.IsNullOrWhiteSpace(spellId) ? name : spellId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? SpellId : displayName;
        public string Description => description;
        public AbilityDefinition Ability => ability;
        public CombatAttackDefinition Attack => attack;
        public SpellTargetingMode TargetingMode => targetingMode;
        public SpellResourceType ResourceType => resourceType;
        public float ResourceCost => Mathf.Max(0f, resourceCost);
        public float CastTimeSeconds => Mathf.Max(0f, castTimeSeconds);
        public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
        public bool Interruptible => interruptible;
        public bool Queueable => queueable;
        public float Range => Mathf.Max(0f, range);
        public float Radius => Mathf.Max(0f, radius);
        public LayerMask TargetMask => targetMask;
        public ProjectileCombatEmitter ProjectilePrefab => projectilePrefab;
        public GameObject AoeVisualPrefab => aoeVisualPrefab;
        public StatusEffectDefinition SelfStatus => selfStatus;
        public int SkillPointRewardOnFirstCast => Mathf.Max(0, skillPointRewardOnFirstCast);
        public Sprite Icon => icon;
        public string AnimationTrigger => animationTrigger;
        public string CastVfxResourcePath => castVfxResourcePath;
        public string CastSfxResourcePath => castSfxResourcePath;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                spellId = name;
            }

            resourceCost = Mathf.Max(0f, resourceCost);
            castTimeSeconds = Mathf.Max(0f, castTimeSeconds);
            cooldownSeconds = Mathf.Max(0f, cooldownSeconds);
            range = Mathf.Max(0f, range);
            radius = Mathf.Max(0f, radius);
            skillPointRewardOnFirstCast = Mathf.Max(0, skillPointRewardOnFirstCast);
        }
    }
}
