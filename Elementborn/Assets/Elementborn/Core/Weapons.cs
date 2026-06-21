using UnityEngine;

namespace Elementborn.Core
{
    public enum WeaponType { None, Hammer, Sword, LongBow, Shield, Dagger, Sai }
    public enum WeaponMaterial { Wood, Metal, Ice }

    /// <summary>An equipped weapon: a type plus the material it's made of.</summary>
    public readonly struct WeaponInstance
    {
        public WeaponType Type { get; }
        public WeaponMaterial Material { get; }
        public WeaponInstance(WeaponType type, WeaponMaterial material) { Type = type; Material = material; }
    }

    /// <summary>
    /// Weapon stats and combat resolution for players without an element. Pure logic, so it is
    /// unit-tested directly. Material sets a damage tier and an elemental weakness: wood breaks
    /// to fire, metal to oreshaping, ice to water channeling — and ice also slows on hit.
    /// </summary>
    public static class Weapons
    {
        public const float ArrowSpeed = 30f;
        public const float HeavyMultiplier = 1.5f;
        public const float IceSlowMagnitude = 0.4f;
        public const float IceSlowDuration = 1.5f;

        public static bool IsRanged(WeaponType type) => type == WeaponType.LongBow;

        public static float BaseDamage(WeaponType type) => type switch
        {
            WeaponType.Hammer => 25f,
            WeaponType.Sword => 18f,
            WeaponType.LongBow => 16f,
            WeaponType.Dagger => 10f,
            WeaponType.Sai => 12f,
            WeaponType.Shield => 4f,
            _ => 0f
        };

        public static float KnockbackFor(WeaponType type) => type switch
        {
            WeaponType.Hammer => 7f,
            WeaponType.Shield => 4f, // shield bash
            WeaponType.Sai => 2f,
            _ => 0f
        };

        public static float MaterialMultiplier(WeaponMaterial material) => material switch
        {
            WeaponMaterial.Wood => 0.7f,  // weak
            WeaponMaterial.Metal => 1.3f, // strong
            WeaponMaterial.Ice => 1.0f,   // mid, slows on hit
            _ => 1.0f
        };

        /// <summary>True when an incoming hit matches the material's weakness and shatters the weapon.</summary>
        public static bool IsBrokenBy(WeaponInstance weapon, DamageInfo damage) => weapon.Material switch
        {
            WeaponMaterial.Wood => damage.Source == Element.Fire,
            WeaponMaterial.Metal => damage.Variant == AbilityVariant.Oreshaping, // oreshaping specifically
            WeaponMaterial.Ice => damage.Source == Element.Water,
            _ => false
        };

        public static AbilityOutcome Resolve(WeaponInstance weapon, ChannelingIntent intent)
        {
            float damage = BaseDamage(weapon.Type) * MaterialMultiplier(weapon.Material);
            float knockback = KnockbackFor(weapon.Type);
            StatusEffect status = weapon.Material == WeaponMaterial.Ice
                ? new StatusEffect(StatusKind.Slow, IceSlowMagnitude, IceSlowDuration)
                : StatusEffect.None;

            // Weapons are physical (non-elemental); tag as Earth for damage metadata only.
            const Element physical = Element.Earth;

            switch (intent.Type)
            {
                case IntentType.PrimaryCast:
                    return IsRanged(weapon.Type)
                        ? new AbilityOutcome(OutcomeKind.Projectile, physical, AbilityVariant.Standard,
                            intent.Direction, damage, ArrowSpeed, status, knockback)
                        : new AbilityOutcome(OutcomeKind.Melee, physical, AbilityVariant.Standard,
                            intent.Direction, damage, 0f, status, knockback);

                case IntentType.SecondaryCast: // heavy attack / charged shot
                {
                    float heavy = damage * HeavyMultiplier;
                    return IsRanged(weapon.Type)
                        ? new AbilityOutcome(OutcomeKind.Projectile, physical, AbilityVariant.Standard,
                            intent.Direction, heavy, ArrowSpeed, status, knockback)
                        : new AbilityOutcome(OutcomeKind.Melee, physical, AbilityVariant.Standard,
                            intent.Direction, heavy, 0f, status, knockback);
                }

                case IntentType.Defend: // block / parry (shield is the strongest)
                    return new AbilityOutcome(OutcomeKind.Barrier, physical, AbilityVariant.Standard,
                        intent.Direction, 0f, 0f, StatusEffect.None);

                default:
                    return AbilityOutcome.Empty;
            }
        }
    }
}
