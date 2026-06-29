using System.IO;
using UnityEngine;
namespace Elementborn.Game
{
    public sealed class QuestUiSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot; [SerializeField] private bool autoLoadOnStart=false; [SerializeField] private bool autoSaveOnApplicationPause=true; [SerializeField] private bool autoSaveOnApplicationQuit=true;
        private void Start(){ if(autoLoadOnStart) LoadCurrentSlot(); } private void OnApplicationPause(bool pause){ if(pause && autoSaveOnApplicationPause) SaveCurrentSlot(); } private void OnApplicationQuit(){ if(autoSaveOnApplicationQuit) SaveCurrentSlot(); }
        public void SetCurrentSlot(int slot){ currentSlot=Mathf.Max(0,slot); } public void SaveCurrentSlot()=>SaveSlot(currentSlot); public void LoadCurrentSlot()=>LoadSlot(currentSlot);
        public void SaveSlot(int slot){ var tracker=QuestUiTracker.Ensure(); var save=new QuestUiSaveFile{ SlotIndex=Mathf.Max(0,slot), TrackedQuestId=tracker.TrackedQuestId }; foreach(var q in tracker.Quests) if(q!=null) save.Quests.Add(q); File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save,true)); }
        public void LoadSlot(int slot){ string path=GetPath(slot); if(!File.Exists(path)) return; var save=JsonUtility.FromJson<QuestUiSaveFile>(File.ReadAllText(path)); if(save==null) return; QuestUiTracker.ClearAll(); var tracker=QuestUiTracker.Ensure(); foreach(var q in save.Quests) tracker.ImportRecord(q); if(!string.IsNullOrWhiteSpace(save.TrackedQuestId)) QuestUiTracker.TrackQuest(save.TrackedQuestId); }
        public static string GetPath(int slot){ string dir=Path.Combine(Application.persistentDataPath,"quest_ui"); Directory.CreateDirectory(dir); return Path.Combine(dir,$"slot_{Mathf.Max(0,slot)}_quest_ui.json"); }
    }
}
