using UnityEngine;

namespace Elementborn.Game
{
    public sealed class BossArenaController : MonoBehaviour
    {
        [SerializeField] private string arenaId = ""; [SerializeField] private BossArenaState state = BossArenaState.Inactive; [SerializeField] private GameObject[] sealObjects; [SerializeField] private GameObject[] hazardObjects;
        public string ArenaId => string.IsNullOrWhiteSpace(arenaId) ? gameObject.name : arenaId; public BossArenaState State => state;
        private void Awake(){ SetSeals(false); SetHazardsActive(false); }
        public void BeginArena(BossController boss){ state=BossArenaState.Sealing; SetSeals(true); SetHazardsActive(false); state=BossArenaState.Active; NotificationFeed.Post("The arena seals shut!", NotificationType.Combat); }
        public void CompleteArena(){ state=BossArenaState.Cleared; SetHazardsActive(false); SetSeals(false); NotificationFeed.Post("The arena opens.", NotificationType.Combat); }
        public void ResetArena(){ state=BossArenaState.Resetting; SetHazardsActive(false); SetSeals(false); state=BossArenaState.Inactive; }
        public void SetHazardsActive(bool active){ if(hazardObjects==null) return; foreach(var hazard in hazardObjects) if(hazard!=null) hazard.SetActive(active); }
        private void SetSeals(bool active){ if(sealObjects==null) return; foreach(var seal in sealObjects) if(seal!=null) seal.SetActive(active); }
    }
}
