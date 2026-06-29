using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class MeleeCombatHitbox : MonoBehaviour
    {
        [SerializeField] private CombatAttackDefinition attackDefinition;
        [SerializeField] private Transform origin;
        [SerializeField] private float radius = 1.5f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private bool excludeSelfRoot = true;
        [SerializeField] private bool drawGizmos = true;

        public void PerformAttack()
        {
            Vector3 center = origin != null ? origin.position : transform.position;
            Collider[] hits = Physics.OverlapSphere(center, radius, targetMask, QueryTriggerInteraction.Ignore);
            var seen = new HashSet<GameObject>();
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                GameObject target = hit.attachedRigidbody != null ? hit.attachedRigidbody.gameObject : hit.gameObject;
                if (excludeSelfRoot && target.transform.root == transform.root) continue;
                if (!seen.Add(target)) continue;

                var context = BuildContext();
                context.Source = gameObject;
                CombatDamageUtility.ApplyHit(target, context);
            }
        }

        private CombatHitContext BuildContext()
        {
            return new CombatHitContext
            {
                Source = gameObject,
                AttackDefinition = attackDefinition,
                BaseDamage = attackDefinition != null ? attackDefinition.BaseDamage : 10f,
                Element = attackDefinition != null ? attackDefinition.Element : AbilityElementType.Neutral,
                CritChance = attackDefinition != null ? attackDefinition.CritChance : 0.05f,
                CritMultiplier = attackDefinition != null ? attackDefinition.CritMultiplier : 1.5f,
                KnockbackForce = attackDefinition != null ? attackDefinition.KnockbackForce : 0f,
                UseEquipmentBonuses = attackDefinition == null || attackDefinition.UseEquipmentBonuses,
                OriginType = attackDefinition != null ? attackDefinition.OriginType : AttackOriginType.OnFoot,
                StatusToApply = attackDefinition != null ? attackDefinition.StatusToApply : null,
                AttackName = attackDefinition != null ? attackDefinition.DisplayName : "Melee Attack"
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            Gizmos.color = Color.yellow;
            Vector3 center = origin != null ? origin.position : transform.position;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
