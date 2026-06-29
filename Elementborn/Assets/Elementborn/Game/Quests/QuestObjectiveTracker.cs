using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Small code-first objective tracker that can feed the map marker system.
    /// Use it now for prototype objectives, then later connect it to the real quest system.
    /// </summary>
    public sealed class QuestObjectiveTracker : MonoBehaviour
    {
        public static QuestObjectiveTracker Instance { get; private set; }

        [SerializeField] private List<QuestObjectiveState> objectives = new List<QuestObjectiveState>();
        [SerializeField] private bool autoReportCurrentObjectiveMarker = true;

        public IReadOnlyList<QuestObjectiveState> Objectives => objectives;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static QuestObjectiveTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(QuestObjectiveTracker));
            return go.AddComponent<QuestObjectiveTracker>();
        }

        public QuestObjectiveState SetObjective(
            string questId,
            string objectiveId,
            string title,
            string description,
            Vector3 worldPosition,
            bool hasWorldPosition = true)
        {
            var objective = FindOrCreate(questId, objectiveId);
            objective.Title = string.IsNullOrWhiteSpace(title) ? "Current Objective" : title;
            objective.Description = description ?? string.Empty;
            objective.WorldPosition = worldPosition;
            objective.HasWorldPosition = hasWorldPosition;
            objective.IsComplete = false;

            if (autoReportCurrentObjectiveMarker && hasWorldPosition)
            {
                PlayerMapMarkerTracker.ReportCurrentObjective(
                    worldPosition,
                    objective.Title,
                    questId,
                    objective.Description);
            }

            return objective;
        }

        public void CompleteObjective(string questId, string objectiveId)
        {
            var objective = Find(questId, objectiveId);
            if (objective == null)
            {
                return;
            }

            objective.IsComplete = true;
            PlayerMapMarkerTracker.RemoveMarker("current_objective_" + PlayerMapMarkerTracker.SafeId(questId));
        }

        public void ClearQuest(string questId)
        {
            objectives.RemoveAll(o => o.QuestId == questId);
            PlayerMapMarkerTracker.RemoveMarker("current_objective_" + PlayerMapMarkerTracker.SafeId(questId));
        }

        public QuestObjectiveState GetCurrentObjective()
        {
            foreach (var objective in objectives)
            {
                if (objective != null && !objective.IsComplete)
                {
                    return objective;
                }
            }

            return null;
        }

        private QuestObjectiveState FindOrCreate(string questId, string objectiveId)
        {
            var existing = Find(questId, objectiveId);
            if (existing != null)
            {
                return existing;
            }

            var created = new QuestObjectiveState
            {
                QuestId = questId ?? string.Empty,
                ObjectiveId = objectiveId ?? string.Empty
            };

            objectives.Add(created);
            return created;
        }

        private QuestObjectiveState Find(string questId, string objectiveId)
        {
            return objectives.Find(o => o.QuestId == questId && o.ObjectiveId == objectiveId);
        }
    }
}
