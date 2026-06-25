using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Runtime owner of the player's checkpoint progress: the <see cref="CheckpointLog"/> of activated shrines and
    /// which one is the active respawn anchor. <see cref="CheckpointObject"/>s activate through it,
    /// <see cref="RespawnController"/> asks it where to revive, the map/minimap read its markers, and it persists
    /// through <see cref="PlayerInventory"/>. The bootstrap scene adds one.
    /// </summary>
    public sealed class CheckpointState : MonoBehaviour
    {
        public static CheckpointState Instance { get; private set; }

        private readonly CheckpointLog _log = new CheckpointLog();
        public CheckpointLog Log => _log;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public bool IsActive(string id) => _log.Active == id;

        /// <summary>Activate a known checkpoint (ignored if the id isn't canonical). True if the anchor changed.</summary>
        public bool Activate(string id)
        {
            foreach (var c in WorldMapLayout.Checkpoints)
                if (c.Id == id) return _log.Activate(id);
            return false;
        }

        /// <summary>The active respawn point, settled on the terrain. False if no checkpoint is active.</summary>
        public bool TryActivePosition(out Vector3 position)
        {
            position = default;
            string id = _log.Active;
            if (string.IsNullOrEmpty(id)) return false;
            foreach (var c in WorldMapLayout.Checkpoints)
                if (c.Id == id)
                {
                    Vector3 p = c.World;
                    p.y = TerrainHeight.Sample(p) + 1f;
                    position = p;
                    return true;
                }
            return false;
        }

        /// <summary>Every checkpoint as a map marker (kind <see cref="MapMarkerKind.Checkpoint"/>) for the map/minimap.</summary>
        public List<MapMarker> Markers()
        {
            var list = new List<MapMarker>();
            foreach (var c in WorldMapLayout.Checkpoints)
                list.Add(new MapMarker(c.Id, MapMarkerKind.Checkpoint, c.World, c.Name));
            return list;
        }

        // --- persistence (folded into PlayerInventory.ToSave / LoadFrom) ---
        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.activatedCheckpoints.Clear();
            foreach (var id in _log.Activated()) d.activatedCheckpoints.Add(id);
            d.activeCheckpoint = _log.Active ?? "";
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            _log.Load(d.activatedCheckpoints, d.activeCheckpoint);
        }
    }
}
