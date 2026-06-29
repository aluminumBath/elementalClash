using UnityEngine;
namespace Elementborn.Game
{
    public sealed class QuestWaypointBinder : MonoBehaviour
    {
        [SerializeField] private bool updateEveryFrame = false;
        private void OnEnable(){ QuestUiEventHub.QuestStarted += Handle; QuestUiEventHub.QuestUpdated += Handle; QuestUiEventHub.QuestTrackedChanged += Handle; }
        private void OnDisable(){ QuestUiEventHub.QuestStarted -= Handle; QuestUiEventHub.QuestUpdated -= Handle; QuestUiEventHub.QuestTrackedChanged -= Handle; }
        private void Update(){ if(updateEveryFrame) SyncTrackedWaypoint(); }
        public void SyncTrackedWaypoint(){ var quest=QuestUiTracker.GetTrackedQuest(); var objective=quest!=null?quest.CurrentObjective:null; if(quest==null || objective==null || !objective.CreateWaypoint) return; WaypointTracker.SetWaypoint(objective.WorldPosition, objective.Title, quest.QuestId); }
        private void Handle(QuestUiRecord quest)=>SyncTrackedWaypoint();
    }
}
