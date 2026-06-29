using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [RequireComponent(typeof(SimpleCombatHealth))]
    public sealed class BossController : MonoBehaviour
    {
        [SerializeField] private BossDefinition bossDefinition;
        [SerializeField] private SimpleCombatHealth health;
        [SerializeField] private EnemyCombatBrain combatBrain;
        [SerializeField] private EnemyWeaknessAwareSelector weaknessSelector;
        [SerializeField] private BossArenaController arena;
        [SerializeField] private Transform summonRoot;
        [SerializeField] private bool startOnAwake = false;
        private BossState state = BossState.Dormant;
        private int currentPhaseIndex = -1;
        private readonly List<GameObject> spawnedAdds = new List<GameObject>();
        private bool rewardsGranted;
        public BossDefinition Definition => bossDefinition; public BossState State => state; public int CurrentPhaseIndex => currentPhaseIndex;
        public float CurrentHealth => health != null ? health.CurrentHealth : 0f; public float MaxHealth => health != null ? health.MaxHealth : 0f; public float Health01 => MaxHealth <= 0f ? 0f : CurrentHealth / MaxHealth;
        public string BossId => bossDefinition != null ? bossDefinition.BossId : gameObject.name; public string DisplayName => bossDefinition != null ? bossDefinition.DisplayName : gameObject.name;
        private void Awake(){ if(health==null) health=GetComponent<SimpleCombatHealth>(); if(combatBrain==null) combatBrain=GetComponent<EnemyCombatBrain>(); if(weaknessSelector==null) weaknessSelector=GetComponent<EnemyWeaknessAwareSelector>(); if(summonRoot==null) summonRoot=transform; }
        private void OnEnable(){ if(health!=null) health.Died += HandleDefeated; }
        private void OnDisable(){ if(health!=null) health.Died -= HandleDefeated; }
        private void Start(){ RegisterMarkerAndJournal(); if(startOnAwake) StartBoss(); }
        private void Update(){ if(state==BossState.Active || state==BossState.PhaseTransition) TickPhases(); }
        public void StartBoss(){ if(state==BossState.Active || state==BossState.Defeated) return; state=BossState.Intro; if(arena!=null) arena.BeginArena(this); string intro=bossDefinition!=null && !string.IsNullOrWhiteSpace(bossDefinition.IntroMessage) ? bossDefinition.IntroMessage : DisplayName + " awakens!"; NotificationFeed.Post(intro, NotificationType.Combat); state=BossState.Active; BossEventHub.RaiseBossStarted(this); TickPhases(true); }
        public void ResetBoss(){ state=BossState.Resetting; currentPhaseIndex=-1; rewardsGranted=false; ClearAdds(); if(health!=null) health.Revive(); if(arena!=null) arena.ResetArena(); state=BossState.Dormant; BossEventHub.RaiseBossReset(this); }
        public void ForcePhase(int phaseIndex){ ApplyPhase(phaseIndex); }
        public BossRuntimeRecord ToRecord(){ return new BossRuntimeRecord{BossId=BossId,DisplayName=DisplayName,State=state,CurrentPhaseIndex=currentPhaseIndex,Defeated=state==BossState.Defeated,LastKnownHealth=CurrentHealth,LastKnownMaxHealth=MaxHealth,Position=transform.position}; }
        public void Import(BossRuntimeRecord record){ if(record==null) return; state=record.State; currentPhaseIndex=record.CurrentPhaseIndex; transform.position=record.Position; }

        public void Configure(BossDefinition definition, BossArenaController arenaController)
        {
            bossDefinition = definition;
            arena = arenaController;
            if (health == null) health = GetComponent<SimpleCombatHealth>();
            if (combatBrain == null) combatBrain = GetComponent<EnemyCombatBrain>();
            if (weaknessSelector == null) weaknessSelector = GetComponent<EnemyWeaknessAwareSelector>();
            if (summonRoot == null) summonRoot = transform;
            RegisterMarkerAndJournal();
        }

        private void TickPhases(bool force=false){ if(bossDefinition==null || bossDefinition.Phases==null || bossDefinition.Phases.Count==0 || health==null) return; float hp=Health01; int next=currentPhaseIndex; for(int i=0;i<bossDefinition.Phases.Count;i++){ var phase=bossDefinition.Phases[i]; if(phase!=null && hp<=phase.StartAtHealthPercent) next=i; } if(force && next<0) next=0; if(next!=currentPhaseIndex && next>=0 && next<bossDefinition.Phases.Count) ApplyPhase(next); }
        private void ApplyPhase(int phaseIndex){ if(bossDefinition==null || phaseIndex<0 || phaseIndex>=bossDefinition.Phases.Count) return; currentPhaseIndex=phaseIndex; state=BossState.PhaseTransition; var phase=bossDefinition.Phases[phaseIndex]; string ann=!string.IsNullOrWhiteSpace(phase.PhaseAnnouncement)?phase.PhaseAnnouncement:$"{DisplayName}: {phase.PhaseName}"; NotificationFeed.Post(ann, NotificationType.Combat); if(phase.Actions!=null) foreach(var action in phase.Actions) ExecuteAction(action); BossEventHub.RaiseBossPhaseChanged(this, phaseIndex); state=BossState.Active; }
        private void ExecuteAction(BossPhaseAction action){ if(action==null) return; switch(action.ActionType){ case BossPhaseActionType.Announce: if(!string.IsNullOrWhiteSpace(action.Message)) NotificationFeed.Post(action.Message, NotificationType.Combat); break; case BossPhaseActionType.SwitchProfile: if(combatBrain!=null && action.ProfileOverride!=null) combatBrain.SetProfile(action.ProfileOverride); break; case BossPhaseActionType.AddAttack: if(weaknessSelector!=null && action.BonusAttack!=null) NotificationFeed.Post(DisplayName + " changes attacks.", NotificationType.Combat); break; case BossPhaseActionType.ApplySelfStatus: if(action.SelfStatus!=null){ var status=GetComponent<StatusEffectController>() ?? gameObject.AddComponent<StatusEffectController>(); status.Apply(action.SelfStatus);} break; case BossPhaseActionType.SummonAdds: SummonAdds(action); break; case BossPhaseActionType.EnableHazards: if(arena!=null) arena.SetHazardsActive(true); break; case BossPhaseActionType.DisableHazards: if(arena!=null) arena.SetHazardsActive(false); break; case BossPhaseActionType.CompleteQuestObjective: if(!string.IsNullOrWhiteSpace(action.QuestId)) QuestObjectiveTracker.Ensure().CompleteObjective(action.QuestId, action.ObjectiveId); break; } }
        private void SummonAdds(BossPhaseAction action){ if(action.Prefab==null) return; int count=Mathf.Max(1,action.SpawnCount); float radius=Mathf.Max(0f,action.SpawnRadius); for(int i=0;i<count;i++){ Vector2 c=Random.insideUnitCircle*radius; GameObject add=Instantiate(action.Prefab, summonRoot.position+new Vector3(c.x,0f,c.y), Quaternion.identity); spawnedAdds.Add(add);} NotificationFeed.Post(DisplayName + " summons allies!", NotificationType.Combat); }
        private void HandleDefeated(){ if(state==BossState.Defeated) return; state=BossState.Defeated; ClearAdds(); if(arena!=null) arena.CompleteArena(); GrantRewards(); string defeat=bossDefinition!=null && !string.IsNullOrWhiteSpace(bossDefinition.DefeatMessage)?bossDefinition.DefeatMessage:DisplayName+" defeated!"; NotificationFeed.Post(defeat, NotificationType.Combat); PlayerMapMarkerTracker.RemoveMarker("boss_" + PlayerJournalTracker.Safe(BossId)); BossEventHub.RaiseBossDefeated(this); }
        private void GrantRewards(){ if(rewardsGranted || bossDefinition==null) return; rewardsGranted=true; if(bossDefinition.CurrencyReward>0) PlayerInventoryTracker.AddCurrency(bossDefinition.CurrencyReward); if(bossDefinition.SkillPointReward>0) PlayerAbilityTracker.AddSkillPoints(bossDefinition.SkillPointReward, "Boss defeated: "+DisplayName); if(bossDefinition.LootTable!=null){ foreach(var entry in bossDefinition.LootTable.Entries){ if(entry==null || string.IsNullOrWhiteSpace(entry.ItemId) || !entry.RollDrop()) continue; int qty=entry.RollQuantity(); if(entry.Item!=null) PlayerInventoryTracker.AddItem(entry.Item, qty); else PlayerInventoryTracker.AddItemId(entry.ItemId, qty); }} if(!string.IsNullOrWhiteSpace(bossDefinition.QuestIdOnDefeat)) QuestObjectiveTracker.Ensure().CompleteObjective(bossDefinition.QuestIdOnDefeat, bossDefinition.ObjectiveIdOnDefeat); PlayerJournalTracker.AddOrUpdateEntry("boss_defeated_"+PlayerJournalTracker.Safe(BossId), JournalEntryType.Discovery, DisplayName+" defeated", bossDefinition.Description, bossDefinition.Region, BossId); }
        private void RegisterMarkerAndJournal(){ if(bossDefinition==null) return; if(bossDefinition.AddMapMarker) PlayerMapMarkerTracker.ReportOrUpdateMarker("boss_"+PlayerJournalTracker.Safe(bossDefinition.BossId), bossDefinition.MarkerType, bossDefinition.MapPosition, bossDefinition.DisplayName, isPersistent:true, notes:bossDefinition.Description); PlayerJournalTracker.AddOrUpdateEntry("boss_"+PlayerJournalTracker.Safe(bossDefinition.BossId), JournalEntryType.Discovery, bossDefinition.DisplayName, bossDefinition.Description, bossDefinition.Region, bossDefinition.BossId); }
        private void ClearAdds(){ for(int i=spawnedAdds.Count-1;i>=0;i--){ if(spawnedAdds[i]!=null) Destroy(spawnedAdds[i]); } spawnedAdds.Clear(); }
    }
}
