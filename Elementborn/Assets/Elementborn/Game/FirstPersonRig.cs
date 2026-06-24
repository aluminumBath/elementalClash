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

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;

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
    }
}
