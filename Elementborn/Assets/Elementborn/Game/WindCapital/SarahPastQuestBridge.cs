using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SarahPastQuestBridge : MonoBehaviour
    {
        [SerializeField] private NpcWorldEntryDefinition sarah;
        [SerializeField] private NpcWorldEntryDefinition ramon;
        [SerializeField] private QuestUiDefinition quest;
        [SerializeField] private string prompt = "Ask about Sarah's past";

        public void StartSarahPastQuest()
        {
            if (quest != null)
            {
                QuestUiTracker.StartQuest(quest);
            }

            if (sarah != null)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "sarah_past_thread",
                    JournalEntryType.Character,
                    sarah.DisplayName + " — The Wind She Won't Name",
                    "Sarah refuses to speak of the Wind Capital. Ramón knows what happened.",
                    "Wind Capital",
                    sarah.NpcId);
            }

            ElementbornAudioService.Play(ElementbornSoundEventId.QuestStart);
        }

        public void RevealSarahSecret(WindCapitalIntrigueHookDefinition hook)
        {
            WindCapitalSecretTracker.Ensure().Reveal(hook);
        }

        public void SetData(NpcWorldEntryDefinition sarahNpc, NpcWorldEntryDefinition ramonNpc, QuestUiDefinition questDefinition)
        {
            sarah = sarahNpc;
            ramon = ramonNpc;
            quest = questDefinition;
        }
    }
}
