using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Bootstrap/Playable Scene Profile", fileName = "PlayableSceneBootstrapProfile")]
    public sealed class PlayableSceneBootstrapProfile : ScriptableObject
    {
        [Header("Scene Pieces")]
        public bool CreateRuntimeSystems = true;
        public bool CreatePlayerRig = true;
        public bool CreateUiCanvas = true;
        public bool CreateTestEnemy = true;
        public bool CreateBossArena = true;
        public bool CreateBoatSetup = true;

        [Header("Positions")]
        public Vector3 PlayerPosition = new Vector3(0f, 1f, 0f);
        public Vector3 EnemyPosition = new Vector3(8f, 1f, 8f);
        public Vector3 BossArenaPosition = new Vector3(24f, 0f, 0f);
        public Vector3 BoatPosition = new Vector3(-12f, 0f, 12f);

        [Header("Optional Assets")]
        public EnemyCombatProfile EnemyProfile;
        public BossDefinition BossDefinition;
        public CombatAttackDefinition EnemyAttack;
        public SpellCastDefinition[] StarterSpells;
        public QuestUiDefinition[] StarterQuests;

        [Header("Checklist")]
        public List<BootstrapChecklistItem> Checklist = new List<BootstrapChecklistItem>();
    }
}
