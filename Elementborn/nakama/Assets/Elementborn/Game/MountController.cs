using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Makes a vehicle or large creature rideable. On mount it attaches the player rig to a seat, disables
    /// the player's own locomotion, and drives movement itself: ground mounts ride the terrain, water craft
    /// sit at the water line, flyers move freely in 3D. Steering is smooth, and a <see cref="ComfortVignette"/>
    /// on the rider's camera fades in with speed. Reads WASD/Space/Ctrl by default; assign the input actions
    /// for VR thumbstick control. Set the locomotion type to match the mount.
    /// </summary>
    public sealed class MountController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private LocomotionType locomotion = LocomotionType.Ground;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float turnSpeed = 90f;
        [SerializeField] private float verticalSpeed = 5f;
        [SerializeField] private float groundOffset = 1f;
        [SerializeField] private float waterLevel = 0f;

        [Header("Seat / dismount")]
        [SerializeField] private Transform seat;
        [SerializeField] private Vector3 dismountOffset = new Vector3(1.5f, 0f, 0f);

        [Header("Rider control to disable while riding (VR move provider, etc.)")]
        [SerializeField] private Behaviour[] disableWhileRiding;
        [Tooltip("Disable the rider's combat while riding (used by the air bubble).")]
        [SerializeField] private bool disableCombatWhileRiding = false;

        [Header("Optional input actions (VR); falls back to keyboard")]
        [SerializeField] private InputActionReference moveAction;     // Vector2: x=turn, y=forward
        [SerializeField] private InputActionReference verticalAction; // float: up/down for flyers

        private GameObject _rider;
        private Transform _riderParent;
        private CharacterController _riderCc;
        private ComfortVignette _vignette;
        private readonly List<Behaviour> _disabled = new List<Behaviour>();

        public bool IsRidden => _rider != null;

        /// <summary>Configure a summoned craft (ice floe, air bubble) at runtime.</summary>
        public void Configure(LocomotionType loc, bool disableCombat, float water, float ground)
        {
            locomotion = loc;
            disableCombatWhileRiding = disableCombat;
            waterLevel = water;
            groundOffset = ground;
        }

        public void Toggle(GameObject riderRoot)
        {
            if (IsRidden) Dismount();
            else Mount(riderRoot);
        }

        public void Mount(GameObject riderRoot)
        {
            if (riderRoot == null || IsRidden) return;
            _rider = riderRoot;
            _vignette = riderRoot.GetComponentInChildren<ComfortVignette>();

            _disabled.Clear();
            DisableIfPresent(riderRoot.GetComponent<FirstPersonRig>());
            if (disableWhileRiding != null)
                foreach (var b in disableWhileRiding) DisableIfPresent(b);
            if (disableCombatWhileRiding)
                DisableIfPresent(riderRoot.GetComponentInChildren<PlayerCombatController>());

            _riderCc = riderRoot.GetComponent<CharacterController>();
            if (_riderCc != null) _riderCc.enabled = false;

            Transform seatT = seat != null ? seat : transform;
            _riderParent = riderRoot.transform.parent;
            riderRoot.transform.SetParent(seatT, false);
            riderRoot.transform.localPosition = Vector3.zero;
            riderRoot.transform.localRotation = Quaternion.identity;
        }

        public void Dismount()
        {
            if (!IsRidden) return;

            foreach (var b in _disabled) if (b != null) b.enabled = true;
            _disabled.Clear();
            if (_riderCc != null) _riderCc.enabled = true;

            _rider.transform.SetParent(_riderParent, false);
            _rider.transform.position = transform.position + transform.TransformVector(dismountOffset);
            _vignette?.SetIntensity(0f);

            _rider = null;
            _riderParent = null;
            _riderCc = null;
            _vignette = null;
        }

        private void DisableIfPresent(Behaviour b)
        {
            if (b == null || !b.enabled) return;
            b.enabled = false;
            _disabled.Add(b);
        }

        private void Update()
        {
            if (!IsRidden) return;

            ReadInput(out float forward, out float turn, out float vertical);

            if (Mathf.Abs(turn) > 0.001f)
                transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

            Vector3 flat = transform.forward;
            flat.y = 0f;
            flat = flat.sqrMagnitude > 0.0001f ? flat.normalized : Vector3.forward;

            Vector3 pos = transform.position + flat * (forward * moveSpeed * Time.deltaTime);

            switch (locomotion)
            {
                case LocomotionType.Ground:
                    pos.y = SampleGround(pos) + groundOffset;
                    break;
                case LocomotionType.Water:
                    pos.y = waterLevel + groundOffset;
                    break;
                case LocomotionType.Flying:
                    pos.y = transform.position.y + vertical * verticalSpeed * Time.deltaTime;
                    break;
            }

            transform.position = pos;

            float speedFactor = Mathf.Clamp01(Mathf.Abs(forward) + Mathf.Abs(turn) * 0.5f + Mathf.Abs(vertical) * 0.5f);
            _vignette?.SetIntensity(speedFactor);
        }

        private void ReadInput(out float forward, out float turn, out float vertical)
        {
            forward = 0f; turn = 0f; vertical = 0f;

            if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
            {
                Vector2 v = moveAction.action.ReadValue<Vector2>();
                forward = v.y; turn = v.x;
            }
            else
            {
                var k = Keyboard.current;
                if (k != null)
                {
                    if (k.wKey.isPressed) forward += 1f;
                    if (k.sKey.isPressed) forward -= 1f;
                    if (k.dKey.isPressed) turn += 1f;
                    if (k.aKey.isPressed) turn -= 1f;
                }
                var pad = Gamepad.current;
                if (pad != null) { Vector2 s = pad.leftStick.ReadValue(); forward += s.y; turn += s.x; }
            }

            if (verticalAction != null && verticalAction.action != null && verticalAction.action.enabled)
            {
                vertical = verticalAction.action.ReadValue<float>();
            }
            else
            {
                var k = Keyboard.current;
                if (k != null)
                {
                    if (k.spaceKey.isPressed) vertical += 1f;
                    if (k.leftCtrlKey.isPressed) vertical -= 1f;
                }
                var pad = Gamepad.current;
                if (pad != null) vertical += pad.rightTrigger.ReadValue() - pad.leftTrigger.ReadValue();
            }
        }

        private static float SampleGround(Vector3 pos) => TerrainHeight.Sample(pos);
    }
}
