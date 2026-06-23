using UnityEngine;
using UnityEngine.XR;

namespace Elementborn.Game
{
    /// <summary>
    /// VR Interact. A press of the right-hand <b>grip</b> fires the current interaction through the shared
    /// <see cref="InteractionArbiter"/> — the same path the desktop Interact key uses, so talking to NPCs, picking
    /// up / activating, mounting, taming, and the leyline-rift / checkpoint prompts all work from the headset.
    /// Reads the controller with the legacy XR input API (matching <c>VrInputProvider</c> / comfort locomotion);
    /// the bootstrap VR rig adds one.
    /// </summary>
    public sealed class VrInteractInput : MonoBehaviour
    {
        [SerializeField] private XRNode hand = XRNode.RightHand;
        private bool _prev;

        private void Update()
        {
            var d = InputDevices.GetDeviceAtXRNode(hand);
            bool now = d.isValid && d.TryGetFeatureValue(CommonUsages.gripButton, out bool grip) && grip;
            if (now && !_prev) InteractionArbiter.SignalInteract(); // rising edge
            _prev = now;
        }
    }
}
