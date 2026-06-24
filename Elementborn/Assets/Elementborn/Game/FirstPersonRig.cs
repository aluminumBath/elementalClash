using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal first-person controller for flat play: WASD movement and mouse look.
    /// Kept separate from combat input so a third-person rig can replace it later
    /// without touching the ability pipeline.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonRig : MonoBehaviour
    {
        [SerializeField] private Camera rigCamera;
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float lookSensitivity = 0.1f;
        [SerializeField] private float stickLookSpeed = 180f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpSpeed = 5.5f;
        [SerializeField] private float glideFallSpeed = GlideMotion.DefaultGlideFallSpeed;
        [SerializeField] private float climbSpeed = 3f;
        [SerializeField] private float climbReach = 0.7f;
        [SerializeField] private float climbRayHeight = 1f;
        [SerializeField] private float mantleBoost = 3f;
        [SerializeField] private float maxWalkableNormalY = ClimbMotion.DefaultMaxWalkableNormalY;

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;
        private bool _climbing;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (rigCamera == null) rigCamera = GetComponentInChildren<Camera>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            Look();
            Move();
        }

        private void Look()
        {
            if (rigCamera == null) return;
            var settings = SettingsStore.Current;

            Vector2 delta = Vector2.zero;
            var mouse = Mouse.current;
            if (mouse != null) delta += mouse.delta.ReadValue() * lookSensitivity;
            var pad = Gamepad.current;
            if (pad != null) delta += pad.rightStick.ReadValue() * stickLookSpeed * Time.deltaTime;
            delta *= settings.mouseSensitivity;

            transform.Rotate(Vector3.up, delta.x);
            float dy = settings.invertY ? -delta.y : delta.y;
            _pitch = Mathf.Clamp(_pitch - dy, -85f, 85f);
            rigCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            rigCamera.fieldOfView = settings.fieldOfView;
        }

        private void Move()
        {
            var keyboard = Keyboard.current;
            Vector2 input = Vector2.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed) input.y -= 1f;
                if (keyboard.dKey.isPressed) input.x += 1f;
                if (keyboard.aKey.isPressed) input.x -= 1f;
            }
            var pad = Gamepad.current;
            if (pad != null) input += pad.leftStick.ReadValue();
            input = Vector2.ClampMagnitude(input, 1f);

            if (TryClimb(input, out Vector3 climbMove))
            {
                _controller.Move(climbMove * Time.deltaTime);
                return;
            }

            Vector3 move = (transform.forward * input.y + transform.right * input.x) * moveSpeed;

            bool grounded = _controller.isGrounded;
            if (grounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            if (grounded && InputBindings.Jump.WasPressedThisFrame()) _verticalVelocity = jumpSpeed;
            _verticalVelocity += gravity * Time.deltaTime;

            if (GlideMotion.IsGliding(grounded, _verticalVelocity, InputBindings.Jump.IsPressed()))
            {
                _verticalVelocity = GlideMotion.ClampDescent(_verticalVelocity, glideFallSpeed);
                move *= GlideMotion.GlideForwardMultiplier; // horizontal only; move.y is set just below
            }

            move.y = _verticalVelocity;
            _controller.Move(move * Time.deltaTime);
        }

        // Flat-play wall climb: push into a steep surface to cling and ascend; press Jump to drop off; clearing
        // the top edge mantles you up and over. (VR grab-climb is a separate VR-moves-pass item.)
        private bool TryClimb(Vector2 input, out Vector3 climbMove)
        {
            climbMove = Vector3.zero;
            if (input.y <= 0.1f) { _climbing = false; return false; } // only climb while pushing toward the wall

            Vector3 normal = -transform.forward;
            bool climbableAhead = false;
            Vector3 origin = transform.position + Vector3.up * climbRayHeight + transform.forward * (_controller.radius * 0.9f);
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, climbReach, ~0, QueryTriggerInteraction.Ignore))
            {
                climbableAhead = ClimbMotion.IsClimbable(hit.normal.y, maxWalkableNormalY);
                if (climbableAhead) normal = hit.normal;
            }

            if (climbableAhead)
            {
                if (InputBindings.Jump.WasPressedThisFrame())
                {
                    _climbing = false;
                    _verticalVelocity = jumpSpeed; // hop off; the normal movement path takes over next frame
                    return false;
                }
                _climbing = true;
                _verticalVelocity = 0f;
                Vector3 wallRight = Vector3.Cross(Vector3.up, normal);
                if (wallRight.sqrMagnitude > 1e-4f) wallRight.Normalize();
                climbMove = (Vector3.up * input.y + wallRight * input.x) * climbSpeed - normal * 0.6f; // ascend + hug
                return true;
            }

            if (_climbing) // was on the wall and just cleared the top: mantle up and over the lip
            {
                _climbing = false;
                _verticalVelocity = 0f;
                climbMove = transform.forward * moveSpeed + Vector3.up * mantleBoost;
                return true;
            }

            return false;
        }
    }
}
