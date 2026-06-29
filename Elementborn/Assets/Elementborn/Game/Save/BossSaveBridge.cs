using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class BossSaveBridge : MonoBehaviour
    {
        [SerializeField] private int currentSlot; [SerializeField] private bool autoLoadOnStart=false; [SerializeField] private bool autoSaveOnApplicationPause=true; [SerializeField] private bool autoSaveOnApplicationQuit=true;
        private void Start(){ if(autoLoadOnStart) LoadCurrentSlot(); } private void OnApplicationPause(bool pause){ if(pause && autoSaveOnApplicationPause) SaveCurrentSlot(); } private void OnApplicationQuit(){ if(autoSaveOnApplicationQuit) SaveCurrentSlot(); }
        public void SetCurrentSlot(int slot){ currentSlot=Mathf.Max(0,slot); } public void SaveCurrentSlot(){ SaveSlot(currentSlot); } public void LoadCurrentSlot(){ LoadSlot(currentSlot); }
        public void SaveSlot(int slot){ var save=new BossSaveFile{SlotIndex=Mathf.Max(0,slot)}; foreach(var boss in ElementbornFindUtility.FindAll<BossController>()) if(boss!=null) save.Bosses.Add(boss.ToRecord()); File.WriteAllText(GetPath(slot), JsonUtility.ToJson(save,true)); }
        public void LoadSlot(int slot){ string path=GetPath(slot); if(!File.Exists(path)) return; var save=JsonUtility.FromJson<BossSaveFile>(File.ReadAllText(path)); if(save==null) return; foreach(var boss in ElementbornFindUtility.FindAll<BossController>()){ if(boss==null) continue; var record=save.Bosses.Find(b=>b.BossId==boss.BossId); if(record!=null) boss.Import(record); }}
        public static string GetPath(int slot){ string dir=Path.Combine(Application.persistentDataPath, "bosses"); Directory.CreateDirectory(dir); return Path.Combine(dir,$"slot_{Mathf.Max(0,slot)}_bosses.json"); }
    }
}
