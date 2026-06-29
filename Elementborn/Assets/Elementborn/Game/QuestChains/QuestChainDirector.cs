using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class QuestChainDirector : MonoBehaviour
    {
        public static QuestChainDirector Instance { get; private set; }

        [SerializeField] private List<QuestChainDefinition> questChains = new List<QuestChainDefinition>();
        [SerializeField] private List<QuestChainRuntimeRecord> runtimeRecords = new List<QuestChainRuntimeRecord>();
        [SerializeField] private bool listenToPoliticalEvents = true;

        public IReadOnlyList<QuestChainDefinition> QuestChains => questChains;
        public IReadOnlyList<QuestChainRuntimeRecord> RuntimeRecords => runtimeRecords;

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

        private void OnEnable()
        {
            if (listenToPoliticalEvents)
            {
                PoliticalWorldEventHub.EventActivated += HandlePoliticalEventActivated;
            }
        }

        private void OnDisable()
        {
            PoliticalWorldEventHub.EventActivated -= HandlePoliticalEventActivated;
        }

        public static QuestChainDirector Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(QuestChainDirector));
            return go.AddComponent<QuestChainDirector>();
        }

        public void SetQuestChains(List<QuestChainDefinition> chains)
        {
            questChains = chains ?? new List<QuestChainDefinition>();
        }

        public void ClearRuntimeRecords()
        {
            runtimeRecords.Clear();
        }

        public void ImportRuntimeRecord(QuestChainRuntimeRecord record)
        {
            if (record == null)
            {
                return;
            }

            QuestChainRuntimeRecord existing = runtimeRecords.Find(r => r != null && r.ChainId == record.ChainId);
            if (existing != null)
            {
                runtimeRecords.Remove(existing);
            }

            runtimeRecords.Add(record);
        }

        public void ReplaceRuntimeRecords(List<QuestChainRuntimeRecord> records)
        {
            runtimeRecords.Clear();
            if (records == null)
            {
                return;
            }

            foreach (QuestChainRuntimeRecord record in records)
            {
                ImportRuntimeRecord(record);
            }
        }


        public QuestChainDefinition FindChain(string chainId)
        {
            return questChains.Find(c => c != null && c.ChainId == chainId);
        }

        public QuestChainRuntimeRecord GetOrCreateRecord(string chainId)
        {
            QuestChainRuntimeRecord record = runtimeRecords.Find(r => r != null && r.ChainId == chainId);
            if (record != null)
            {
                return record;
            }

            record = new QuestChainRuntimeRecord { ChainId = chainId };
            runtimeRecords.Add(record);
            return record;
        }

        public bool StartChain(string chainId)
        {
            QuestChainDefinition chain = FindChain(chainId);
            if (chain == null)
            {
                Debug.LogWarning($"Quest chain not found: {chainId}");
                return false;
            }

            QuestChainRuntimeRecord record = GetOrCreateRecord(chain.ChainId);
            if (!record.Started)
            {
                record.Started = true;
                QuestChainEventHub.RaiseChainStarted(chain);
                NotificationFeed.Post($"Quest chain started: {chain.DisplayName}", NotificationType.Quest);
            }

            QuestChainStageDefinition first = chain.FirstStage;
            if (first != null && string.IsNullOrWhiteSpace(record.ActiveStageId))
            {
                StartStage(chain, first.StageId);
            }

            return true;
        }

        public bool StartStage(string chainId, string stageId)
        {
            QuestChainDefinition chain = FindChain(chainId);
            return chain != null && StartStage(chain, stageId);
        }

        public bool StartStage(QuestChainDefinition chain, string stageId)
        {
            QuestChainStageDefinition stage = chain.FindStage(stageId);
            if (stage == null)
            {
                Debug.LogWarning($"Quest chain stage not found: {chain.ChainId}/{stageId}");
                return false;
            }

            QuestChainRuntimeRecord record = GetOrCreateRecord(chain.ChainId);
            record.Started = true;
            record.ActiveStageId = stage.StageId;

            QuestChainStageRuntimeRecord stageRecord = record.GetOrCreateStage(stage.StageId);
            stageRecord.State = QuestChainStageState.Active;

            ApplyConsequences(stage.OnStartConsequences);

            if (stage.Quest != null && stage.AutoStartQuest)
            {
                QuestUiTracker.StartQuest(stage.Quest);
            }

            PlayerJournalTracker.AddOrUpdateEntry(
                "quest_chain_" + PlayerJournalTracker.Safe(chain.ChainId),
                JournalEntryType.Quest,
                chain.DisplayName + " — " + stage.Title,
                chain.Summary + "\n\n" + stage.Summary,
                chain.PrimaryCapital.ToString(),
                chain.ChainId);

            QuestChainEventHub.RaiseStageStarted(chain, stage);
            NotificationFeed.Post($"Quest chain stage started: {stage.Title}", NotificationType.Quest);
            return true;
        }

        public bool CompleteStage(string chainId, string stageId)
        {
            QuestChainDefinition chain = FindChain(chainId);
            if (chain == null)
            {
                return false;
            }

            QuestChainStageDefinition stage = chain.FindStage(stageId);
            if (stage == null)
            {
                return false;
            }

            QuestChainRuntimeRecord record = GetOrCreateRecord(chain.ChainId);
            QuestChainStageRuntimeRecord stageRecord = record.GetOrCreateStage(stage.StageId);
            stageRecord.State = QuestChainStageState.Completed;

            ApplyConsequences(stage.OnCompleteConsequences);
            QuestChainEventHub.RaiseStageCompleted(chain, stage);
            NotificationFeed.Post($"Quest chain stage completed: {stage.Title}", NotificationType.Info);

            if (!string.IsNullOrWhiteSpace(stage.DefaultNextStageId))
            {
                StartStage(chain, stage.DefaultNextStageId);
            }
            else
            {
                record.Completed = true;
                record.ActiveStageId = "";
                QuestChainEventHub.RaiseChainCompleted(chain);
                NotificationFeed.Post($"Quest chain completed: {chain.DisplayName}", NotificationType.Quest);
            }

            return true;
        }

        public bool ApplyChoice(string chainId, string stageId, string choiceId)
        {
            QuestChainDefinition chain = FindChain(chainId);
            if (chain == null)
            {
                return false;
            }

            QuestChainStageDefinition stage = chain.FindStage(stageId);
            if (stage == null)
            {
                return false;
            }

            QuestChainChoiceDefinition choice = stage.Choices.Find(c => c != null && c.ChoiceId == choiceId);
            if (choice == null)
            {
                Debug.LogWarning($"Quest chain choice not found: {chainId}/{stageId}/{choiceId}");
                return false;
            }

            QuestChainRuntimeRecord record = GetOrCreateRecord(chain.ChainId);
            QuestChainStageRuntimeRecord stageRecord = record.GetOrCreateStage(stage.StageId);
            stageRecord.ChoiceId = choice.ChoiceId;

            ApplyConsequences(choice.Consequences);

            PlayerJournalTracker.AddOrUpdateEntry(
                "quest_chain_choice_" + PlayerJournalTracker.Safe(chain.ChainId + "_" + stage.StageId),
                JournalEntryType.Quest,
                choice.DisplayText,
                choice.PlayerFacingResult + "\n\nDirector notes: " + choice.HiddenDirectorNotes,
                chain.PrimaryCapital.ToString(),
                choice.ChoiceId);

            QuestChainEventHub.RaiseChoiceApplied(chain, stage, choice);
            NotificationFeed.Post(choice.PlayerFacingResult, NotificationType.Quest);

            if (!string.IsNullOrWhiteSpace(choice.NextStageId))
            {
                CompleteStage(chainId, stageId);
                StartStage(chain, choice.NextStageId);
            }

            return true;
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Quest Chain Director");
            foreach (QuestChainDefinition chain in questChains)
            {
                if (chain == null)
                {
                    continue;
                }

                QuestChainRuntimeRecord record = GetOrCreateRecord(chain.ChainId);
                sb.AppendLine($"- {chain.DisplayName}: started={record.Started}, completed={record.Completed}, active={record.ActiveStageId}");
            }

            return sb.ToString();
        }

        private void ApplyConsequences(List<QuestChainConsequenceDefinition> consequences)
        {
            if (consequences == null)
            {
                return;
            }

            foreach (QuestChainConsequenceDefinition consequence in consequences)
            {
                consequence?.Apply();
            }
        }

        private void HandlePoliticalEventActivated(PoliticalWorldEventDefinition evt)
        {
            if (evt == null)
            {
                return;
            }

            foreach (QuestChainDefinition chain in questChains)
            {
                if (chain == null || !chain.AutoStartOnEvent)
                {
                    continue;
                }

                foreach (QuestChainStageDefinition stage in chain.Stages)
                {
                    if (stage != null && stage.TriggeredByPoliticalEvent == evt)
                    {
                        StartChain(chain.ChainId);
                        StartStage(chain, stage.StageId);
                        return;
                    }
                }
            }
        }
    }
}
