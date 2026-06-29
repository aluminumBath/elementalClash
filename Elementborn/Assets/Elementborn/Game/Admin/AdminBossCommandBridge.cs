using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AdminBossCommandBridge : MonoBehaviour
    {
        [SerializeField] private BossController boss; [SerializeField] private BossArenaController arena;
        public bool ExecuteCommand(string command){ if(string.IsNullOrWhiteSpace(command)) return false; ResolveTargets(); string t=command.Trim(); if(t=="boss.start"){ if(boss!=null) boss.StartBoss(); return true;} if(t=="boss.reset"){ if(boss!=null) boss.ResetBoss(); return true;} if(t.StartsWith("boss.phase ")){ if(boss!=null && int.TryParse(t.Substring(11), out int p)) boss.ForcePhase(Mathf.Max(0,p)); return true;} if(t.StartsWith("boss.damage ")){ if(boss!=null && float.TryParse(t.Substring(12), out float a)){ var h=boss.GetComponent<SimpleCombatHealth>(); if(h!=null) h.ApplyDamage(a);} return true;} if(t=="boss.kill"){ if(boss!=null){ var h=boss.GetComponent<SimpleCombatHealth>(); if(h!=null) h.ApplyDamage(h.CurrentHealth+99999f);} return true;} if(t=="boss.state"){ if(boss!=null) Debug.Log($"Boss={boss.DisplayName}, State={boss.State}, Phase={boss.CurrentPhaseIndex}, HP={boss.CurrentHealth}/{boss.MaxHealth}"); return true;} if(t=="boss.hazards.on"){ if(arena!=null) arena.SetHazardsActive(true); return true;} if(t=="boss.hazards.off"){ if(arena!=null) arena.SetHazardsActive(false); return true;} return false; }
        private void ResolveTargets(){ if(boss==null) boss=GetComponent<BossController>(); if(arena==null) arena=GetComponent<BossArenaController>(); if(arena==null && boss!=null) arena=boss.GetComponentInParent<BossArenaController>(); }
    }
}
