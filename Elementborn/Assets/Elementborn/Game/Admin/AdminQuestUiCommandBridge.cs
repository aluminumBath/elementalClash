using UnityEngine;
namespace Elementborn.Game
{
    public sealed class AdminQuestUiCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if(string.IsNullOrWhiteSpace(command)) return false; string trimmed=command.Trim();
            if(trimmed.StartsWith("questui.start ")){ string[] p=trimmed.Substring("questui.start ".Length).Split('|'); string id=p.Length>0?p[0].Trim():"admin_quest"; string title=p.Length>1?p[1].Trim():id; string desc=p.Length>2?p[2].Trim():""; float x=p.Length>3&&float.TryParse(p[3],out float px)?px:transform.position.x; float y=p.Length>4&&float.TryParse(p[4],out float py)?py:transform.position.y; float z=p.Length>5&&float.TryParse(p[5],out float pz)?pz:transform.position.z; QuestUiTracker.StartRuntimeQuest(id,title,desc,"Admin","Admin",new Vector3(x,y,z),"Go to objective"); return true; }
            if(trimmed.StartsWith("questui.track ")){ QuestUiTracker.TrackQuest(trimmed.Substring("questui.track ".Length).Trim()); return true; }
            if(trimmed.StartsWith("questui.complete.objective ")){ string[] p=trimmed.Substring("questui.complete.objective ".Length).Split('|'); QuestUiTracker.CompleteObjective(p.Length>0?p[0].Trim():"", p.Length>1?p[1].Trim():""); return true; }
            if(trimmed.StartsWith("questui.complete ")){ QuestUiTracker.CompleteQuest(trimmed.Substring("questui.complete ".Length).Trim()); return true; }
            if(trimmed=="questui.clear"){ QuestUiTracker.ClearAll(); return true; }
            if(trimmed=="questui.list"){ foreach(var q in QuestUiTracker.Ensure().Quests) if(q!=null) Debug.Log($"{q.QuestId}: {q.Title} [{q.State}] tracked={q.Tracked}"); return true; }
            return false;
        }
    }
}
