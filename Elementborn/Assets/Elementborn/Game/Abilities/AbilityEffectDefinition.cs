using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class AbilityEffectDefinition
    {
        [Header("Numbers")]
        public float Power = 1f;
        public float Radius = 0f;
        public float DurationSeconds = 0f;
        public float CooldownSeconds = 1f;
        public float ResourceCost = 0f;

        [Header("Gameplay Flags")]
        public bool IsProjectile;
        public bool IsAreaEffect;
        public bool IsMovement;
        public bool IsBuff;
        public bool IsHealing;
        public bool IsBoatEffect;
        public bool IsCreatureEffect;

        [Header("Debug")]
        public string DebugEffectName = "";
    }
}
