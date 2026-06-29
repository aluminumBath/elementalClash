using System;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Serializable data for a persistent map marker.
    /// Designed to work for boats, companions, items, NPCs, world discoveries, hazards, and custom pins.
    /// </summary>
    [Serializable]
    public class TrackedMapMarkerRecord
    {
        public string MarkerId;
        public MapMarkerType MarkerType = MapMarkerType.Unknown;
        public Vector3 WorldPosition;
        public string Label = string.Empty;
        public string ContextId = string.Empty;
        public string Notes = string.Empty;

        public bool IsVisible = true;
        public bool IsPersistent = true;
        public bool IsDiscovered = true;
        public bool HideWhileOverlappingPlayer = false;

        /// <summary>
        /// -1 means no expiry. Otherwise uses unscaled time as a simple runtime expiry.
        /// </summary>
        public float ExpiresAtUnscaledTime = -1f;

        public CreatureTraversalType CreatureTraversalType = CreatureTraversalType.Unknown;

        public bool IsExpired(float now)
        {
            return ExpiresAtUnscaledTime > 0f && now >= ExpiresAtUnscaledTime;
        }
    }
}
