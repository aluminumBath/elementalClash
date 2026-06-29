using UnityEngine;

namespace Elementborn.Game
{
    public sealed class EnemyRangedAttackDriver : MonoBehaviour
    {
        [SerializeField] private CombatAttackDefinition attackDefinition;
        [SerializeField] private ProjectileCombatEmitter projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 12f;

        private float rangedReadyAt;

        public bool CanAttack(float cooldown)
        {
            return Time.unscaledTime >= rangedReadyAt + Mathf.Max(0f, cooldown);
        }

        public void Attack(Transform target, float cooldown)
        {
            rangedReadyAt = Time.unscaledTime;

            if (target == null)
            {
                return;
            }

            Transform origin = firePoint != null ? firePoint : transform;

            if (projectilePrefab != null)
            {
                ProjectileCombatEmitter projectile = Instantiate(projectilePrefab, origin.position, Quaternion.LookRotation((target.position - origin.position).normalized));
                projectile.SetOwner(gameObject);
                return;
            }

            var context = new CombatHitContext
            {
                Source = gameObject,
                AttackDefinition = attackDefinition,
                BaseDamage = attackDefinition != null ? attackDefinition.BaseDamage : 8f,
                Element = attackDefinition != null ? attackDefinition.Element : AbilityElementType.Neutral,
                CritChance = attackDefinition != null ? attackDefinition.CritChance : 0.03f,
                CritMultiplier = attackDefinition != null ? attackDefinition.CritMultiplier : 1.4f,
                KnockbackForce = attackDefinition != null ? attackDefinition.KnockbackForce : 0f,
                UseEquipmentBonuses = false,
                OriginType = AttackOriginType.OnFoot,
                StatusToApply = attackDefinition != null ? attackDefinition.StatusToApply : null,
                AttackName = attackDefinition != null ? attackDefinition.DisplayName : "Enemy Ranged"
            };

            CombatDamageUtility.ApplyHit(target.gameObject, context);
        }

        public void SetAttackDefinition(CombatAttackDefinition definition)
        {
            attackDefinition = definition;
        }
    }
}
