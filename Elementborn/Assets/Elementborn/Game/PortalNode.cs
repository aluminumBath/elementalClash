using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A portal you physically stand at inside the portal-pool map. Walk up and Interact to <b>step through</b> to
    /// its world location — but only once it's been discovered out in the world. Discovered nodes glow in their
    /// <see cref="PortalTheme"/> colour; undiscovered ones stay dim and only hint that you must find them first.
    /// No map UI is needed — you're standing on the map. Placed by <see cref="PortalPoolRoom"/>.
    /// </summary>
    public sealed class PortalNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private float range = 2.6f;

        private string _riftId;
        private string _riftName;
        private Element? _element;
        private Renderer[] _renderers;
        private bool _wasDiscovered;

        public void Configure(LeylineRift rift)
        {
            _riftId = rift.Id;
            _riftName = rift.Name;
            _element = rift.Elem;
            _renderers = GetComponentsInChildren<Renderer>(true);
            _wasDiscovered = IsDiscovered();
            Paint(_wasDiscovered);
        }

        private bool IsDiscovered() =>
            MapState.Instance != null && MapState.Instance.Network != null && MapState.Instance.Network.IsDiscovered(_riftId);

        private void Paint(bool discovered)
        {
            if (_renderers == null) return;
            PortalStyle style = PortalTheme.For(_element);
            float emit = discovered ? 2.4f : 0.2f;
            Color body = discovered ? style.Glow : style.Glow * 0.35f;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                var m = r.material;
                m.color = body;
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", style.Glow * emit);
            }
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;

            bool discovered = IsDiscovered();
            if (discovered != _wasDiscovered) { _wasDiscovered = discovered; Paint(discovered); }

            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > range) return false;

            if (discovered)
                interaction = new Interaction(d, 0, "Step through — " + _riftName, () => MapState.Instance?.WarpToRift(_riftId));
            else
                interaction = new Interaction(d, 0, _riftName + " (undiscovered)",
                    () => GameHud.Instance?.Toast("Find " + _riftName + " out in the world to attune this portal."));
            return true;
        }
    }
}
