using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>A hoard inside a site interior — Interact to claim it. Spawned by <see cref="SiteInteriorController"/>;
    /// the reward hookup (currency/loot) lands in a follow-up, so for now claiming acknowledges and clears it.</summary>
    public sealed class SiteTreasure : MonoBehaviour, IInteractable
    {
        [SerializeField] private float interactRange = 3f;
        private bool _claimed;

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            if (_claimed) return false;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > interactRange) return false;
            interaction = new Interaction(d, 0, "Claim the hoard", Claim);
            return true;
        }

        private void Claim()
        {
            _claimed = true;
            GameHud.Instance?.Toast("You claim the hoard!");
            AudioController.Instance?.Confirm();
            gameObject.SetActive(false);
        }
    }
}
