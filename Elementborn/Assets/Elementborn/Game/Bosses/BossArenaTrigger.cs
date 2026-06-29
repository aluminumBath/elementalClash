using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(Collider))]
    public sealed class BossArenaTrigger : MonoBehaviour
    {
        [SerializeField] private BossController boss; [SerializeField] private bool triggerOnce = true; private bool used;
        private void Reset(){ GetComponent<Collider>().isTrigger = true; }
        private void OnTriggerEnter(Collider other){ if(used && triggerOnce) return; if(!other.CompareTag("Player")) return; if(boss==null) return; used=true; boss.StartBoss(); }

        public void Configure(BossController value)
        {
            boss = value;
        }

    }
}
