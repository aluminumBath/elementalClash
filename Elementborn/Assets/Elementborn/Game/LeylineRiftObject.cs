using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// An in-world leyline rift. Stepping near it discovers it (so it becomes a fast-travel destination); standing
    /// in range also offers an Interact to open the leyline map. Discovery and travel run through
    /// <see cref="MapState"/>. Spawned by <see cref="LeylineRiftSpawner"/> from the canonical <see cref="WorldMap"/>
    /// list, so it offers its interaction through the shared <see cref="InteractionArbiter"/>.
    /// </summary>
    public sealed class LeylineRiftObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string riftId;
        [SerializeField] private string riftName;
        [SerializeField] private float discoverRange = 7f;
        [SerializeField] private float interactRange = 5f;

        public void Configure(LeylineRift rift)
        {
            riftId = rift.Id;
            riftName = rift.Name;
            transform.position = rift.World;
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        // The arbiter calls this every frame for every registered rift, so discovery-on-approach works even when
        // another interactable wins the prompt and even before we're within interact range.
        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);

            if (d <= discoverRange && MapState.Instance != null && MapState.Instance.Discover(riftId))
            {
                GameHud.Instance?.Toast("Leyline attuned — " + riftName);
                AudioController.Instance?.Confirm();
            }

            if (d > interactRange) return false;
            interaction = new Interaction(d, 0, "Leyline map", () => MapViewerController.Instance?.Open());
            return true;
        }
    }
}
