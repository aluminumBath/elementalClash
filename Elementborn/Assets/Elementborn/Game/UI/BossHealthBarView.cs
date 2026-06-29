using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class BossHealthBarView : MonoBehaviour
    {
        [SerializeField] private BossController boss; [SerializeField] private GameObject root; [SerializeField] private Slider healthSlider; [SerializeField] private Text nameText; [SerializeField] private Text phaseText; [SerializeField] private Image icon;
        private void OnEnable(){ BossEventHub.BossStarted += HandleBossStarted; BossEventHub.BossPhaseChanged += HandlePhaseChanged; BossEventHub.BossDefeated += HandleBossEnded; BossEventHub.BossReset += HandleBossEnded; }
        private void OnDisable(){ BossEventHub.BossStarted -= HandleBossStarted; BossEventHub.BossPhaseChanged -= HandlePhaseChanged; BossEventHub.BossDefeated -= HandleBossEnded; BossEventHub.BossReset -= HandleBossEnded; }
        private void Update(){ Refresh(); }
        public void SetBoss(BossController value){ boss=value; Refresh(); }
        public void Refresh(){ bool active=boss!=null && boss.State!=BossState.Dormant && boss.State!=BossState.Defeated; if(root!=null) root.SetActive(active); if(!active) return; if(healthSlider!=null) healthSlider.value=boss.Health01; if(nameText!=null) nameText.text=boss.DisplayName; if(phaseText!=null) phaseText.text=boss.CurrentPhaseIndex>=0 ? $"Phase {boss.CurrentPhaseIndex+1}" : ""; if(icon!=null){ icon.sprite=boss.Definition!=null?boss.Definition.Icon:null; icon.enabled=icon.sprite!=null; }}
        private void HandleBossStarted(BossController b){ SetBoss(b); } private void HandlePhaseChanged(BossController b,int p){ SetBoss(b); } private void HandleBossEnded(BossController b){ if(boss==b) Refresh(); }
    }
}
