using UnityEngine;
namespace Elementborn.Game
{
    [RequireComponent(typeof(Collider))]
    public sealed class QuestUiCompletionTrigger : MonoBehaviour
    {
        [SerializeField] private string questId=""; [SerializeField] private string objectiveId=""; [SerializeField] private bool completeWholeQuest=false; [SerializeField] private bool triggerOnce=true; [SerializeField] private bool used;
        private void Reset(){ GetComponent<Collider>().isTrigger=true; }
        private void OnTriggerEnter(Collider other){ if(used && triggerOnce) return; if(!other.CompareTag("Player")) return; used=true; if(completeWholeQuest) QuestUiTracker.CompleteQuest(questId); else QuestUiTracker.CompleteObjective(questId, objectiveId); }
    }
}
