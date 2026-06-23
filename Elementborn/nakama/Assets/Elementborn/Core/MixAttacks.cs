using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// A creature's signature attack that blends two elements. The <see cref="Primary"/> element drives the
    /// outcome's look and damage type; the <see cref="Secondary"/> is the blend's flavour (baked into the
    /// damage/status here). <see cref="ToOutcome"/> turns it into a standard <see cref="AbilityOutcome"/>, so it
    /// rides the same projectile/presentation pipeline as everything else.
    /// </summary>
    public readonly struct MixAttack
    {
        public readonly string Name;
        public readonly Element Primary;
        public readonly Element Secondary;
        public readonly float Damage;
        public readonly float Range;
        public readonly float Knockback;
        public readonly StatusEffect Status;

        public MixAttack(string name, Element primary, Element secondary, float damage, float range,
            float knockback, StatusEffect status)
        {
            Name = name;
            Primary = primary;
            Secondary = secondary;
            Damage = damage;
            Range = range;
            Knockback = knockback;
            Status = status;
        }

        public bool IsNone => string.IsNullOrEmpty(Name);
        public static MixAttack None => default;

        private const float ProjectileSpeed = 16f;

        public AbilityOutcome ToOutcome(Vector3 direction) => new AbilityOutcome(
            OutcomeKind.Projectile, Primary, AbilityVariant.Standard, direction, Damage, ProjectileSpeed, Status, Knockback);
    }

    /// <summary>
    /// The mix attack each creature can unleash. The exotic apex creatures and our rocs and thunderbirds each
    /// have one; ordinary wildlife returns <see cref="MixAttack.None"/>. Tweak names/elements/damage here.
    /// </summary>
    public static class CreatureMixAttacks
    {
        public static MixAttack For(CreatureKind kind)
        {
            switch (kind)
            {
                case CreatureKind.Ridgewing:   // Air + Earth — grit on a cliff-gale
                    return new MixAttack("Gritgale", Element.Air, Element.Earth, 20f, 12f, 6f, StatusEffect.None);
                case CreatureKind.Glidewisp:   // Air + Water — a clinging damp gust
                    return new MixAttack("Mistveil", Element.Air, Element.Water, 12f, 10f, 3f, new StatusEffect(StatusKind.Slow, 0.3f, 2f));
                case CreatureKind.Skytyrant:   // Fire + Air — a searing firestorm dive
                    return new MixAttack("Cinderstorm", Element.Fire, Element.Air, 38f, 14f, 10f, new StatusEffect(StatusKind.Burn, 6f, 3f));
                case CreatureKind.Goldkoi:     // Water + Earth — a silt-laden surge
                    return new MixAttack("Siltsurge", Element.Water, Element.Earth, 18f, 11f, 5f, new StatusEffect(StatusKind.Slow, 0.4f, 2f));
                case CreatureKind.Direstalker: // Earth + Fire — a ground-cracking, ember-laced pounce
                    return new MixAttack("Emberquake", Element.Earth, Element.Fire, 34f, 6f, 8f, new StatusEffect(StatusKind.Burn, 5f, 3f));
                case CreatureKind.Skimfin:     // Water + Air — a slicing spray as it skims
                    return new MixAttack("Spraylash", Element.Water, Element.Air, 20f, 12f, 7f, StatusEffect.None);
                case CreatureKind.Gillcloak:   // Water + Fire — a scalding steam veil
                    return new MixAttack("Scaldveil", Element.Water, Element.Fire, 22f, 8f, 4f, new StatusEffect(StatusKind.Burn, 4f, 2f));
                case CreatureKind.Tidewarden:  // Water + Earth — a colossal tidal surge
                    return new MixAttack("Deepsurge", Element.Water, Element.Earth, 40f, 14f, 12f, new StatusEffect(StatusKind.Slow, 0.5f, 2f));
                case CreatureKind.Roc:         // Earth + Air — a storm of flung boulders
                    return new MixAttack("Boulderstorm", Element.Earth, Element.Air, 22f, 12f, 9f, StatusEffect.None);
                case CreatureKind.Thunderbird: // Fire + Air — a lightning-charged squall
                    return new MixAttack("Thunderhead", Element.Fire, Element.Air, 26f, 14f, 6f, new StatusEffect(StatusKind.Stun, 1f, 0.8f));
                default:
                    return MixAttack.None;
            }
        }
    }
}
