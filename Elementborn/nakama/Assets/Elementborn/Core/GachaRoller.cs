namespace Elementborn.Core
{
    /// <summary>
    /// Resolves character creation. The player chooses an element (or a weapon);
    /// luck then decides whether a channeler is upgraded to a sub-art or, very rarely,
    /// the full Confluence set.
    /// </summary>
    public static class GachaRoller
    {
        /// <summary>Rolls a loadout for a player who chose to bend <paramref name="chosenElement"/>.</summary>
        public static ChannelerLoadout RollForChanneler(Element chosenElement, IRandomSource random, GachaConfig config)
        {
            if (random == null) throw new System.ArgumentNullException(nameof(random));
            if (config == null) throw new System.ArgumentNullException(nameof(config));

            double roll = random.NextUnit();

            if (roll < config.ConfluenceChance)
                return ChannelerLoadout.Confluence(config.ConfluenceIncludesSubArts);

            if (roll < config.ConfluenceChance + config.SubArtChance)
                return ChannelerLoadout.ElementWithSubArt(chosenElement);

            return ChannelerLoadout.SingleElement(chosenElement);
        }

        /// <summary>Creates a loadout for a player who opted out of channeling.</summary>
        public static ChannelerLoadout ChooseWeapon(WeaponType weapon) =>
            ChannelerLoadout.WeaponUser(weapon);
    }
}
