using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Combat/Attack Definition", fileName = "CombatAttack")]
    public sealed class CombatAttackDefinition : ScriptableObject
    {
        [SerializeField] private string attackId = "";
        [SerializeField] private string displayName = "Attack";
        [TextArea]
        [SerializeField] private string description = "";
        [SerializeField] private AbilityElementType element = AbilityElementType.Neutral;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float critChance = 0.05f;
        [SerializeField] private float critMultiplier = 1.5f;
        [SerializeField] private float knockbackForce = 0f;
        [SerializeField] private bool useEquipmentBonuses = true;
        [SerializeField] private AttackOriginType originType = AttackOriginType.OnFoot;
        [SerializeField] private StatusEffectDefinition statusToApply;
        [SerializeField] private Sprite icon;

        public string AttackId => string.IsNullOrWhiteSpace(attackId) ? name : attackId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? AttackId : displayName;
        public string Description => description;
        public AbilityElementType Element => element;
        public float BaseDamage => baseDamage;
        public float CritChance => Mathf.Clamp01(critChance);
        public float CritMultiplier => Mathf.Max(1f, critMultiplier);
        public float KnockbackForce => knockbackForce;
        public bool UseEquipmentBonuses => useEquipmentBonuses;
        public AttackOriginType OriginType => originType;
        public StatusEffectDefinition StatusToApply => statusToApply;
        public Sprite Icon => icon;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(attackId)) attackId = name;
            critChance = Mathf.Clamp01(critChance);
            critMultiplier = Mathf.Max(1f, critMultiplier);
        }
    }
}
