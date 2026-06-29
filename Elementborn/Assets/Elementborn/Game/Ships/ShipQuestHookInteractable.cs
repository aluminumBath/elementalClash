using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShipQuestHookInteractable : BaseInteractable
    {
        [SerializeField] private NamedShipDefinition ship;
        [SerializeField] private QuestUiDefinition quest;
        [SerializeField] private string prompt = "Check ship rumors";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = ship != null ? $"{prompt}: {ship.DisplayName}" : prompt;
            return InteractionPromptData.Simple(title, "Check");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return ship != null;
        }

        public override void Interact(GameObject interactor)
        {
            if (quest != null)
            {
                QuestUiTracker.StartQuest(quest);
            }

            if (ship != null)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "ship_hook_" + PlayerJournalTracker.Safe(ship.ShipId),
                    JournalEntryType.Quest,
                    ship.DisplayName + " Rumor",
                    ship.RaidStyle + "\n\n" + ship.CelebrationStyle,
                    ship.Region,
                    ship.ShipId);

                ElementbornAudioService.PlayAt(ElementbornSoundEventId.UiConfirm, transform.position);
            }
        }

        public void SetShip(NamedShipDefinition value)
        {
            ship = value;
        }

        public void SetQuest(QuestUiDefinition value)
        {
            quest = value;
        }
    }
}
