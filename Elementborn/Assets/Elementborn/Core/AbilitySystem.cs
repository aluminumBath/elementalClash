using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Turns a <see cref="ChannelingIntent"/> into a concrete <see cref="AbilityOutcome"/> using
    /// the player's loadout. Fully platform-agnostic, so it is unit-tested without any Unity
    /// scene setup. All four elements and the Magmacraft / Lightning / Ice / Oreshaping / SanguineGrip / Flight
    /// variants are implemented; weapon combat is a seam.
    /// </summary>
    public sealed class AbilitySystem
    {
        // Fire + Lightning
        public const float FireBaseDamage = 15f;
        public const float FireChargeBonus = 35f;
        public const float FireProjectileSpeed = 18f;
        public const float LavaDamageMultiplier = 1.5f;
        public const float LightningBaseDamage = 40f;
        public const float LightningChargeBonus = 40f;
        public const float LightningSpeed = 55f;
        public const float LightningStunDuration = 0.75f;

        // Water + Ice + Sanguine Grip
        public const float WaterBaseDamage = 10f;
        public const float WaterChargeBonus = 20f;
        public const float WaterProjectileSpeed = 22f;
        public const float IceBaseDamage = 18f;
        public const float IceChargeBonus = 22f;
        public const float IceSpeed = 16f;
        public const float IceSlowMagnitude = 0.6f;
        public const float IceSlowDuration = 2.5f;
        public const float BloodDamage = 5f;
        public const float BloodControlDuration = 2f;
        public const float BloodFlingForce = 12f;

        // Earth + Metal
        public const float EarthBaseDamage = 20f;
        public const float EarthChargeBonus = 30f;
        public const float EarthProjectileSpeed = 14f;
        public const float MetalDamageMultiplier = 1.5f;
        public const float BoulderDamageMultiplier = 1.4f;
        public const float BoulderKnockback = 8f;

        // Air + Flight glide
        public const float AirBaseDamage = 6f;
        public const float AirChargeBonus = 12f;
        public const float AirProjectileSpeed = 26f;
        public const float AirKnockback = 6f;
        public const float AirScytheDamageMultiplier = 2.5f;
        public const float AirScytheKnockback = 10f;
        public const float AirDashSpeed = 12f;
        public const float AirGlideSpeed = 20f;

        // Heavy (committed power attacks) + Sweep (wide crowd-control arcs) — the extra VR gesture moves.
        public const float HeavyChargeBonus = 30f;
        public const float HeavySpeedMul = 0.7f;
        public const float HeavyKnockback = 10f;
        public const float FireHeavyDamage = 34f;
        public const float WaterHeavyDamage = 30f;
        public const float EarthHeavyDamage = 40f;
        public const float AirHeavyDamage = 8f;
        public const float SweepChargeBonus = 18f;
        public const float SweepKnockback = 7f;
        public const float FireSweepDamage = 16f;
        public const float WaterSweepDamage = 8f;
        public const float EarthSweepDamage = 18f;
        public const float AirSweepDamage = 6f;
        // Sweep is now a wide multi-target arc (OutcomeKind.Sweep); each element carries a distinct rider.
        public const float FireSweepKnockback = 3f;   // Fire: light shove, leaves a burn
        public const float WaterSweepKnockback = 8f;  // Water: hard shove, wets footing (slow)
        public const float EarthSweepKnockback = 7f;  // Earth: shove + brief stagger (control)
        public const float AirSweepKnockback = 12f;   // Air: pure displacement, biggest knockback
        public const float SweepBurnDps = 4f;
        public const float SweepBurnDuration = 2f;
        public const float SweepWetSlowDuration = 1.5f;
        public const float EarthSweepStaggerDuration = 0.5f;

        private readonly ChannelerLoadout _loadout;

        public AbilitySystem(ChannelerLoadout loadout)
        {
            _loadout = loadout ?? throw new System.ArgumentNullException(nameof(loadout));
        }

        public AbilityOutcome Resolve(ChannelingIntent intent)
        {
            if (intent.Type == IntentType.None) return AbilityOutcome.Empty;
            if (!_loadout.IsChanneler) return ResolveWeapon(intent);

            Element active = _loadout.Elements[0];
            return active switch
            {
                Element.Fire => ResolveFire(intent),
                Element.Water => ResolveWater(intent),
                Element.Earth => ResolveEarth(intent),
                Element.Air => ResolveAir(intent),
                _ => AbilityOutcome.Empty
            };
        }

        private AbilityOutcome ResolveWeapon(ChannelingIntent intent) =>
            Weapons.Resolve(new WeaponInstance(_loadout.Weapon, WeaponMaterial.Metal), intent);

        private AbilityOutcome ResolveFire(ChannelingIntent intent)
        {
            bool hasLava = _loadout.HasSubArt(SubArt.Magmacraft);
            switch (intent.Type)
            {
                case IntentType.PrimaryCast:
                {
                    float damage = FireBaseDamage + FireChargeBonus * intent.Charge;
                    var variant = AbilityVariant.Standard;
                    var status = StatusEffect.None;
                    if (hasLava)
                    {
                        damage *= LavaDamageMultiplier;
                        variant = AbilityVariant.Magmacraft;
                        status = new StatusEffect(StatusKind.Burn, 6f, 3f);
                    }
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Fire, variant,
                        intent.Direction, damage, FireProjectileSpeed, status);
                }
                case IntentType.SecondaryCast: // Lightning
                {
                    float damage = LightningBaseDamage + LightningChargeBonus * intent.Charge;
                    var status = new StatusEffect(StatusKind.Stun, 1f, LightningStunDuration);
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Fire, AbilityVariant.Lightning,
                        intent.Direction, damage, LightningSpeed, status);
                }
                case IntentType.Defend:
                    return new AbilityOutcome(OutcomeKind.Barrier, Element.Fire, AbilityVariant.Standard,
                        intent.Direction, 0f, 0f, StatusEffect.None);
                case IntentType.Heavy: // overhead meteor lob
                {
                    float damage = FireHeavyDamage + HeavyChargeBonus * intent.Charge;
                    var variant = AbilityVariant.Standard;
                    var status = StatusEffect.None;
                    if (hasLava)
                    {
                        damage *= LavaDamageMultiplier;
                        variant = AbilityVariant.Magmacraft;
                        status = new StatusEffect(StatusKind.Burn, 6f, 3f);
                    }
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Fire, variant,
                        intent.Direction, damage, FireProjectileSpeed * HeavySpeedMul, status, HeavyKnockback);
                }
                case IntentType.Sweep: // fan of flame — moderate, leaves a short burn
                {
                    float damage = FireSweepDamage + SweepChargeBonus * intent.Charge;
                    var status = new StatusEffect(StatusKind.Burn, SweepBurnDps, SweepBurnDuration);
                    return new AbilityOutcome(OutcomeKind.Sweep, Element.Fire, AbilityVariant.Standard,
                        intent.Direction, damage, 0f, status, FireSweepKnockback);
                }
                default:
                    return AbilityOutcome.Empty;
            }
        }

        private AbilityOutcome ResolveWater(ChannelingIntent intent)
        {
            bool hasBlood = _loadout.HasSubArt(SubArt.SanguineGrip);
            switch (intent.Type)
            {
                case IntentType.PrimaryCast: // water jet
                {
                    float damage = WaterBaseDamage + WaterChargeBonus * intent.Charge;
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Water, AbilityVariant.Standard,
                        intent.Direction, damage, WaterProjectileSpeed, StatusEffect.None);
                }
                case IntentType.SecondaryCast:
                {
                    if (hasBlood) // Sanguine Grip: immobilize the target and fling it
                    {
                        var control = new StatusEffect(StatusKind.Control, 1f, BloodControlDuration);
                        return new AbilityOutcome(OutcomeKind.Control, Element.Water, AbilityVariant.SanguineGrip,
                            intent.Direction, BloodDamage, 0f, control, BloodFlingForce);
                    }
                    // Ice shard: heavier, slows on hit
                    float damage = IceBaseDamage + IceChargeBonus * intent.Charge;
                    var status = new StatusEffect(StatusKind.Slow, IceSlowMagnitude, IceSlowDuration);
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Water, AbilityVariant.Ice,
                        intent.Direction, damage, IceSpeed, status);
                }
                case IntentType.Defend:
                    return new AbilityOutcome(OutcomeKind.Barrier, Element.Water, AbilityVariant.Ice,
                        intent.Direction, 0f, 0f, StatusEffect.None);
                case IntentType.Heavy: // rising ice geyser, slows
                {
                    float damage = WaterHeavyDamage + HeavyChargeBonus * intent.Charge;
                    var status = new StatusEffect(StatusKind.Slow, IceSlowMagnitude, IceSlowDuration);
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Water, AbilityVariant.Ice,
                        intent.Direction, damage, IceSpeed * HeavySpeedMul, status, HeavyKnockback);
                }
                case IntentType.Sweep: // wide wave — shoves hard and wets footing (slow)
                {
                    float damage = WaterSweepDamage + SweepChargeBonus * intent.Charge;
                    var status = new StatusEffect(StatusKind.Slow, IceSlowMagnitude, SweepWetSlowDuration);
                    return new AbilityOutcome(OutcomeKind.Sweep, Element.Water, AbilityVariant.Standard,
                        intent.Direction, damage, 0f, status, WaterSweepKnockback);
                }
                default:
                    return AbilityOutcome.Empty;
            }
        }

        private AbilityOutcome ResolveEarth(ChannelingIntent intent)
        {
            bool hasMetal = _loadout.HasSubArt(SubArt.Oreshaping);
            switch (intent.Type)
            {
                case IntentType.PrimaryCast: // rock hurl (metal shard if sub-art)
                {
                    float damage = EarthBaseDamage + EarthChargeBonus * intent.Charge;
                    var variant = AbilityVariant.Standard;
                    if (hasMetal)
                    {
                        damage *= MetalDamageMultiplier;
                        variant = AbilityVariant.Oreshaping;
                    }
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Earth, variant,
                        intent.Direction, damage, EarthProjectileSpeed, StatusEffect.None);
                }
                case IntentType.SecondaryCast: // heavy boulder, knocks back
                {
                    float damage = EarthBaseDamage * BoulderDamageMultiplier + EarthChargeBonus * intent.Charge;
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Earth, AbilityVariant.Standard,
                        intent.Direction, damage, EarthProjectileSpeed * 0.7f, StatusEffect.None, BoulderKnockback);
                }
                case IntentType.Defend:
                    return new AbilityOutcome(OutcomeKind.Barrier, Element.Earth, AbilityVariant.Standard,
                        intent.Direction, 0f, 0f, StatusEffect.None);
                case IntentType.Heavy: // driving ground slam (metal shard if sub-art)
                {
                    float damage = EarthHeavyDamage + HeavyChargeBonus * intent.Charge;
                    var variant = AbilityVariant.Standard;
                    if (hasMetal) { damage *= MetalDamageMultiplier; variant = AbilityVariant.Oreshaping; }
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Earth, variant,
                        intent.Direction, damage, EarthProjectileSpeed * HeavySpeedMul, StatusEffect.None, HeavyKnockback);
                }
                case IntentType.Sweep: // low rock wall — shoves and briefly staggers (control)
                {
                    float damage = EarthSweepDamage + SweepChargeBonus * intent.Charge;
                    var status = new StatusEffect(StatusKind.Control, 1f, EarthSweepStaggerDuration);
                    return new AbilityOutcome(OutcomeKind.Sweep, Element.Earth, AbilityVariant.Standard,
                        intent.Direction, damage, 0f, status, EarthSweepKnockback);
                }
                default:
                    return AbilityOutcome.Empty;
            }
        }

        private AbilityOutcome ResolveAir(ChannelingIntent intent)
        {
            switch (intent.Type)
            {
                case IntentType.PrimaryCast: // air blast: fast, low damage, knocks back
                {
                    float damage = AirBaseDamage + AirChargeBonus * intent.Charge;
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Air, AbilityVariant.Standard,
                        intent.Direction, damage, AirProjectileSpeed, StatusEffect.None, AirKnockback);
                }
                case IntentType.SecondaryCast: // air scythe
                {
                    float damage = AirBaseDamage * AirScytheDamageMultiplier + AirChargeBonus * intent.Charge;
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Air, AbilityVariant.Standard,
                        intent.Direction, damage, AirProjectileSpeed, StatusEffect.None, AirScytheKnockback);
                }
                case IntentType.Dash: // base = quick dash; Flight sub-art = longer, faster glide
                {
                    bool hasFlight = _loadout.HasSubArt(SubArt.Flight);
                    float speed = hasFlight ? AirGlideSpeed : AirDashSpeed;
                    var variant = hasFlight ? AbilityVariant.Flight : AbilityVariant.Standard;
                    return new AbilityOutcome(OutcomeKind.Movement, Element.Air, variant,
                        intent.Direction, 0f, speed, StatusEffect.None);
                }
                case IntentType.Defend:
                    return new AbilityOutcome(OutcomeKind.Barrier, Element.Air, AbilityVariant.Standard,
                        intent.Direction, 0f, 0f, StatusEffect.None);
                case IntentType.Heavy: // updraft launch: low damage, big knock-up
                {
                    float damage = AirHeavyDamage + HeavyChargeBonus * intent.Charge;
                    return new AbilityOutcome(OutcomeKind.Projectile, Element.Air, AbilityVariant.Standard,
                        intent.Direction, damage, AirProjectileSpeed, StatusEffect.None, HeavyKnockback);
                }
                case IntentType.Sweep: // downburst gust — low damage, biggest knockback, pure displacement
                {
                    float damage = AirSweepDamage + SweepChargeBonus * intent.Charge;
                    return new AbilityOutcome(OutcomeKind.Sweep, Element.Air, AbilityVariant.Standard,
                        intent.Direction, damage, 0f, StatusEffect.None, AirSweepKnockback);
                }
                default:
                    return AbilityOutcome.Empty;
            }
        }
    }
}
