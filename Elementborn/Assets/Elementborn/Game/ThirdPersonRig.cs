using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// A third-person controller: an orbit camera on a boom behind a visible player capsule, with
    /// camera-relative WASD movement and mouse orbit. It deliberately mirrors <see cref="FirstPersonRig"/>'s
    /// shape (CharacterController + gravity) and leaves combat untouched: point <see cref="FlatInputProvider"/>'s
    /// aim camera at <see cref="rigCamera"/> and casts go where the camera looks. Reads sensitivity, invert-Y,
    /// and FOV from <see cref="SettingsStore"/>. A short spring keeps the camera out of walls/terrain.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class ThirdPersonRig : MonoBehaviour
    {
        [SerializeField] private Camera rigCamera;
        [SerializeField] private Transform body;          // the visible model to rotate (optional)
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float stickLookSpeed = 180f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float bodyTurnSpeed = 12f;

        [Header("Camera boom")]
        [SerializeField] private float distance = 4.5f;
        [SerializeField] private float pivotHeight = 1.6f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private LayerMask cameraCollision = ~0;

        private CharacterController _controller;
        private float _yaw;
        private float _pitch = 15f;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (rigCamera == null) rigCamera = GetComponentInChildren<Camera>();
            if (rigCamera == null)
            {
                var camGo = new GameObject("ThirdPersonCamera");
                camGo.transform.SetParent(transform, false);
                rigCamera = camGo.AddComponent<Camera>();
            }
            _yaw = transform.eulerAngles.y;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            Look();
            Move();
        }

        private void LateUpdate() => PlaceCamera();

        private void Look()
        {
            var settings = SettingsStore.Current;
            Vector2 delta = Vector2.zero;
            var mouse = Mouse.current;
            if (mouse != null) delta += mouse.delta.ReadValue() * lookSensitivity;
            var pad = Gamepad.current;
            if (pad != null) delta += pad.rightStick.ReadValue() * stickLookSpeed * Time.deltaTime;
            delta *= settings.mouseSensitivity;
            _yaw += delta.x;
            float dy = settings.invertY ? -delta.y : delta.y;
            _pitch = Mathf.Clamp(_pitch - dy, minPitch, maxPitch);
            if (rigCamera != null) rigCamera.fieldOfView = settings.fieldOfView;
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

            // Camera-relative, flattened to the ground plane.
            Quaternion flatYaw = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 forward = flatYaw * Vector3.forward;
            Vector3 right = flatYaw * Vector3.right;
            Vector3 wish = (forward * input.y + right * input.x);
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            Vector3 move = wish * moveSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -1f;
            _verticalVelocity += gravity * Time.deltaTime;
            move.y = _verticalVelocity;
            _controller.Move(move * Time.deltaTime);

            // Turn the visible body toward movement.
            Transform t = body != null ? body : transform;
            if (wish.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(wish, Vector3.up);
                t.rotation = Quaternion.Slerp(t.rotation, target, bodyTurnSpeed * Time.deltaTime);
            }
        }

        private void PlaceCamera()
        {
            if (rigCamera == null) return;
            Vector3 pivot = transform.position + Vector3.up * pivotHeight;
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desired = pivot + rot * (Vector3.back * distance);

            // Pull in if something is between the pivot and the camera.
            Vector3 dir = desired - pivot;
            float dist = dir.magnitude;
            if (dist > 0.001f && Physics.SphereCast(pivot, 0.25f, dir.normalized, out RaycastHit hit, dist, cameraCollision, QueryTriggerInteraction.Ignore))
                desired = pivot + dir.normalized * Mathf.Max(0.6f, hit.distance - 0.1f);

            rigCamera.transform.position = desired;
            rigCamera.transform.rotation = Quaternion.LookRotation(pivot - desired, Vector3.up);
        }
    }
}
