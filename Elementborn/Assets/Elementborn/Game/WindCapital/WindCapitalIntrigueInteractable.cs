using UnityEngine;

namespace Elementborn.Game
{
    public sealed class WindCapitalIntrigueInteractable : BaseInteractable
    {
        [SerializeField] private WindCapitalIntrigueHookDefinition hook;
        [SerializeField] private string prompt = "Investigate wind-capital rumor";
        [SerializeField] private bool revealSecretOnInteract = false;

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
            NotificationFeed.Post(hook.Summary, NotificationType.Quest);

            if (hook.Quest != null)
            {
                QuestUiTracker.StartQuest(hook.Quest);
            }

            if (hook.FervorDelta != 0)
            {
                ReligiousFervorTracker.Ensure().AddFervor(hook.FervorDelta, hook.Title);
            }

            if (revealSecretOnInteract)
            {
                WindCapitalSecretTracker.Ensure().Reveal(hook);
            }

            PlayerJournalTracker.AddOrUpdateEntry(
                "wind_intrigue_" + PlayerJournalTracker.Safe(hook.HookId),
                JournalEntryType.Quest,
                hook.Title,
                hook.Summary,
                "Wind Capital",
                hook.HookId);
        }

        public void SetHook(WindCapitalIntrigueHookDefinition value)
        {
            hook = value;
        }
    }
}
