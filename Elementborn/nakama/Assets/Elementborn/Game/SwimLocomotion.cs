using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Underwater swimming. While the actor's <see cref="UnderwaterController"/> reports submerged, this takes
    /// over movement from the ground rig and swims in full 3D — you move where you look, with explicit
    /// ascend/descend, gentle buoyancy, and the speed scaled by <see cref="UnderwaterController.SpeedScale"/>
    /// (water users full speed, others slower, bubble/ice slower still). Capped speed plus the shared
    /// <see cref="ComfortVignette"/> keep it comfortable in a headset; the actual swim math is the pure
    /// <see cref="SwimMotion"/>. On the surface it disables itself and the normal rig drives.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class SwimLocomotion : MonoBehaviour
    {
        [SerializeField] private UnderwaterController underwater;
        [Tooltip("Head/camera that defines 'forward' (HMD in VR, the rig camera on flat).")]
        [SerializeField] private Transform head;
        [Tooltip("Ground locomotion disabled while submerged (FirstPersonRig / ThirdPersonRig / VR locomotion).")]
        [SerializeField] private Behaviour groundMovement;
        [SerializeField] private ComfortVignette comfort;

        [SerializeField] private float swimSpeed = 4f;
        [SerializeField] private float maxSpeed = 6f;
        [SerializeField] private float buoyancy = 0.4f;            // gentle upward drift
        [SerializeField] private float comfortFullSpeed = 5f;      // speed at which the vignette is fully on

        private CharacterController _cc;
        private bool _swimming;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (underwater == null) underwater = GetComponent<UnderwaterController>();
            if (head == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) head = cam.transform;
            }
            if (comfort == null && head != null) comfort = head.GetComponentInChildren<ComfortVignette>();
            if (comfort == null) comfort = GetComponentInChildren<ComfortVignette>();
            if (groundMovement == null)
                groundMovement = (Behaviour)GetComponent<FirstPersonRig>() ?? GetComponent<ThirdPersonRig>();
        }

        private void Update()
        {
            bool submerged = underwater != null && underwater.IsSubmerged;
            if (submerged != _swimming)
            {
                _swimming = submerged;
                if (groundMovement != null) groundMovement.enabled = !submerged;
                if (!submerged && comfort != null) comfort.SetIntensity(0f);
            }
            if (!submerged) return;

            Vector3 planar = ReadPlanar();
            float vertical = ReadVertical();

            Transform basis = head != null ? head : transform;
            Vector3 worldMove = basis.forward * planar.y + basis.right * planar.x; // full 3D: look up + push = ascend

            Vector3 v = SwimMotion.Velocity(worldMove, vertical, swimSpeed, underwater.SpeedScale, maxSpeed);
            v.y += buoyancy;
            _cc.Move(v * Time.deltaTime);

            if (comfort != null) comfort.SetIntensity(Mathf.Clamp01(v.magnitude / Mathf.Max(0.01f, comfortFullSpeed)));
        }

        private Vector2 ReadPlanar()
        {
            Vector2 input = Vector2.zero;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed) input.y += 1f;
                if (kb.sKey.isPressed) input.y -= 1f;
                if (kb.dKey.isPressed) input.x += 1f;
                if (kb.aKey.isPressed) input.x -= 1f;
            }
            var pad = Gamepad.current;
            if (pad != null) input += pad.leftStick.ReadValue();

            var left = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (left.isValid && left.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
                input += axis;

            return Vector2.ClampMagnitude(input, 1f);
        }

        private float ReadVertical()
        {
            float v = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.eKey.isPressed) v += 1f;
                if (kb.qKey.isPressed) v -= 1f;
            }
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (right.isValid && right.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
                v += axis.y;

            return Mathf.Clamp(v, -1f, 1f);
        }
    }
}
