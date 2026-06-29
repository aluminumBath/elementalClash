using UnityEngine;

namespace Elementborn.Game
{
    public static class CombatDamageResolver
    {
        public static CombatHitResult Resolve(GameObject attacker, GameObject target, CombatHitContext context)
        {
            float damage = Mathf.Max(0f, context != null ? context.BaseDamage : 0f);
            bool crit = false;
            bool weak = false;
            bool resisted = false;

            if (context == null)
            {
                return new CombatHitResult(0f, false, false, false, "No context.");
            }

            // outgoing equipment bonuses (player/global for now)
            if (context.UseEquipmentBonuses)
            {
                damage += PlayerEquipmentTracker.GetFlatBonus(GearStatType.AttackPower);
                damage = PlayerEquipmentTracker.ApplyBonuses(GearStatType.AttackPower, damage);
                damage += GetElementFlatBonus(context.Element);
                damage = ApplyElementPercentBonus(context.Element, damage);
            }

            // statuses on attacker can influence attack power
            var attackerStatus = attacker != null ? attacker.GetComponent<StatusEffectController>() : null;
            if (attackerStatus != null)
            {
                damage *= attackerStatus.GetAttackMultiplier();
            }

            // crit
            if (Random.value <= Mathf.Clamp01(context.CritChance))
            {
                crit = true;
                damage *= Mathf.Max(1f, context.CritMultiplier);
            }

            // boat/creature hooks
            if (attacker != null)
            {
                if (context.OriginType == AttackOriginType.Boat)
                {
                    var hook = attacker.GetComponent<BoatCombatHook>();
                    if (hook != null) damage *= hook.DamageMultiplier;
                    damage += PlayerEquipmentTracker.GetFlatBonus(GearStatType.BoatHandling) * 0.25f;
                }
                else if (context.OriginType == AttackOriginType.Creature)
                {
                    var hook = attacker.GetComponent<CreatureCombatHook>();
                    if (hook != null) damage *= hook.DamageMultiplier;
                    damage += PlayerEquipmentTracker.GetPercentBonus(GearStatType.CreatureBondGain) * 0.05f;
                }
            }

            // target statuses and synergies
            var status = target != null ? target.GetComponent<StatusEffectController>() : null;
            if (status != null)
            {
                damage /= Mathf.Max(0.01f, status.GetDefenseMultiplier());
                if (context.Element == AbilityElementType.Fire && status.HasStatus(StatusEffectType.Wet))
                {
                    weak = true;
                    damage *= 1.2f;
                    status.Remove(StatusEffectType.Wet);
                }
                if (context.Element == AbilityElementType.Lightning && status.HasStatus(StatusEffectType.Wet))
                {
                    weak = true;
                    damage *= 1.35f;
                }
                if (context.Element == AbilityElementType.Fire && status.HasStatus(StatusEffectType.Chilled))
                {
                    weak = true;
                    damage *= 1.25f;
                    status.Remove(StatusEffectType.Chilled);
                }
                if (context.Element == AbilityElementType.Ice && status.HasStatus(StatusEffectType.Wet))
                {
                    weak = true;
                    damage *= 1.15f;
                }
            }

            // resistances
            var resist = target != null ? target.GetComponent<CombatResistanceProfile>() : null;
            if (resist != null)
            {
                damage = Mathf.Max(0f, damage - resist.FlatDefense);
                float pct = resist.GetPercent(context.Element);
                if (pct > 0.001f) resisted = true;
                if (pct < -0.001f) weak = true;
                damage *= 1f - pct / 100f;
            }

            // player defense gear if player is target
            if (target != null && target.CompareTag("Player"))
            {
                damage -= PlayerEquipmentTracker.GetFlatBonus(GearStatType.Defense);
                damage /= Mathf.Max(0.25f, 1f + PlayerEquipmentTracker.GetPercentBonus(GearStatType.Defense) / 100f);
            }

            damage = Mathf.Max(0f, damage);
            return new CombatHitResult(damage, crit, weak, resisted, weak ? "Weakness" : resisted ? "Resisted" : string.Empty);
        }

        private static float GetElementFlatBonus(AbilityElementType element)
        {
            return element switch
            {
                AbilityElementType.Fire => PlayerEquipmentTracker.GetFlatBonus(GearStatType.FirePower),
                AbilityElementType.Water => PlayerEquipmentTracker.GetFlatBonus(GearStatType.WaterPower),
                AbilityElementType.Earth => PlayerEquipmentTracker.GetFlatBonus(GearStatType.EarthPower),
                AbilityElementType.Air => PlayerEquipmentTracker.GetFlatBonus(GearStatType.AirPower),
                AbilityElementType.Ice => PlayerEquipmentTracker.GetFlatBonus(GearStatType.IcePower),
                AbilityElementType.Plant => PlayerEquipmentTracker.GetFlatBonus(GearStatType.PlantPower),
                AbilityElementType.Metal => PlayerEquipmentTracker.GetFlatBonus(GearStatType.MetalPower),
                AbilityElementType.Blood => PlayerEquipmentTracker.GetFlatBonus(GearStatType.BloodPower),
                AbilityElementType.Steam => PlayerEquipmentTracker.GetFlatBonus(GearStatType.SteamPower),
                AbilityElementType.Lightning => PlayerEquipmentTracker.GetFlatBonus(GearStatType.LightningPower),
                AbilityElementType.Shadow => PlayerEquipmentTracker.GetFlatBonus(GearStatType.ShadowPower),
                AbilityElementType.Light => PlayerEquipmentTracker.GetFlatBonus(GearStatType.LightPower),
                _ => PlayerEquipmentTracker.GetFlatBonus(GearStatType.ElementPower)
            };
        }

        private static float ApplyElementPercentBonus(AbilityElementType element, float damage)
        {
            float pct = element switch
            {
                AbilityElementType.Fire => PlayerEquipmentTracker.GetPercentBonus(GearStatType.FirePower),
                AbilityElementType.Water => PlayerEquipmentTracker.GetPercentBonus(GearStatType.WaterPower),
                AbilityElementType.Earth => PlayerEquipmentTracker.GetPercentBonus(GearStatType.EarthPower),
                AbilityElementType.Air => PlayerEquipmentTracker.GetPercentBonus(GearStatType.AirPower),
                AbilityElementType.Ice => PlayerEquipmentTracker.GetPercentBonus(GearStatType.IcePower),
                AbilityElementType.Plant => PlayerEquipmentTracker.GetPercentBonus(GearStatType.PlantPower),
                AbilityElementType.Metal => PlayerEquipmentTracker.GetPercentBonus(GearStatType.MetalPower),
                AbilityElementType.Blood => PlayerEquipmentTracker.GetPercentBonus(GearStatType.BloodPower),
                AbilityElementType.Steam => PlayerEquipmentTracker.GetPercentBonus(GearStatType.SteamPower),
                AbilityElementType.Lightning => PlayerEquipmentTracker.GetPercentBonus(GearStatType.LightningPower),
                AbilityElementType.Shadow => PlayerEquipmentTracker.GetPercentBonus(GearStatType.ShadowPower),
                AbilityElementType.Light => PlayerEquipmentTracker.GetPercentBonus(GearStatType.LightPower),
                _ => PlayerEquipmentTracker.GetPercentBonus(GearStatType.ElementPower)
            };
            return damage * (1f + pct / 100f);
        }
    }
}
