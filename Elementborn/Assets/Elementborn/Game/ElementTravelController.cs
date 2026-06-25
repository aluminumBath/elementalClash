using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Water and air crossing. One toggle (F by default, or a VR button) summons the right craft for the
    /// player's element and mounts them on it: a **water** channeler raises an **ice floe** to surf on; an
    /// **air** channeler forms a **bubble** to float in — and the bubble disables attacks (the mount does
    /// that via its combat flag). Both ride at the water line, so you must be over/near water to summon.
    /// Put this on the player rig (which also needs a FactionMember so we know your element).
    /// </summary>
    public sealed class ElementTravelController : MonoBehaviour
    {
        [SerializeField] private InputActionReference travelAction;
        [SerializeField] private float waterReach = 3f; // how far above the water line you may summon

        private MountController _active;
        private GameObject _craft;

        public bool Traveling => _active != null;

        private void Update()
        {
            if (TravelPressed()) Toggle();
        }

        public void Toggle()
        {
            if (_active != null) { End(); return; }

            var mode = ElementTravel.ModeFor(PlayerElement());
            bool confluence = PlayerInventory.Instance != null && PlayerInventory.Instance.PlayerIsConfluence;
            if (mode == TravelMode.None && confluence) mode = TravelMode.IceFloe; // Confluence defaults to the floe
            if (mode == TravelMode.None) return; // fire / earth have no water-travel

            if (!NearWater())
            {
                Debug.Log("[Elementborn] Element travel needs water nearby.");
                return;
            }

            Vector3 at = new Vector3(transform.position.x, WorldWater.SeaLevelY, transform.position.z);
            _craft = mode == TravelMode.IceFloe ? BuildFloe(at) : BuildBubble(at);
            _active = _craft.GetComponent<MountController>();
            _active.Mount(gameObject);
        }

        private void End()
        {
            if (_active != null) _active.Dismount();
            if (_craft != null) Destroy(_craft);
            _active = null;
            _craft = null;
        }

        private Element? PlayerElement()
        {
            var fm = GetComponent<FactionMember>();
            return fm != null ? fm.Element : (Element?)null;
        }

        private bool NearWater() => transform.position.y <= WorldWater.SeaLevelY + waterReach;

        private bool TravelPressed()
        {
            if (travelAction != null && travelAction.action != null && travelAction.action.enabled)
                return travelAction.action.WasPressedThisFrame();
            return InputBindings.ElementTravel.WasPressedThisFrame();
        }

        private GameObject BuildFloe(Vector3 at)
        {
            var go = new GameObject("IceFloe");
            go.transform.position = at;

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.transform.SetParent(go.transform, false);
            disc.transform.localScale = new Vector3(3f, 0.12f, 3f);
            disc.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            var col = disc.GetComponent<Collider>();
            if (col != null) Destroy(col);
            disc.GetComponent<MeshRenderer>().sharedMaterial = ToonPalette.Tinted(new Color(0.72f, 0.9f, 1f));

            var mc = go.AddComponent<MountController>();
            mc.Configure(LocomotionType.Water, false, WorldWater.SeaLevelY, 0.12f);
            return go;
        }

        private GameObject BuildBubble(Vector3 at)
        {
            var go = new GameObject("AirBubble");
            go.transform.position = at;

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(go.transform, false);
            sphere.transform.localScale = Vector3.one * 3f;
            sphere.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var col = sphere.GetComponent<Collider>();
            if (col != null) Destroy(col);
            sphere.GetComponent<MeshRenderer>().sharedMaterial = BubbleMaterial();

            var mc = go.AddComponent<MountController>();
            mc.Configure(LocomotionType.Water, true, WorldWater.SeaLevelY, 0.1f); // bubble disables attacks
            return go;
        }

        private static Material BubbleMaterial()
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) return ToonPalette.Tinted(new Color(0.6f, 0.85f, 1f));
            return new Material(shader) { color = new Color(0.6f, 0.85f, 1f, 0.28f) };
        }
    }
}
