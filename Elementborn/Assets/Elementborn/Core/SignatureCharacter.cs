using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>Ash's special transformation.</summary>
    public enum DragonForm { Shadowthorn }

    /// <summary>One of the hero's named, unique companions (a bespoke ally, not a wild bestiary creature).</summary>
    public readonly struct SignatureSidekick
    {
        public readonly string Name;     // Apollo / Artemis / Iago
        public readonly string Species;  // Kitsune / Shadowhound / Iridescent Phoenix
        public readonly string Trait;    // its signature behaviour, described
        public readonly CompanionProfile Combat;

        public SignatureSidekick(string name, string species, string trait, CompanionProfile combat)
        {
            Name = name; Species = species; Trait = trait; Combat = combat;
        }
    }

    /// <summary>
    /// The owner's signature hero, Ash Shadowthorn: a three-element Channeler (Air / Water / Fire and each one's
    /// sub-art) who can take a dragon form, with three named companions. Data-only — the admin/identity side is
    /// <see cref="Social.AdminAccounts"/>. (Wild creatures live in the bestiary; these are bespoke hero allies.)
    /// </summary>
    public static class SignatureCharacter
    {
        public const string Name = "Ash Shadowthorn";
        public const string AdminEmail = "steeleschauer@gmail.com";
        public const string Appearance = "Teal eyes, reddish-brown hair";
        public const string Accent = "Deep Texan";
        public const DragonForm Dragon = DragonForm.Shadowthorn;

        /// <summary>Air, Water, and Fire, each with its sub-art (Flight, Sanguine Grip, Magmacraft). No weapon.</summary>
        public static ChannelerLoadout Loadout() => ChannelerLoadout.FromState(
            new[] { Element.Air, Element.Water, Element.Fire },
            new[] { SubArt.Flight, SubArt.SanguineGrip, SubArt.Magmacraft },
            WeaponType.None);

        /// <summary>The elements Iago the phoenix mirrors — the same as the hero's.</summary>
        public static IReadOnlyList<Element> IagoElements => Loadout().Elements;

        /// <summary>Apollo — a kitsune; a fast, fiery trickster.</summary>
        public static readonly SignatureSidekick Apollo = new SignatureSidekick(
            "Apollo", "Kitsune", "A fox-spirit trickster — fast, fiery feints.",
            new CompanionProfile(Element.Fire, AbilityVariant.Standard, StatusKind.Burn, 3f, 2f,
                DamageImmunity.None, canBlink: false, canWeb: false, canRebirth: false));

        /// <summary>Artemis — a shadowhound that vanishes into shadow and reappears from any other (blink).</summary>
        public static readonly SignatureSidekick Artemis = new SignatureSidekick(
            "Artemis", "Shadowhound", "Vanishes into shadow and teleports out of any other.",
            new CompanionProfile(Element.Air, AbilityVariant.Standard, StatusKind.None, 0f, 0f,
                DamageImmunity.None, canBlink: true, canWeb: false, canRebirth: false));

        /// <summary>Iago — an iridescent phoenix that wields Ash's own elements and is reborn once on death.</summary>
        public static readonly SignatureSidekick Iago = new SignatureSidekick(
            "Iago", "Iridescent Phoenix", "Wields Ash's own elements (Air/Water/Fire); reborn once when it falls.",
            new CompanionProfile(Element.Fire, AbilityVariant.Magmacraft, StatusKind.Burn, 4f, 3f,
                DamageImmunity.None, canBlink: false, canWeb: false, canRebirth: true));

        public static readonly SignatureSidekick[] Sidekicks = { Apollo, Artemis, Iago };
    }
}
