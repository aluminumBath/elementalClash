using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class WorldEventRecord
    {
        public string EventId = "";
        public string DisplayName = "";
        public WorldEventType EventType = WorldEventType.Unknown;
        public WorldEventState State = WorldEventState.Inactive;
        public string Region = "";
        public float X;
        public float Y;
        public float Z;
        public bool HasWorldPosition;
        public float ScheduledAtUnscaledTime = -1f;
        public float ActivatedAtUnscaledTime = -1f;
        public float ExpiresAtUnscaledTime = -1f;
        public int TimesActivated = 0;
        public string LastReason = "";

        public Vector3 WorldPosition
        {
            get => new Vector3(X, Y, Z);
            set { X = value.x; Y = value.y; Z = value.z; }
        }

        public bool IsExpired(float now)
        {
            return ExpiresAtUnscaledTime > 0f && now >= ExpiresAtUnscaledTime;
        }
    }
}
