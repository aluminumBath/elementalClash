using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>
    /// The moddable form of an enemy — a stat block plus an optional element and a behaviour template
    /// (<see cref="BaseKind"/>, which says whether it kites or closes in, etc.). Built-ins are bridged in from
    /// <see cref="EnemyArchetypes"/>; mods add more by string id.
    /// </summary>
    public sealed class EnemyDef : IContentDef
    {
        public string Id { get; }
        public readonly string Name;
        public readonly EnemyStats Stats;
        public readonly Element? Element;
        public readonly EnemyKind BaseKind;

        public EnemyDef(string id, string name, EnemyStats stats, Element? element, EnemyKind baseKind)
        {
            Id = id;
            Name = string.IsNullOrEmpty(name) ? id : name;
            Stats = stats;
            Element = element;
            BaseKind = baseKind;
        }

        public static EnemyDef FromArchetype(EnemyKind kind) =>
            new EnemyDef(kind.ToString(), kind.ToString(), EnemyArchetypes.For(kind), null, kind);
    }

    /// <summary>
    /// The moddable superset of enemies. Seeds from the built-in <see cref="EnemyArchetypes"/> on first use;
    /// mods register more (see <c>ModLoader</c>). A spawner builds one with <c>EnemyController.ConfigureById</c>.
    /// </summary>
    public static class EnemyRegistry
    {
        private static readonly ContentRegistry<EnemyDef> _reg = new ContentRegistry<EnemyDef>();
        private static bool _seeded;

        public static void EnsureSeeded()
        {
            if (_seeded) return;
            _seeded = true;
            foreach (EnemyKind k in Enum.GetValues(typeof(EnemyKind)))
                _reg.Register(EnemyDef.FromArchetype(k));
        }

        public static void Register(EnemyDef def) { EnsureSeeded(); _reg.Register(def); }
        public static EnemyDef Get(string id) { EnsureSeeded(); return _reg.Get(id); }
        public static bool TryGet(string id, out EnemyDef def) { EnsureSeeded(); return _reg.TryGet(id, out def); }
        public static IEnumerable<EnemyDef> All { get { EnsureSeeded(); return _reg.All; } }
        public static int Count { get { EnsureSeeded(); return _reg.Count; } }
    }
}
