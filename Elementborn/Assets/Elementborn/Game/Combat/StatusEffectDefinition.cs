using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Combat/Status Effect", fileName = "StatusEffect")]
    public sealed class StatusEffectDefinition : ScriptableObject
    {
        [SerializeField] private string effectId = "";
        [SerializeField] private string displayName = "Status Effect";
        [TextArea]
        [SerializeField] private string description = "";
        [SerializeField] private StatusEffectType effectType = StatusEffectType.None;
        [SerializeField] private AbilityElementType element = AbilityElementType.Neutral;
        [SerializeField] private float durationSeconds = 4f;
        [SerializeField] private float tickIntervalSeconds = 1f;
        [SerializeField] private float tickDamage = 0f;
        [SerializeField] private float moveSpeedPercentDelta = 0f;
        [SerializeField] private float attackPowerPercentDelta = 0f;
        [SerializeField] private float defensePercentDelta = 0f;
        [SerializeField] private bool uniquePerTarget = true;
        [SerializeField] private Sprite icon;

        public string EffectId => string.IsNullOrWhiteSpace(effectId) ? name : effectId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? EffectId : displayName;
        public string Description => description;
        public StatusEffectType EffectType => effectType;
        public AbilityElementType Element => element;
        public float DurationSeconds => Mathf.Max(0.1f, durationSeconds);
        public float TickIntervalSeconds => Mathf.Max(0.1f, tickIntervalSeconds);
        public float TickDamage => tickDamage;
        public float MoveSpeedPercentDelta => moveSpeedPercentDelta;
        public float AttackPowerPercentDelta => attackPowerPercentDelta;
        public float DefensePercentDelta => defensePercentDelta;
        public bool UniquePerTarget => uniquePerTarget;
        public Sprite Icon => icon;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(effectId)) effectId = name;
            durationSeconds = Mathf.Max(0.1f, durationSeconds);
            tickIntervalSeconds = Mathf.Max(0.1f, tickIntervalSeconds);
        }
    }
}
