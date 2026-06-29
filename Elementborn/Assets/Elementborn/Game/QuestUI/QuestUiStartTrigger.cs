using UnityEngine;
namespace Elementborn.Game
{
    [RequireComponent(typeof(Collider))]
    public sealed class QuestUiStartTrigger : MonoBehaviour
    {
        [SerializeField] private QuestUiDefinition quest; [SerializeField] private bool triggerOnce=true; [SerializeField] private bool used;
        private void Reset(){ GetComponent<Collider>().isTrigger=true; }
        private void OnTriggerEnter(Collider other){ if(used && triggerOnce) return; if(!other.CompareTag("Player")) return; used=true; QuestUiTracker.StartQuest(quest); }
    }
}
