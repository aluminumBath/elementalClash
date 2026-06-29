using UnityEngine;
using UnityEngine.UI;
namespace Elementborn.Game
{
    public sealed class QuestRewardPreviewView : MonoBehaviour
    {
        [SerializeField] private Text text; [SerializeField] private QuestUiDefinition previewQuest;
        private void Start(){ Refresh(); } public void SetQuest(QuestUiDefinition quest){ previewQuest=quest; Refresh(); }
        [ContextMenu("Refresh")]
        public void Refresh(){ if(text==null) return; if(previewQuest==null){ text.text="No quest selected."; return; } var sb=new System.Text.StringBuilder(); sb.AppendLine($"{previewQuest.Title} Rewards"); foreach(var r in previewQuest.Rewards){ if(r==null) continue; if(r.Currency>0) sb.AppendLine($"- {r.Currency} coins"); if(r.SkillPoints>0) sb.AppendLine($"- {r.SkillPoints} skill point(s)"); if(!string.IsNullOrWhiteSpace(r.ItemId)) sb.AppendLine($"- {r.DisplayName} x{Mathf.Max(1,r.Quantity)}"); } text.text=sb.ToString(); }
    }
}
