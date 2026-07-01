using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// An in-world leyline rift. Stepping near it discovers it (so it becomes a fast-travel destination); standing
    /// in range also offers an Interact to open the leyline map. Discovery and travel run through
    /// <see cref="MapState"/>. Spawned by <see cref="LeylineRiftSpawner"/> from the canonical <see cref="WorldMapLayout"/>
    /// list, so it offers its interaction through the shared <see cref="InteractionArbiter"/>.
    /// </summary>
    public sealed class LeylineRiftObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string riftId;
        [SerializeField] private string riftName;
        [SerializeField] private float discoverRange = 7f;
        [SerializeField] private float interactRange = 5f;

        private Element? _element;      // the portal's element (null = neutral gate/crossing)
        private PortalTier _tier;       // capital hub vs city portal
        private Renderer[] _renderers;
        private bool _litForDiscovery;

        public void Configure(LeylineRift rift)
        {
            riftId = rift.Id;
            riftName = rift.Name;
            _element = rift.Elem;
            _tier = rift.Tier;
            transform.position = rift.World;
            ApplyTheme(false); // undiscovered: a dim elemental tint
        }

        // Tint + emissive the rift's meshes with its element's portal colour (water = teal, fire = ember, ...).
        // Discovered portals glow brighter so an attuned destination reads at a glance.
        private void ApplyTheme(bool discovered)
        {
            var style = PortalTheme.For(_element);
            if (_renderers == null) _renderers = GetComponentsInChildren<Renderer>(true);
            float emit = discovered ? 2.2f : 0.7f;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                var m = r.material; // runtime instance
                m.color = style.Glow;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", style.Glow * emit);
            }
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

            // Brighten the moment it's discovered (once), so an attuned portal visibly glows.
            if (!_litForDiscovery && MapState.Instance != null && MapState.Instance.Network != null
                && MapState.Instance.Network.IsDiscovered(riftId))
            {
                _litForDiscovery = true;
                ApplyTheme(true);
            }

            if (d > interactRange) return false;

            // A capital pool routes to its element's discovered city portals; everything else opens the leyline map.
            if (_tier == PortalTier.Capital && _element.HasValue)
                interaction = new Interaction(d, 0, "Portal pool", () => PortalPoolController.EnsureInstance().Open(_element.Value));
            else
                interaction = new Interaction(d, 0, "Leyline map", () => MapViewerController.Instance?.Open());
            return true;
        }
    }
}
