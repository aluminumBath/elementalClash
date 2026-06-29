using System;

namespace Elementborn.Game
{
    public static class QuestChainEventHub
    {
        public static event Action<QuestChainDefinition> ChainStarted;
        public static event Action<QuestChainDefinition, QuestChainStageDefinition> StageStarted;
        public static event Action<QuestChainDefinition, QuestChainStageDefinition> StageCompleted;
        public static event Action<QuestChainDefinition, QuestChainStageDefinition, QuestChainChoiceDefinition> ChoiceApplied;
        public static event Action<QuestChainDefinition> ChainCompleted;

        public static void RaiseChainStarted(QuestChainDefinition chain) => ChainStarted?.Invoke(chain);
        public static void RaiseStageStarted(QuestChainDefinition chain, QuestChainStageDefinition stage) => StageStarted?.Invoke(chain, stage);
        public static void RaiseStageCompleted(QuestChainDefinition chain, QuestChainStageDefinition stage) => StageCompleted?.Invoke(chain, stage);
        public static void RaiseChoiceApplied(QuestChainDefinition chain, QuestChainStageDefinition stage, QuestChainChoiceDefinition choice) => ChoiceApplied?.Invoke(chain, stage, choice);
        public static void RaiseChainCompleted(QuestChainDefinition chain) => ChainCompleted?.Invoke(chain);
    }
}
