using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class QuestChainConsequenceDefinition
    {
        public QuestChainConsequenceType ConsequenceType = QuestChainConsequenceType.CapitalPressure;
        public CapitalId Capital = CapitalId.Unknown;
        public CapitalPressureType PressureType = CapitalPressureType.Unrest;
        public int Amount = 0;
        public QuestUiDefinition QuestToStart;
        public PoliticalWorldEventDefinition PoliticalEvent;
        public WindCapitalIntrigueHookDefinition WindSecret;
        public NpcWorldEntryDefinition MetalSecretNpc;
        public string ShipId = "";
        public string JournalId = "";
        public string JournalTitle = "";
        [TextArea]
        public string Notes = "";

        public void Apply()
        {
            switch (ConsequenceType)
            {
                case QuestChainConsequenceType.CapitalPressure:
                    CapitalWorldStateTracker.Ensure().AddPressure(Capital, PressureType, Amount, Notes);
                    break;
                case QuestChainConsequenceType.CapitalLegitimacy:
                    CapitalWorldStateTracker.Ensure().AddLegitimacy(Capital, Amount, Notes);
                    break;
                case QuestChainConsequenceType.CapitalStability:
                    CapitalWorldStateTracker.Ensure().AddStability(Capital, Amount, Notes);
                    break;
                case QuestChainConsequenceType.StartQuest:
                    if (QuestToStart != null)
                    {
                        QuestUiTracker.StartQuest(QuestToStart);
                    }
                    break;
                case QuestChainConsequenceType.ActivateWorldEvent:
                    if (PoliticalEvent != null)
                    {
                        PoliticalWorldEventDirector.Ensure().Activate(PoliticalEvent.EventId, Notes);
                    }
                    break;
                case QuestChainConsequenceType.RevealWindSecret:
                    if (WindSecret != null)
                    {
                        WindCapitalSecretTracker.Ensure().Reveal(WindSecret);
                    }
                    break;
                case QuestChainConsequenceType.RevealMetalSecret:
                    HiddenChannelerSecretTracker.Ensure().RevealSecret(MetalSecretNpc, Notes);
                    break;
                case QuestChainConsequenceType.AddThievesGuildReputation:
                    ThievesGuildReputationTracker.Ensure().AddReputation(Amount, Notes);
                    break;
                case QuestChainConsequenceType.AddReligiousFervor:
                    ReligiousFervorTracker.Ensure().AddFervor(Amount, Notes);
                    break;
                case QuestChainConsequenceType.AddShipReputation:
                    ShipReputationTracker.Ensure().AddReputation(ShipId, Amount);
                    break;
                case QuestChainConsequenceType.JournalEntry:
                    PlayerJournalTracker.AddOrUpdateEntry(
                        string.IsNullOrWhiteSpace(JournalId) ? "quest_chain_note" : JournalId,
                        JournalEntryType.Quest,
                        string.IsNullOrWhiteSpace(JournalTitle) ? "Quest Chain Note" : JournalTitle,
                        Notes,
                        Capital.ToString(),
                        JournalId);
                    break;
            }
        }
    }
}
