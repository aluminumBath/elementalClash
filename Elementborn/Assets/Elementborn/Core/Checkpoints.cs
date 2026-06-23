using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>A respawn shrine at a world position. Activating one makes it your respawn anchor.</summary>
    public readonly struct Checkpoint
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Vector3 World;
        public Checkpoint(string id, string name, Vector3 world) { Id = id; Name = name; World = world; }
    }

    /// <summary>
    /// Which checkpoints the player has activated, and which one is the **active** respawn anchor (the most
    /// recently activated). Activating a different checkpoint moves the anchor. Savable; pure and unit-tested.
    /// </summary>
    public sealed class CheckpointLog
    {
        private readonly HashSet<string> _activated = new HashSet<string>();
        private string _active;

        /// <summary>The active respawn checkpoint id, or null if none has been activated.</summary>
        public string Active => _active;
        public bool IsActivated(string id) => _activated.Contains(id);
        public IReadOnlyCollection<string> Activated() => new List<string>(_activated);

        /// <summary>Record a checkpoint and make it the active respawn anchor. True if the active anchor changed.</summary>
        public bool Activate(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            _activated.Add(id);
            if (_active == id) return false;
            _active = id;
            return true;
        }

        public void Load(IEnumerable<string> activated, string active)
        {
            _activated.Clear();
            if (activated != null)
                foreach (var id in activated) if (!string.IsNullOrEmpty(id)) _activated.Add(id);
            _active = string.IsNullOrEmpty(active) ? null : active;
            if (_active != null) _activated.Add(_active); // the active anchor is always an activated one
        }
    }
}
