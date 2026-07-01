namespace Elementborn.Core
{
    /// <summary>
    /// What damage a creature shrugs off. Lets us express "immune to water but not ice" (immune to the
    /// Water element except the Ice variant) and "immune to ice but not water" (immune to the Ice variant
    /// only). Pure + unit-tested.
    /// </summary>
    public readonly struct DamageImmunity
    {
        public readonly Element? ImmuneElement;        // immune to this element...
        public readonly AbilityVariant? ExceptVariant; // ...unless the hit carries this variant (the weakness)
        public readonly AbilityVariant? ImmuneVariant;  // also fully immune to this specific variant

        public DamageImmunity(Element? immuneElement, AbilityVariant? exceptVariant, AbilityVariant? immuneVariant)
        {
            ImmuneElement = immuneElement;
            ExceptVariant = exceptVariant;
            ImmuneVariant = immuneVariant;
        }

        public static DamageImmunity None => new DamageImmunity(null, null, null);

        public bool Blocks(DamageInfo d)
        {
            if (ImmuneVariant.HasValue && d.Variant == ImmuneVariant.Value) return true;
            if (ImmuneElement.HasValue && d.Source == ImmuneElement.Value)
            {
                if (ExceptVariant.HasValue && d.Variant == ExceptVariant.Value) return false; // weakness pierces
                return true;
            }
            return false;
        }
    }

    /// <summary>Combat identity of a rare companion: how it attacks, what it shrugs off, and its tricks.</summary>
    public readonly struct CompanionProfile
    {
        public readonly Element AttackElement;
        public readonly AbilityVariant AttackVariant;
        public readonly StatusKind OnHitStatus;
        public readonly float StatusMagnitude;
        public readonly float StatusDuration;
        public readonly DamageImmunity Immunity;
        public readonly bool CanBlink;    // dive/dig and reappear elsewhere
        public readonly bool CanWeb;      // spider: lay a flammable web that roots enemies
        public readonly bool CanRebirth;  // phoenix: revive once on death

        public CompanionProfile(Element attackElement, AbilityVariant attackVariant, StatusKind onHitStatus,
            float statusMagnitude, float statusDuration, DamageImmunity immunity, bool canBlink, bool canWeb, bool canRebirth)
        {
            AttackElement = attackElement;
            AttackVariant = attackVariant;
            OnHitStatus = onHitStatus;
            StatusMagnitude = statusMagnitude;
            StatusDuration = statusDuration;
            Immunity = immunity;
            CanBlink = canBlink;
            CanWeb = canWeb;
            CanRebirth = canRebirth;
        }
    }

    public static class CompanionProfiles
    {
        public static CompanionProfile For(CreatureKind kind)
        {
            switch (kind)
            {
                case CreatureKind.Spider: // earth attacks + flammable webs
                    return new CompanionProfile(Element.Earth, AbilityVariant.Standard, StatusKind.None, 0f, 0f,
                        DamageImmunity.None, false, true, false);

                case CreatureKind.WaterCat: // water attacks, immune to water (but ice hurts), dives & reappears
                    return new CompanionProfile(Element.Water, AbilityVariant.Standard, StatusKind.None, 0f, 0f,
                        new DamageImmunity(Element.Water, AbilityVariant.Ice, null), true, false, false);

                case CreatureKind.IceCat: // snow attacks (slow), immune to ice (but water hurts)
                    return new CompanionProfile(Element.Water, AbilityVariant.Ice, StatusKind.Slow, 0.5f, 2f,
                        new DamageImmunity(null, null, AbilityVariant.Ice), true, false, false);

                case CreatureKind.Phoenix: // fire attacks (burns), immune to fire, reborn once
                    return new CompanionProfile(Element.Fire, AbilityVariant.Magmacraft, StatusKind.Burn, 6f, 2f,
                        new DamageImmunity(Element.Fire, null, null), false, false, true);

                case CreatureKind.ElectricSquirrel: // lightning attacks (stun)
                    return new CompanionProfile(Element.Fire, AbilityVariant.Lightning, StatusKind.Stun, 1f, 0.6f,
                        DamageImmunity.None, false, false, false);

                case CreatureKind.Dog: // earth attacks, digs & reappears
                    return new CompanionProfile(Element.Earth, AbilityVariant.Standard, StatusKind.None, 0f, 0f,
                        DamageImmunity.None, true, false, false);

                case CreatureKind.StormWolf: // air attacks (gusts that slow), fast blink-dash
                    return new CompanionProfile(Element.Air, AbilityVariant.Standard, StatusKind.Slow, 0.4f, 1.5f,
                        DamageImmunity.None, true, false, false);

                case CreatureKind.VoltWolf: // air/lightning attacks (stun), fast blink-dash
                    return new CompanionProfile(Element.Air, AbilityVariant.Lightning, StatusKind.Stun, 1f, 0.5f,
                        DamageImmunity.None, true, false, false);

                default:
                    return new CompanionProfile(Element.Earth, AbilityVariant.Standard, StatusKind.None, 0f, 0f,
                        DamageImmunity.None, false, false, false);
            }
        }
    }

    /// <summary>Which rare companion can be found (and tamed) in a biome, or null. Deterministic per RNG.</summary>
    public static class CompanionSpawns
    {
        public static CreatureKind? For(BiomeType biome, IRandomSource rng)
        {
            double r = rng.NextUnit();
            switch (biome)
            {
                case BiomeType.Volcano: return CreatureKind.Phoenix;
                case BiomeType.Beach:
                case BiomeType.Island:
                case BiomeType.CoralReefForest:
                case BiomeType.Swamp:
                case BiomeType.Marsh: return CreatureKind.WaterCat;
                case BiomeType.Mountains: return r < 0.5 ? CreatureKind.IceCat : CreatureKind.Spider;
                case BiomeType.Desert: return CreatureKind.Dog;
                case BiomeType.CloudTemple: return CreatureKind.ElectricSquirrel;
                case BiomeType.Plains:
                case BiomeType.ForestTemple: return r < 0.5 ? CreatureKind.Spider : CreatureKind.Dog;
                default: return null;
            }
        }
    }
}
