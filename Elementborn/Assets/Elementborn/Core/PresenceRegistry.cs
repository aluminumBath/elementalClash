using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// A small time-stamped store of where other players are, fed by whatever presence transport the build uses
    /// (a Nakama presence source online; a dev simulator offline). Entries go stale: only positions reported
    /// within a freshness window are returned, so a player who stops broadcasting or drops simply disappears.
    /// Pure and unit-tested; the map layer owns one of these and decides the window.
    /// </summary>
    public sealed class PresenceRegistry
    {
        private readonly Dictionary<string, Vector3> _pos = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, float> _at = new Dictionary<string, float>();

        /// <summary>Record (or refresh) a player's position at time <paramref name="now"/> (seconds).</summary>
        public void Report(string id, Vector3 position, float now)
        {
            if (string.IsNullOrEmpty(id)) return;
            _pos[id] = position;
            _at[id] = now;
        }

        /// <summary>Forget a player immediately (e.g. they stopped sharing or left the session).</summary>
        public void Drop(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            _pos.Remove(id);
            _at.Remove(id);
        }

        public void Clear() { _pos.Clear(); _at.Clear(); }

        /// <summary>True if <paramref name="id"/> reported a position within <paramref name="ttl"/> of <paramref name="now"/>.</summary>
        public bool IsFresh(string id, float now, float ttl) =>
            _at.TryGetValue(id, out float t) && (now - t) <= ttl;

        public bool TryGet(string id, float now, float ttl, out Vector3 position)
        {
            if (IsFresh(id, now, ttl) && _pos.TryGetValue(id, out position)) return true;
            position = default;
            return false;
        }

        /// <summary>Every position still fresh at <paramref name="now"/>. Also prunes anything past the window.</summary>
        public IReadOnlyDictionary<string, Vector3> Fresh(float now, float ttl)
        {
            Prune(now, ttl);
            return new Dictionary<string, Vector3>(_pos);
        }

        private void Prune(float now, float ttl)
        {
            List<string> stale = null;
            foreach (var kv in _at)
                if ((now - kv.Value) > ttl)
                {
                    if (stale == null) stale = new List<string>();
                    stale.Add(kv.Key);
                }
            if (stale == null) return;
            foreach (var id in stale) { _pos.Remove(id); _at.Remove(id); }
        }
    }
}
