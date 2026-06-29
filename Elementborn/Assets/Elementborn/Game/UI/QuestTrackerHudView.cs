using UnityEngine;
using UnityEngine.UI;
namespace Elementborn.Game
{
    public sealed class QuestTrackerHudView : MonoBehaviour
    {
        [SerializeField] private GameObject root; [SerializeField] private Text titleText; [SerializeField] private Text objectiveText; [SerializeField] private Text distanceText; [SerializeField] private Transform player;
        private void OnEnable(){ QuestUiEventHub.QuestStarted += Handle; QuestUiEventHub.QuestUpdated += Handle; QuestUiEventHub.QuestTrackedChanged += Handle; QuestUiEventHub.QuestCompleted += Handle; QuestUiEventHub.ObjectiveCompleted += HandleObjective; }
        private void OnDisable(){ QuestUiEventHub.QuestStarted -= Handle; QuestUiEventHub.QuestUpdated -= Handle; QuestUiEventHub.QuestTrackedChanged -= Handle; QuestUiEventHub.QuestCompleted -= Handle; QuestUiEventHub.ObjectiveCompleted -= HandleObjective; }
        private void Update(){ Refresh(); }
        public void Refresh(){ var quest=QuestUiTracker.GetTrackedQuest(); bool active=quest!=null && quest.State==QuestUiState.Active; if(root!=null) root.SetActive(active); if(!active) return; var objective=quest.CurrentObjective; if(titleText!=null) titleText.text=quest.Title; if(objectiveText!=null) objectiveText.text=objective!=null?objective.Title:""; if(distanceText!=null) distanceText.text=objective!=null && player!=null ? $"{Vector3.Distance(player.position, objective.WorldPosition):0}m" : ""; }
        private void Handle(QuestUiRecord q)=>Refresh(); private void HandleObjective(QuestUiRecord q, QuestObjectiveUiRecord o)=>Refresh();
    }
}
