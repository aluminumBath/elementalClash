using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Anchors a persistent, code-built HUD canvas comfortably in VR. Unlike <see cref="VrCanvasAdapter"/> (which
    /// parks a menu in front of you when it opens), this keeps a small readout in the lower field of view and
    /// follows the head with a little smoothing, so it never feels rigidly locked or sits dead-centre. When XR is
    /// active it switches the canvas to <b>World Space</b>, disables the screen <see cref="CanvasScaler"/>, sizes
    /// it, and re-places it every frame from a head-relative offset (right / up / forward, in metres). Flat/desktop
    /// is untouched — it no-ops, leaving the screen-space layout alone. Added at runtime by the player HUD
    /// controllers, each with its own <see cref="viewOffset"/> so the readouts don't overlap. Offsets and sizes are
    /// sensible starting values meant to be fine-tuned in the editor against a real headset.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public sealed class VrHudAnchor : MonoBehaviour
    {
        public Vector3 viewOffset = new Vector3(0f, -0.40f, 1.45f); // right, up, forward (m) from the head
        public Vector2 worldSize = new Vector2(520f, 360f);
        public float scale = 0.0016f;     // world units per UI unit
        public float follow = 10f;        // higher = snappier; lower = floatier/more comfortable

        private bool _init;
        private bool _converted;

        private void EnsureConverted()
        {
            _init = true;
            if (!XrState.Active) return; // flat/desktop: leave the screen-space canvas exactly as built
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var scaler = GetComponent<CanvasScaler>();
            if (scaler != null) scaler.enabled = false; // a screen scaler is meaningless in world space
            var rt = (RectTransform)canvas.transform;
            rt.sizeDelta = worldSize;
            rt.localScale = Vector3.one * scale;
            _converted = true;
        }

        private void LateUpdate()
        {
            if (!_init) EnsureConverted();
            if (!_converted) return;
            var cam = Camera.main;
            if (cam == null) return;

            var rt = (RectTransform)transform;
            Vector3 target = cam.transform.position
                + cam.transform.right * viewOffset.x
                + cam.transform.up * viewOffset.y
                + cam.transform.forward * viewOffset.z;

            float t = 1f - Mathf.Exp(-follow * Time.deltaTime); // frame-rate-independent smoothing
            rt.position = Vector3.Lerp(rt.position, target, t);
            Quaternion face = Quaternion.LookRotation(rt.position - cam.transform.position, cam.transform.up);
            rt.rotation = Quaternion.Slerp(rt.rotation, face, t);
        }
    }
}
