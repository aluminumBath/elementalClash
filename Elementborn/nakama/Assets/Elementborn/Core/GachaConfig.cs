namespace Elementborn.Core
{
    /// <summary>
    /// Tunable drop rates for the character-creation roll.
    /// Bands are evaluated low-to-high: Confluence first, then sub-art, else base element.
    /// </summary>
    public sealed class GachaConfig
    {
        /// <summary>Chance (0-1) of rolling the full Confluence set. Extremely rare.</summary>
        public double ConfluenceChance { get; set; } = 0.001;

        /// <summary>Chance (0-1) of upgrading the chosen element to its sub-art.</summary>
        public double SubArtChance { get; set; } = 0.05;

        /// <summary>Whether an Confluence roll also grants every sub-art.</summary>
        public bool ConfluenceIncludesSubArts { get; set; } = true;

        public static GachaConfig Default => new GachaConfig();
    }
}
