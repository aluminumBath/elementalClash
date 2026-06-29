using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class BossDebugPanel : MonoBehaviour
    {
        [SerializeField] private Text text; [SerializeField] private BossController boss; [SerializeField] private bool refreshEveryFrame = true; private void Reset(){ text=GetComponentInChildren<Text>(); } private void Update(){ if(refreshEveryFrame) Refresh(); }
        public void Refresh(){ if(text==null) return; if(boss==null){ text.text="No BossController assigned."; return; } text.text=$"Boss: {boss.DisplayName}\nState: {boss.State}\nPhase: {boss.CurrentPhaseIndex+1}\nHP: {boss.CurrentHealth:0.#}/{boss.MaxHealth:0.#}"; }
    }
}
