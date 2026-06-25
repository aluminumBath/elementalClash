using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>The way out of a site interior — Interact to return to the open world. Spawned inside the pocket
    /// room by <see cref="SiteInteriorController"/>; it offers its prompt through the shared interaction arbiter.</summary>
    public sealed class SiteExit : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactRange = 3.5f;

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > interactRange) return false;
            interaction = new Interaction(d, 0, "Leave", () => SiteInteriorController.Instance?.Exit());
            return true;
        }
    }
}
