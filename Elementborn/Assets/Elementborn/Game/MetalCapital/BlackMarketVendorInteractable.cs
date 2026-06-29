using UnityEngine;

namespace Elementborn.Game
{
    public sealed class BlackMarketVendorInteractable : BaseInteractable
    {
        [SerializeField] private MetalCapitalContactDefinition contact;
        [SerializeField] private string prompt = "Browse black market goods";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = contact != null ? $"{prompt}: {contact.DisplayName}" : prompt;
            return InteractionPromptData.Simple(title, "Browse");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return contact != null;
        }

        public override void Interact(GameObject interactor)
        {
            if (contact == null)
            {
                return;
            }

            ElementbornAudioService.PlayAt(ElementbornSoundEventId.UiConfirm, transform.position);
            NotificationFeed.Post($"{contact.DisplayName}: {BuildListingSummary()}", NotificationType.Info);

            PlayerJournalTracker.AddOrUpdateEntry(
                "black_market_" + PlayerJournalTracker.Safe(contact.ContactId),
                JournalEntryType.Character,
                contact.DisplayName,
                BuildListingSummary(),
                "Metal Capital",
                contact.ContactId);
        }

        public void SetContact(MetalCapitalContactDefinition value)
        {
            contact = value;
        }

        private string BuildListingSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(contact.Description);
            foreach (var listing in contact.Listings)
            {
                if (listing == null)
                {
                    continue;
                }

                sb.AppendLine($"- {listing.DisplayName}: {listing.Price} coins [{listing.RiskLevel}]");
                if (!string.IsNullOrWhiteSpace(listing.Rumor))
                {
                    sb.AppendLine($"  Rumor: {listing.Rumor}");
                }
            }

            return sb.ToString();
        }
    }
}
