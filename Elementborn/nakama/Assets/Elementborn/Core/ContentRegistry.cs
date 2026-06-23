using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>A piece of registrable game content with a unique string id (so mods can add their own).</summary>
    public interface IContentDef
    {
        string Id { get; }
    }

    /// <summary>
    /// A simple id-keyed registry — the backbone of the data-driven content system. Built-in content is seeded
    /// in code; mods register more at load time through the same door. Lookups are case-insensitive, and a later
    /// registration of the same id wins (so a mod can override a built-in). Pure C#, no Unity dependency, so it
    /// is trivially testable.
    /// </summary>
    public sealed class ContentRegistry<T> where T : class, IContentDef
    {
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        public int Count => _items.Count;
        public IEnumerable<T> All => _items.Values;
        public IEnumerable<string> Ids => _items.Keys;

        public void Register(T def)
        {
            if (def == null || string.IsNullOrEmpty(def.Id)) return;
            _items[def.Id] = def; // last write wins — lets a mod override a built-in
        }

        public bool Contains(string id) => id != null && _items.ContainsKey(id);

        public T Get(string id) => id != null && _items.TryGetValue(id, out var d) ? d : null;

        public bool TryGet(string id, out T def)
        {
            if (id != null) return _items.TryGetValue(id, out def);
            def = null;
            return false;
        }

        public void Clear() => _items.Clear();
    }
}
