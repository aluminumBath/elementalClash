namespace Elementborn.Core
{
    /// <summary>What an element lets you do to cross water.</summary>
    public enum TravelMode { None, IceFloe, Bubble }

    /// <summary>Pure mapping from a channeler's element to its water-travel ability.</summary>
    public static class ElementTravel
    {
        public static TravelMode ModeFor(Element? element)
        {
            if (element == Element.Water) return TravelMode.IceFloe;
            if (element == Element.Air) return TravelMode.Bubble;
            return TravelMode.None;
        }
    }
}
