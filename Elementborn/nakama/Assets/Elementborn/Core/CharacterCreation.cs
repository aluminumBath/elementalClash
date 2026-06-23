namespace Elementborn.Core
{
    /// <summary>What the character-creation reveal should celebrate.</summary>
    public enum RevealTier
    {
        Weapon,
        BaseElement,
        SubArt,
        Confluence
    }

    public readonly struct CharacterCreationResult
    {
        public ChannelerLoadout Loadout { get; }
        public RevealTier Tier { get; }
        /// <summary>The element the player chose (meaningful when Tier is not Weapon).</summary>
        public Element ChosenElement { get; }

        public CharacterCreationResult(ChannelerLoadout loadout, RevealTier tier, Element chosenElement)
        {
            Loadout = loadout;
            Tier = tier;
            ChosenElement = chosenElement;
        }
    }

    /// <summary>
    /// Orchestrates character creation: the player chooses an element (or a weapon), the
    /// gacha roll decides a channeler's tier, and the result — loadout plus reveal tier — is
    /// returned for the UI to present and for the player to equip. Platform-agnostic, so
    /// the same flow backs both the VR and flat creation screens.
    /// </summary>
    public static class CharacterCreationService
    {
        public static CharacterCreationResult CreateChanneler(Element chosen, IRandomSource random, GachaConfig config)
        {
            ChannelerLoadout loadout = GachaRoller.RollForChanneler(chosen, random, config);
            RevealTier tier = loadout.IsConfluence
                ? RevealTier.Confluence
                : (loadout.SubArts.Count > 0 ? RevealTier.SubArt : RevealTier.BaseElement);
            return new CharacterCreationResult(loadout, tier, chosen);
        }

        public static CharacterCreationResult CreateWeaponUser(WeaponType weapon)
        {
            ChannelerLoadout loadout = GachaRoller.ChooseWeapon(weapon);
            return new CharacterCreationResult(loadout, RevealTier.Weapon, default);
        }
    }
}
