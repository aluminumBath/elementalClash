using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Put on one of Willow's companions. Stand near and press Interact to feed it (its food is flavour for now —
    /// a food-item requirement is a follow-up once an item inventory exists). Records the feeding with
    /// <see cref="SidekickFeedingController"/>. Offers its "Feed" interaction through the
    /// <see cref="InteractionArbiter"/>.
    /// </summary>
    public sealed class SidekickFeedPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private WillowSidekick sidekick = WillowSidekick.Gunnar;
        [SerializeField] private float reach = 3f;

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > reach) return false;
            var info = WillowSidekicks.For(sidekick);
            interaction = new Interaction(d, 0, $"Feed {info.Name} ({info.Food})",
                () => SidekickFeedingController.Instance?.Feed(sidekick));
            return true;
        }
    }
}
