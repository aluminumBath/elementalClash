using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class BossPhaseBannerView : MonoBehaviour
    {
        [SerializeField] private GameObject root; [SerializeField] private Text phaseText; [SerializeField] private Image phaseIcon; [SerializeField] private float visibleSeconds = 2.5f; private float hideAt;
        private void OnEnable(){ BossEventHub.BossPhaseChanged += HandlePhaseChanged; } private void OnDisable(){ BossEventHub.BossPhaseChanged -= HandlePhaseChanged; }
        private void Update(){ if(root!=null && root.activeSelf && Time.unscaledTime>=hideAt) root.SetActive(false); }
        private void HandlePhaseChanged(BossController boss,int phaseIndex){ if(boss==null || boss.Definition==null || phaseIndex<0 || phaseIndex>=boss.Definition.Phases.Count) return; var phase=boss.Definition.Phases[phaseIndex]; if(root!=null) root.SetActive(true); if(phaseText!=null) phaseText.text=!string.IsNullOrWhiteSpace(phase.PhaseAnnouncement)?phase.PhaseAnnouncement:phase.PhaseName; if(phaseIcon!=null){ phaseIcon.sprite=phase.PhaseIcon; phaseIcon.enabled=phaseIcon.sprite!=null;} hideAt=Time.unscaledTime+Mathf.Max(0.25f,visibleSeconds); }
    }
}
