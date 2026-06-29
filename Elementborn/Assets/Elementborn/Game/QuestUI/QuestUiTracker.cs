using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class QuestUiTracker : MonoBehaviour
    {
        public static QuestUiTracker Instance { get; private set; }
        [SerializeField] private List<QuestUiRecord> quests = new List<QuestUiRecord>();
        [SerializeField] private string trackedQuestId = "";
        public IReadOnlyList<QuestUiRecord> Quests => quests;
        public string TrackedQuestId => trackedQuestId;
        private void Awake(){ if(Instance!=null && Instance!=this){ Destroy(gameObject); return; } Instance=this; DontDestroyOnLoad(gameObject); }
        public static QuestUiTracker Ensure(){ if(Instance!=null) return Instance; var go=new GameObject(nameof(QuestUiTracker)); return go.AddComponent<QuestUiTracker>(); }
        public static QuestUiRecord StartQuest(QuestUiDefinition definition)
        {
            if(definition==null) return null; var tracker=Ensure(); var record=tracker.GetOrCreate(definition.QuestId);
            record.Title=definition.Title; record.Description=definition.Description; record.Region=definition.Region; record.GiverName=definition.GiverName; record.State=QuestUiState.Active; record.LastUpdatedAtUnscaledTime=Time.unscaledTime; record.ActiveObjectiveIndex=0;
            record.Objectives.Clear(); foreach(var o in definition.Objectives){ if(o==null) continue; record.Objectives.Add(new QuestObjectiveUiRecord{ ObjectiveId=o.ObjectiveId, Title=o.Title, Description=o.Description, State=QuestUiState.Active, WorldPosition=o.WorldPosition, CreateWaypoint=o.CreateWaypoint, Required=o.Required }); }
            record.Rewards.Clear(); foreach(var r in definition.Rewards){ if(r!=null) record.Rewards.Add(r); }
            if(definition.AutoTrack) TrackQuest(record.QuestId); SyncObjectiveSystem(record); PostJournal(record); QuestUiEventHub.RaiseQuestStarted(record); NotificationFeed.Post($"Quest started: {record.Title}", NotificationType.Journal); return record;
        }
        public static QuestUiRecord StartRuntimeQuest(string questId,string title,string description,string region,string giverName,Vector3 objectivePosition,string objectiveTitle="Go to objective")
        { var tracker=Ensure(); var record=tracker.GetOrCreate(questId); record.Title=string.IsNullOrWhiteSpace(title)?questId:title; record.Description=description??""; record.Region=region??""; record.GiverName=giverName??""; record.State=QuestUiState.Active; record.Tracked=true; record.LastUpdatedAtUnscaledTime=Time.unscaledTime; record.ActiveObjectiveIndex=0; record.Objectives.Clear(); record.Objectives.Add(new QuestObjectiveUiRecord{ ObjectiveId="objective_1", Title=objectiveTitle, Description=description??"", State=QuestUiState.Active, WorldPosition=objectivePosition, CreateWaypoint=true, Required=true }); tracker.trackedQuestId=record.QuestId; SyncObjectiveSystem(record); PostJournal(record); QuestUiEventHub.RaiseQuestStarted(record); NotificationFeed.Post($"Quest started: {record.Title}", NotificationType.Journal); return record; }
        public static bool TrackQuest(string questId){ var tracker=Ensure(); var record=tracker.Find(questId); if(record==null) return false; foreach(var q in tracker.quests) if(q!=null) q.Tracked=false; record.Tracked=true; tracker.trackedQuestId=record.QuestId; record.LastUpdatedAtUnscaledTime=Time.unscaledTime; SyncObjectiveSystem(record); QuestUiEventHub.RaiseQuestTrackedChanged(record); return true; }
        public static bool CompleteObjective(string questId,string objectiveId){ var record=Ensure().Find(questId); if(record==null) return false; var objective=record.Objectives.Find(o=>o.ObjectiveId==objectiveId) ?? record.CurrentObjective; if(objective==null) return false; objective.State=QuestUiState.ObjectiveComplete; record.LastUpdatedAtUnscaledTime=Time.unscaledTime; QuestUiEventHub.RaiseObjectiveCompleted(record,objective); NotificationFeed.Post($"Objective complete: {objective.Title}", NotificationType.Journal); int index=record.Objectives.IndexOf(objective); if(index>=0 && index+1<record.Objectives.Count){ record.ActiveObjectiveIndex=index+1; SyncObjectiveSystem(record); QuestUiEventHub.RaiseQuestUpdated(record); } else CompleteQuest(record.QuestId); return true; }
        public static bool CompleteQuest(string questId){ var record=Ensure().Find(questId); if(record==null) return false; record.State=QuestUiState.Complete; record.LastUpdatedAtUnscaledTime=Time.unscaledTime; foreach(var objective in record.Objectives) if(objective!=null) objective.State=QuestUiState.ObjectiveComplete; PlayerJournalTracker.AddOrUpdateEntry("quest_complete_"+PlayerJournalTracker.Safe(record.QuestId), JournalEntryType.Quest, record.Title+" Complete", record.Description, record.Region, record.QuestId); QuestUiEventHub.RaiseQuestCompleted(record); NotificationFeed.Post($"Quest complete: {record.Title}", NotificationType.Journal); return true; }
        public static QuestUiRecord GetTrackedQuest(){ var tracker=Ensure(); if(!string.IsNullOrWhiteSpace(tracker.trackedQuestId)){ var tracked=tracker.Find(tracker.trackedQuestId); if(tracked!=null) return tracked; } return tracker.quests.Find(q=>q!=null && q.State==QuestUiState.Active); }
        public static void ClearAll(){ var tracker=Ensure(); tracker.quests.Clear(); tracker.trackedQuestId=""; }
        public void ImportRecord(QuestUiRecord record){ if(record==null || string.IsNullOrWhiteSpace(record.QuestId)) return; quests.RemoveAll(q=>q.QuestId==record.QuestId); quests.Add(record); if(record.Tracked) trackedQuestId=record.QuestId; }
        private QuestUiRecord GetOrCreate(string questId){ var record=Find(questId); if(record!=null) return record; record=new QuestUiRecord{ QuestId=questId, Title=questId, State=QuestUiState.Available }; quests.Add(record); return record; }
        private QuestUiRecord Find(string questId){ return quests.Find(q=>q!=null && q.QuestId==questId); }
        private static void SyncObjectiveSystem(QuestUiRecord record){ var objective=record.CurrentObjective; if(objective==null) return; QuestObjectiveTracker.Ensure().SetObjective(record.QuestId, objective.ObjectiveId, objective.Title, objective.Description, objective.WorldPosition); if(objective.CreateWaypoint) WaypointTracker.SetWaypoint(objective.WorldPosition, objective.Title, record.QuestId); }
        private static void PostJournal(QuestUiRecord record){ PlayerJournalTracker.AddOrUpdateEntry("quest_"+PlayerJournalTracker.Safe(record.QuestId), JournalEntryType.Quest, record.Title, record.Description, record.Region, record.QuestId); }
    }
}
