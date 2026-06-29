using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ShipRaidCelebrationController : MonoBehaviour
    {
        [SerializeField] private NamedShipDefinition ship;
        [SerializeField] private bool playSound = true;
        [SerializeField] private bool postNotification = true;
        [SerializeField] private bool addJournalEntry = true;

        public void TriggerRaidVictory()
        {
            if (ship == null)
            {
                return;
            }

            ShipReputationTracker.Ensure().RecordRaid(ship.ShipId, true);
            TriggerCelebration();
        }

        public void TriggerCelebration()
        {
            if (ship == null)
            {
                return;
            }

            ShipReputationTracker.Ensure().RecordCelebration(ship.ShipId);

            if (playSound)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.QuestComplete, ship.WorldPosition);
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.BoatWaveCreak, ship.WorldPosition);
            }

            if (postNotification)
            {
                NotificationFeed.Post($"{ship.DisplayName} erupts into a lavish celebration!", NotificationType.Info);
            }

            if (addJournalEntry)
            {
                PlayerJournalTracker.AddOrUpdateEntry(
                    "ship_party_" + PlayerJournalTracker.Safe(ship.ShipId),
                    JournalEntryType.Location,
                    ship.DisplayName + " Celebration",
                    ship.CelebrationStyle,
                    ship.Region,
                    ship.ShipId);
            }
        }

        public void SetShip(NamedShipDefinition value)
        {
            ship = value;
        }
    }
}
