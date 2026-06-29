using UnityEngine;

namespace Elementborn.Game
{
    public sealed class MetalCapitalIntrigueInteractable : BaseInteractable
    {
        [SerializeField] private MetalCapitalIntrigueHookDefinition hook;
        [SerializeField] private string prompt = "Investigate";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = hook != null ? $"{prompt}: {hook.Title}" : prompt;
            return InteractionPromptData.Simple(title, "Investigate");
        }

        public override bool CanInteract(GameObject interactor)
        {
            return hook != null;
        }

        public override void Interact(GameObject interactor)
        {
            if (hook == null)
            {
                return;
            }

            ElementbornAudioService.PlayAt(ElementbornSoundEventId.UiConfirm, transform.position);
            NotificationFeed.Post(hook.PlayerFacingRumor, NotificationType.Quest);

            if (hook.Quest != null)
            {
                QuestUiTracker.StartQuest(hook.Quest);
            }

            if (hook.ThievesGuildReputationDelta != 0)
            {
                ThievesGuildReputationTracker.Ensure().AddReputation(hook.ThievesGuildReputationDelta, hook.Title);
            }

            PlayerJournalTracker.AddOrUpdateEntry(
                "metal_intrigue_" + PlayerJournalTracker.Safe(hook.HookId),
                JournalEntryType.Quest,
                hook.Title,
                hook.Summary + "\n\nRumor: " + hook.PlayerFacingRumor,
                "Metal Capital",
                hook.HookId);
        }

        public void SetHook(MetalCapitalIntrigueHookDefinition value)
        {
            hook = value;
        }
    }
}
