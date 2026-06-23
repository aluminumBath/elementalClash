using System.Collections.Generic;
using System.Linq;

namespace Elementborn.Core
{
    /// <summary>
    /// The result of character creation: what a player can actually do.
    /// A loadout is either element-based (one element, an upgraded sub-art,
    /// or the full Confluence set) or weapon-based.
    /// </summary>
    public sealed class ChannelerLoadout
    {
        public IReadOnlyList<Element> Elements { get; }
        public IReadOnlyList<SubArt> SubArts { get; }
        public WeaponType Weapon { get; }

        public bool IsChanneler => Elements.Count > 0;
        public bool IsConfluence => Elements.Count >= 4;

        private ChannelerLoadout(IEnumerable<Element> elements, IEnumerable<SubArt> subArts, WeaponType weapon)
        {
            Elements = elements?.ToList() ?? new List<Element>();
            SubArts = subArts?.Where(s => s != SubArt.None).ToList() ?? new List<SubArt>();
            Weapon = weapon;
        }

        public static ChannelerLoadout SingleElement(Element element) =>
            new ChannelerLoadout(new[] { element }, null, WeaponType.None);

        public static ChannelerLoadout ElementWithSubArt(Element element) =>
            new ChannelerLoadout(new[] { element }, new[] { ElementMapping.SubArtFor(element) }, WeaponType.None);

        public static ChannelerLoadout Confluence(bool includeSubArts)
        {
            var elements = new[] { Element.Fire, Element.Water, Element.Earth, Element.Air };
            var subArts = includeSubArts
                ? elements.Select(ElementMapping.SubArtFor)
                : System.Array.Empty<SubArt>();
            return new ChannelerLoadout(elements, subArts, WeaponType.None);
        }

        public static ChannelerLoadout WeaponUser(WeaponType weapon) =>
            new ChannelerLoadout(null, null, weapon);

        /// <summary>Rebuild a loadout from stored state (used by save/load). Mirrors the private ctor's rules.</summary>
        public static ChannelerLoadout FromState(IEnumerable<Element> elements, IEnumerable<SubArt> subArts, WeaponType weapon) =>
            new ChannelerLoadout(elements, subArts, weapon);

        public bool HasElement(Element element) => Elements.Contains(element);
        public bool HasSubArt(SubArt subArt) => SubArts.Contains(subArt);
    }
}
