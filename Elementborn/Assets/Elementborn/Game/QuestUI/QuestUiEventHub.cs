using System;
namespace Elementborn.Game
{
    public static class QuestUiEventHub
    {
        public static event Action<QuestUiRecord> QuestStarted; public static event Action<QuestUiRecord> QuestUpdated;
        public static event Action<QuestUiRecord, QuestObjectiveUiRecord> ObjectiveCompleted; public static event Action<QuestUiRecord> QuestCompleted; public static event Action<QuestUiRecord> QuestTrackedChanged;
        public static void RaiseQuestStarted(QuestUiRecord q)=>QuestStarted?.Invoke(q); public static void RaiseQuestUpdated(QuestUiRecord q)=>QuestUpdated?.Invoke(q);
        public static void RaiseObjectiveCompleted(QuestUiRecord q, QuestObjectiveUiRecord o)=>ObjectiveCompleted?.Invoke(q,o); public static void RaiseQuestCompleted(QuestUiRecord q)=>QuestCompleted?.Invoke(q); public static void RaiseQuestTrackedChanged(QuestUiRecord q)=>QuestTrackedChanged?.Invoke(q);
    }
}
