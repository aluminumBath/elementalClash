using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class BossRuntimeRecord
    {
        public string BossId = ""; public string DisplayName = ""; public BossState State = BossState.Dormant; public int CurrentPhaseIndex = -1; public bool Defeated = false; public float LastKnownHealth = 0f; public float LastKnownMaxHealth = 0f; public float X; public float Y; public float Z;
        public Vector3 Position { get => new Vector3(X,Y,Z); set { X=value.x; Y=value.y; Z=value.z; } }
    }
}
