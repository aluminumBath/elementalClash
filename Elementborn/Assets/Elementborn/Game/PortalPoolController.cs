using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The capital "portal pool": stand at an elemental capital's pool and pick a discovered <b>city</b> portal of
    /// that same element to fast-travel to. Water pools glow teal and lead to glassy-pond city portals; fire pools
    /// ember, earth mossy, air shimmering. Only same-element city portals you've already discovered appear — capitals
    /// are hubs, not destinations. Opened by a capital <see cref="LeylineRiftObject"/>; travels via <see cref="MapState"/>.
    /// </summary>
    public sealed class PortalPoolController : MonoBehaviour
    {
        public static PortalPoolController Instance { get; private set; }

        private Canvas _canvas;
        private Transform _content;

        public static PortalPoolController EnsureInstance()
        {
            if (Instance != null) return Instance;
            var found = FindObjectOfType<PortalPoolController>();
            if (found != null) return found;
            return new GameObject(nameof(PortalPoolController)).AddComponent<PortalPoolController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Open the pool for an element, listing every discovered same-element city portal as a travel target.</summary>
        public void Open(Element element)
        {
            PortalStyle style = PortalTheme.For(element);
            if (_canvas != null) Destroy(_canvas.gameObject);

            var p = OverlayUi.Panel("PortalPoolCanvas", element + " Portal Pool", 235, new Vector2(560, 620), Hide);
            _canvas = p.canvas;
            _content = p.content;

            OverlayUi.Body(_content, "Step through to " + style.Look + ".", 16, style.Glow);
            OverlayUi.Body(_content, " ", 6);

            FastTravelNetwork net = MapState.Instance != null ? MapState.Instance.Network : null;
            List<LeylineRift> cities = net != null ? net.DiscoveredCitiesOfElement(element) : new List<LeylineRift>();

            if (cities.Count == 0)
            {
                OverlayUi.Body(_content, "No " + element + " city portals discovered yet — find and attune them out in the world.", 16);
                return;
            }

            foreach (var city in cities)
            {
                string destId = city.Id; // capture per-iteration
                Button btn = UiTheme.Button(_content, city.Name, () => { MapState.Instance?.WarpToRift(destId); Hide(); }, 480, 48);
                if (btn.targetGraphic is Image img) img.color = style.Glow; // tint the button to the element's glow
            }
        }

        public void Hide()
        {
            if (_canvas != null) Destroy(_canvas.gameObject);
            _canvas = null;
            _content = null;
        }
    }
}
