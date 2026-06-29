using UnityEngine;
using UnityEngine.UI;
namespace Elementborn.Game
{
    public sealed class QuestLogPanelView : MonoBehaviour
    {
        [SerializeField] private Text text; [SerializeField] private bool includeCompleted = true; [SerializeField] private bool refreshEveryFrame = false;
        private void OnEnable(){ Refresh(); } private void Update(){ if(refreshEveryFrame) Refresh(); }
        [ContextMenu("Refresh")]
        public void Refresh(){ if(text==null) return; var sb=new System.Text.StringBuilder(); sb.AppendLine("Quest Log"); foreach(var quest in QuestUiTracker.Ensure().Quests){ if(quest==null) continue; if(!includeCompleted && quest.State==QuestUiState.Complete) continue; string tracked=quest.Tracked?"*":"-"; sb.AppendLine($"{tracked} {quest.Title} [{quest.State}]"); if(!string.IsNullOrWhiteSpace(quest.Description)) sb.AppendLine($"  {quest.Description}"); for(int i=0;i<quest.Objectives.Count;i++){ var o=quest.Objectives[i]; if(o!=null) sb.AppendLine($"  {(i==quest.ActiveObjectiveIndex?">":" ")} {o.Title} [{o.State}]"); } if(quest.Rewards.Count>0){ sb.AppendLine("  Rewards:"); foreach(var r in quest.Rewards) if(r!=null) sb.AppendLine($"   - {r.DisplayName} x{Mathf.Max(1,r.Quantity)}"); } } text.text=sb.ToString(); }
        public void TrackQuest(string questId){ QuestUiTracker.TrackQuest(questId); Refresh(); }
    }
}
