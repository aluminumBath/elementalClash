using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyMeleeAttackDriver : MonoBehaviour
    {
        [SerializeField] private CombatAttackDefinition attackDefinition;
        [SerializeField] private MeleeCombatHitbox hitbox;
        [SerializeField] private float windupSeconds = 0.25f;

        private float attackReadyAt;

        public bool CanAttack(float cooldown)
        {
            return Time.unscaledTime >= attackReadyAt + Mathf.Max(0f, cooldown);
        }

        public void Attack(GameObject target, float cooldown)
        {
            attackReadyAt = Time.unscaledTime;

            if (hitbox != null)
            {
                hitbox.PerformAttack();
                return;
            }

            if (target == null)
            {
                return;
            }

            var context = new CombatHitContext
            {
                Source = gameObject,
                AttackDefinition = attackDefinition,
                BaseDamage = attackDefinition != null ? attackDefinition.BaseDamage : 10f,
                Element = attackDefinition != null ? attackDefinition.Element : AbilityElementType.Neutral,
                CritChance = attackDefinition != null ? attackDefinition.CritChance : 0.05f,
                CritMultiplier = attackDefinition != null ? attackDefinition.CritMultiplier : 1.5f,
                KnockbackForce = attackDefinition != null ? attackDefinition.KnockbackForce : 1.5f,
                UseEquipmentBonuses = false,
                OriginType = AttackOriginType.OnFoot,
                StatusToApply = attackDefinition != null ? attackDefinition.StatusToApply : null,
                AttackName = attackDefinition != null ? attackDefinition.DisplayName : "Enemy Melee"
            };

            CombatDamageUtility.ApplyHit(target, context);
        }

        public void SetAttackDefinition(CombatAttackDefinition definition)
        {
            attackDefinition = definition;
        }
    }
}
