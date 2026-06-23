using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Makes a code-built overlay canvas usable in VR. When XR is active it switches the canvas to <b>World
    /// Space</b>, drops the screen <see cref="CanvasScaler"/>, sizes it, and (each time it opens) places it in
    /// front of the player and faces it to them. Flat/desktop is untouched — the adapter no-ops. As with the
    /// creation UI, the controller-ray <i>click</i> still needs an XRI TrackedDeviceGraphicRaycaster +
    /// XRUIInputModule on the rig (added in-editor); this only handles the canvas side. Attached by the overlay
    /// builders (OverlayUi.Panel and the few direct-canvas screens).
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class VrCanvasAdapter : MonoBehaviour
    {
        [SerializeField] private float distance = 2.2f;   // metres in front of the head
        [SerializeField] private float height = 1.5f;     // fallback height if no camera
        [SerializeField] private float scale = 0.0016f;   // world units per UI unit
        [SerializeField] private Vector2 worldSize = new Vector2(1500f, 950f);

        private bool _converted;

        private void Awake()
        {
            if (!XrState.Active) return;
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null) scaler.enabled = false; // a screen scaler is meaningless in world space

            var rt = (RectTransform)canvas.transform;
            rt.sizeDelta = worldSize;
            rt.localScale = Vector3.one * scale;
            _converted = true;
        }

        // Each time the overlay is shown, bring it to where the player is looking.
        private void OnEnable()
        {
            if (!_converted) return;
            var cam = Camera.main;
            var rt = (RectTransform)transform;
            if (cam != null)
            {
                rt.position = cam.transform.position + cam.transform.forward * distance;
                rt.rotation = Quaternion.LookRotation(rt.position - cam.transform.position);
            }
            else
            {
                rt.position = new Vector3(0f, height, distance);
                rt.rotation = Quaternion.identity;
            }
        }
    }
}
