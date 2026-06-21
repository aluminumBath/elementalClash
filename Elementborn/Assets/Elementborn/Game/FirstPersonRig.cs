using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private float gravity = -9.81f;

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
            var mouse = Mouse.current;
            if (mouse == null || rigCamera == null) return;

            Vector2 delta = mouse.delta.ReadValue() * lookSensitivity;
            transform.Rotate(Vector3.up, delta.x);
            _pitch = Mathf.Clamp(_pitch - delta.y, -85f, 85f);
            rigCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
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

            Vector3 move = (transform.forward * input.y + transform.right * input.x) * moveSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _controller.Move(move * Time.deltaTime);
        }
    }
}
