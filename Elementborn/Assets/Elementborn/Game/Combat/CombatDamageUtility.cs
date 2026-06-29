using UnityEngine;

namespace Elementborn.Game
{
    public static class CombatDamageUtility
    {
        public static CombatHitResult ApplyHit(GameObject target, CombatHitContext context)
        {
            if (target == null || context == null)
            {
                return new CombatHitResult(0f, false, false, false, "Missing target or context.");
            }

            var result = CombatDamageResolver.Resolve(context.Source, target, context);
            float finalDamage = result.FinalDamage;

            var defense = target.GetComponent<CombatDefenseController>();
            if (defense != null)
            {
                finalDamage = defense.ModifyIncomingDamage(finalDamage, context.Element, context.Source, out bool perfectBlocked, out bool dodged);
                if (dodged)
                {
                    DamageNumberSpawner.Spawn(target.transform.position, 0f, context.Element, false);
                    HitFeedbackService.Play(target, target.transform.position, context.Element, HitFeedbackType.Dodge, false);
                    return new CombatHitResult(0f, result.Critical, result.Weakness, result.Resisted, "Dodged");
                }

                if (perfectBlocked)
                {
                    result = new CombatHitResult(finalDamage, result.Critical, result.Weakness, result.Resisted, "Perfect Block");
                }
            }

            ApplyRawDamage(target, finalDamage, context.Element, context.Source, context.AttackName, context.OriginType == AttackOriginType.Boat);

            if (finalDamage > 0f && context.StatusToApply != null)
            {
                var status = target.GetComponent<StatusEffectController>();
                if (status == null) status = target.AddComponent<StatusEffectController>();
                status.Apply(context.StatusToApply);
            }

            var rb = target.GetComponent<Rigidbody>();
            if (finalDamage > 0f && rb != null && context.KnockbackForce > 0f && context.Source != null)
            {
                Vector3 dir = (target.transform.position - context.Source.transform.position).normalized;
                rb.AddForce((dir + Vector3.up * 0.25f) * context.KnockbackForce, ForceMode.Impulse);
            }

            DamageNumberSpawner.Spawn(target.transform.position, finalDamage, context.Element, result.Critical);
            HitFeedbackType feedbackType = result.Critical ? HitFeedbackType.CriticalHit : HitFeedbackType.NormalHit;
            if (result.Notes == "Perfect Block")
            {
                feedbackType = HitFeedbackType.PerfectBlock;
            }
            else if (finalDamage < result.FinalDamage)
            {
                feedbackType = HitFeedbackType.Block;
            }

            HitFeedbackService.Play(target, target.transform.position, context.Element, feedbackType, result.Critical);
            return new CombatHitResult(finalDamage, result.Critical, result.Weakness, result.Resisted, result.Notes);
        }

        public static void ApplyRawDamage(GameObject target, float amount, AbilityElementType element, GameObject source, string attackName, bool boatHit)
        {
            if (target == null || amount <= 0f) return;

            var simple = target.GetComponent<SimpleCombatHealth>();
            if (simple != null)
            {
                simple.ApplyDamage(amount);
                return;
            }

            // best-effort compatibility with projects that already have their own health system
            target.SendMessage("ApplyDamage", amount, SendMessageOptions.DontRequireReceiver);
            target.SendMessage("TakeDamage", amount, SendMessageOptions.DontRequireReceiver);
            target.SendMessage("ReceiveDamage", amount, SendMessageOptions.DontRequireReceiver);
        }
    }
}
