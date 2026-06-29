using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Drop this into prototype scenes to create/update a quest objective when the player enters a trigger.
    /// Useful for rapidly wiring "go here next" behavior before the full quest editor exists.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class QuestObjectiveTrigger : MonoBehaviour
    {
        [SerializeField] private string questId = "prototype_quest";
        [SerializeField] private string objectiveId = "objective";
        [SerializeField] private string title = "Current Objective";
        [TextArea]
        [SerializeField] private string description = "";
        [SerializeField] private Transform objectiveLocation;
        [SerializeField] private bool completeInsteadOfSet = false;
        [SerializeField] private bool once = true;

        private bool used;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (once && used)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            used = true;

            var tracker = QuestObjectiveTracker.Ensure();
            if (completeInsteadOfSet)
            {
                tracker.CompleteObjective(questId, objectiveId);
            }
            else
            {
                Vector3 position = objectiveLocation != null ? objectiveLocation.position : transform.position;
                tracker.SetObjective(questId, objectiveId, title, description, position);
            }
        }
    }
}
