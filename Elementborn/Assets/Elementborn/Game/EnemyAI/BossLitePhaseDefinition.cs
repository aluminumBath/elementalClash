using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class BossLitePhaseDefinition
    {
        [Range(0f, 1f)]
        public float StartAtHealthPercent = 1f;
        public EnemyCombatProfile ProfileOverride;
        public CombatAttackDefinition BonusAttack;
        public StatusEffectDefinition SelfBuffStatus;
        public string PhaseAnnouncement = "The boss changes tactics.";
    }
}
