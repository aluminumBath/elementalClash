using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CombatStaminaPickup : BaseInteractable
    {
        [SerializeField] private float restoreAmount = 35f;
        [SerializeField] private bool destroyOnUse = true;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple("Stamina Wisp", $"Restore {Mathf.RoundToInt(restoreAmount)} stamina");
        }

        public override void Interact(GameObject interactor)
        {
            StaminaResource stamina = interactor != null ? interactor.GetComponent<StaminaResource>() : null;
            if (stamina == null && interactor != null)
            {
                stamina = interactor.GetComponentInParent<StaminaResource>();
            }

            if (stamina != null)
            {
                stamina.Restore(restoreAmount);
            }

            base.Interact(interactor);

            if (destroyOnUse)
            {
                Destroy(gameObject);
            }
        }
    }
}
