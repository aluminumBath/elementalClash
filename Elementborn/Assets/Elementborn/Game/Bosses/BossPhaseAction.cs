using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class BossPhaseAction
    {
        public BossPhaseActionType ActionType = BossPhaseActionType.Announce;
        public string Message = "";
        public EnemyCombatProfile ProfileOverride;
        public CombatAttackDefinition BonusAttack;
        public StatusEffectDefinition SelfStatus;
        public GameObject Prefab;
        public int SpawnCount = 1;
        public float SpawnRadius = 6f;
        public string QuestId = "";
        public string ObjectiveId = "";
    }
}
