using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// Which elements the player has encountered — by facing an opponent of that element, or being struck by a move
    /// of it. The attunement HUD reveals a matchup only once its element is in here; the player's own element is
    /// recorded up front. Pure, UnityEngine-free, savable, and monotonic (discovery never un-happens).
    /// </summary>
    public sealed class ElementDex
    {
        private readonly HashSet<Element> _seen = new HashSet<Element>();

        public bool IsDiscovered(Element element) => _seen.Contains(element);
        public int Count => _seen.Count;

        /// <summary>Mark an element discovered. True if it was newly added.</summary>
        public bool Discover(Element element) => _seen.Add(element);

        public IReadOnlyList<string> ToSave()
        {
            var list = new List<string>();
            foreach (var e in _seen) list.Add(e.ToString());
            return list;
        }

        public static ElementDex LoadFrom(IReadOnlyList<string> data)
        {
            var dex = new ElementDex();
            if (data != null)
                foreach (var s in data)
                    if (System.Enum.TryParse(s, out Element e)) dex._seen.Add(e);
            return dex;
        }
    }
}
