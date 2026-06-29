using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CombatHitContext
    {
        public GameObject Source;
        public CombatAttackDefinition AttackDefinition;
        public AbilityElementType Element = AbilityElementType.Neutral;
        public float BaseDamage = 10f;
        public float CritChance = 0f;
        public float CritMultiplier = 1.5f;
        public float KnockbackForce = 0f;
        public bool UseEquipmentBonuses = true;
        public AttackOriginType OriginType = AttackOriginType.OnFoot;
        public StatusEffectDefinition StatusToApply;
        public string AttackName = "Attack";
    }
}
