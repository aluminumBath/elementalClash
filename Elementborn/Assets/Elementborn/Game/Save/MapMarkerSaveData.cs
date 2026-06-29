using System;
using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [Serializable]
    public class MapMarkerSaveFile
    {
        public int Version = 1;
        public int SlotIndex = 0;
        public List<MapMarkerSaveRecord> Markers = new List<MapMarkerSaveRecord>();
    }

    [Serializable]
    public class MapMarkerSaveRecord
    {
        public string MarkerId;
        public MapMarkerType MarkerType;
        public float X;
        public float Y;
        public float Z;
        public string Label;
        public string ContextId;
        public string Notes;
        public bool IsVisible;
        public bool IsPersistent;
        public bool IsDiscovered;
        public bool HideWhileOverlappingPlayer;
        public CreatureTraversalType CreatureTraversalType;

        public static MapMarkerSaveRecord FromRuntime(TrackedMapMarkerRecord marker)
        {
            return new MapMarkerSaveRecord
            {
                MarkerId = marker.MarkerId,
                MarkerType = marker.MarkerType,
                X = marker.WorldPosition.x,
                Y = marker.WorldPosition.y,
                Z = marker.WorldPosition.z,
                Label = marker.Label,
                ContextId = marker.ContextId,
                Notes = marker.Notes,
                IsVisible = marker.IsVisible,
                IsPersistent = marker.IsPersistent,
                IsDiscovered = marker.IsDiscovered,
                HideWhileOverlappingPlayer = marker.HideWhileOverlappingPlayer,
                CreatureTraversalType = marker.CreatureTraversalType
            };
        }

        public Vector3 ToPosition()
        {
            return new Vector3(X, Y, Z);
        }
    }
}
