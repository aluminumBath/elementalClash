using UnityEngine;

namespace Elementborn.Game
{
    public sealed class FireCapitalCourtInteractable : BaseInteractable
    {
        [SerializeField] private FireCapitalCourtHookDefinition hook;
        [SerializeField] private bool resolveInsteadOfStart;
        [SerializeField] private string promptPrefix = "Fire Capital";

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            string title = hook != null ? $"{promptPrefix}: {hook.Title}" : promptPrefix;
            return InteractionPromptData.Simple(title, resolveInsteadOfStart ? "Resolve" : "Start");
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

            if (resolveInsteadOfStart)
            {
                FireCapitalRegistry.Ensure().ResolveHook(hook.HookId, "Resolved through interactable.");
            }
            else
            {
                FireCapitalRegistry.Ensure().StartHook(hook.HookId);
            }
        }

        public void Configure(FireCapitalCourtHookDefinition value, bool resolve = false)
        {
            hook = value;
            resolveInsteadOfStart = resolve;
        }
    }
}
