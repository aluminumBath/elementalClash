using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Generic interactable base. Use it directly for UnityEvent-driven interactions,
    /// or inherit from it for boats, NPCs, chests, doors, shrines, etc.
    /// </summary>
    public class BaseInteractable : MonoBehaviour, IInteractable
    {
        [Header("Prompt")]
        [SerializeField] private string title = "Interact";
        [SerializeField] private string actionText = "Interact";
        [SerializeField] private Sprite icon;
        [SerializeField] private bool requiresHold;
        [SerializeField] private float holdSeconds = 0.5f;

        [Header("Availability")]
        [SerializeField] private bool interactable = true;
        [SerializeField] private bool disableAfterUse;

        [Header("Events")]
        [SerializeField] private GameObjectUnityEvent onInteract;

        public virtual bool CanInteract(GameObject interactor)
        {
            return interactable && isActiveAndEnabled;
        }

        public virtual InteractionPromptData GetPrompt(GameObject interactor)
        {
            return new InteractionPromptData
            {
                Title = title,
                ActionText = actionText,
                Icon = icon,
                RequiresHold = requiresHold,
                HoldSeconds = Mathf.Max(0f, holdSeconds)
            };
        }

        public virtual void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                return;
            }

            onInteract?.Invoke(interactor);

            if (disableAfterUse)
            {
                interactable = false;
            }
        }

        public void SetInteractable(bool value)
        {
            interactable = value;
        }

        public void SetPrompt(string newTitle, string newActionText)
        {
            title = newTitle;
            actionText = newActionText;
        }
    }
}
