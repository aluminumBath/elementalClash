using UnityEngine;
using UnityEngine.XR;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// VR admin toggle for the live-log console: look at the inside of your left wrist and tap it with your right
    /// hand to show/hide <see cref="AdminLogConsole"/>. Self-bootstrapping and gated by
    /// <see cref="AdminLogConsole.AdminUnlocked"/>; reads the HMD + both controller poses from XR device nodes (no
    /// scene wiring, same approach as <c>VrInputProvider</c>) and evaluates the unit-tested
    /// <see cref="WristGesture"/>. The pose is held briefly to confirm, then a short cooldown ensures one toggle
    /// per gesture.
    ///
    /// NOTE: look-at-left-hand + right-hand-touch approximates the wrist tap; tighten it to a palm-up inner-wrist
    /// pose once hand-tracking is wired (see VR_INPUT_MAP.md).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WristMenuGesture : MonoBehaviour
    {
        private const float MaxGazeAngle = 35f;
        private const float TouchDistance = 0.12f;
        private const float HoldSeconds = 0.35f;
        private const float Cooldown = 1.0f;

        private static WristMenuGesture _instance;
        private float _held;
        private float _cooldown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("WristMenuGesture");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<WristMenuGesture>();
        }

        private void Update()
        {
            if (!AdminLogConsole.AdminUnlocked) { _held = 0f; return; }
            if (_cooldown > 0f) { _cooldown -= Time.unscaledDeltaTime; return; }

            if (!TryPos(XRNode.Head, out Vector3 head) ||
                !TryRot(XRNode.Head, out Quaternion headRot) ||
                !TryPos(XRNode.LeftHand, out Vector3 left) ||
                !TryPos(XRNode.RightHand, out Vector3 right))
            {
                _held = 0f;
                return; // not in VR / no tracking this frame
            }

            Vector3 fwd = headRot * Vector3.forward;
            bool active = WristGesture.IsActivated(
                head.x, head.y, head.z,
                fwd.x, fwd.y, fwd.z,
                left.x, left.y, left.z,
                right.x, right.y, right.z,
                MaxGazeAngle, TouchDistance);

            if (!active) { _held = 0f; return; }

            _held += Time.unscaledDeltaTime;
            if (_held >= HoldSeconds)
            {
                _held = 0f;
                _cooldown = Cooldown;
                AdminLogConsole.Toggle();
            }
        }

        private static bool TryPos(XRNode node, out Vector3 pos)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            if (d.isValid && d.TryGetFeatureValue(CommonUsages.devicePosition, out pos)) return true;
            pos = Vector3.zero;
            return false;
        }

        private static bool TryRot(XRNode node, out Quaternion rot)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            if (d.isValid && d.TryGetFeatureValue(CommonUsages.deviceRotation, out rot)) return true;
            rot = Quaternion.identity;
            return false;
        }
    }
}
