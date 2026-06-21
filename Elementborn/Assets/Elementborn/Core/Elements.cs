namespace Elementborn.Core
{
    /// <summary>The four base channeling disciplines.</summary>
    public enum Element
    {
        Fire,
        Water,
        Earth,
        Air
    }

    /// <summary>Rare sub-disciplines, each gated behind a matching base element.</summary>
    public enum SubArt
    {
        None,
        Magmacraft,   // Fire
        SanguineGrip, // Water
        Oreshaping,   // Earth
        Flight        // Air
    }

    /// <summary>Maps base elements to their advanced sub-art, mirroring the source lore.</summary>
    public static class ElementMapping
    {
        public static SubArt SubArtFor(Element element) => element switch
        {
            Element.Fire => SubArt.Magmacraft,
            Element.Water => SubArt.SanguineGrip,
            Element.Earth => SubArt.Oreshaping,
            Element.Air => SubArt.Flight,
            _ => SubArt.None
        };
    }
}
