using UnityEngine;
using UnityEngine.XR;

namespace Elementborn.Game
{
    /// <summary>
    /// VR comfort locomotion for the headset rig: snap (or smooth) turning with a comfort-vignette pulse,
    /// thumbstick walking in the direction you're facing, plus recenter and height-reset. Put it on the XR origin
    /// (the rig root) that has a Camera Offset child with the HMD camera under it. Turning honors the
    /// comfort-vignette setting through <see cref="ComfortVignette"/>. These are sensible starting values —
    /// on-headset comfort (turn angle, move speed, vignette strength) still wants real-device tuning.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class VrComfortLocomotion : MonoBehaviour
    {
        public enum TurnStyle { Snap, Smooth }

        [Header("References")]
        [SerializeField] private Transform cameraOffset;   // between the origin and the HMD camera
        [SerializeField] private Transform head;           // the HMD camera
        [SerializeField] private ComfortVignette comfort;

        [Header("Turning")]
        [SerializeField] private TurnStyle turnStyle = TurnStyle.Snap;
        [SerializeField] private float snapAngle = 45f;
        [SerializeField] private float smoothTurnSpeed = 120f;
        [SerializeField, Range(0.3f, 0.95f)] private float turnDeadzone = 0.7f;

        [Header("Moving")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField, Range(0.05f, 0.5f)] private float moveDeadzone = 0.15f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Comfort vignette")]
        [SerializeField, Range(0f, 1f)] private float moveVignette = 0.35f;
        [SerializeField, Range(0f, 1f)] private float turnVignette = 0.6f;
        [SerializeField] private float turnVignetteDecay = 4f;

        [Header("Height")]
        [SerializeField] private float standingHeight = 1.7f;

        private CharacterController _cc;
        private bool _turnArmed = true;
        private float _verticalVel;
        private float _turnPulse;
        private bool _prevRecenter;
        private bool _prevCalibrate;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (head == null && Camera.main != null) head = Camera.main.transform;
            if (comfort == null && head != null) comfort = head.GetComponentInChildren<ComfortVignette>();
            if (comfort == null) comfort = GetComponentInChildren<ComfortVignette>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float moveMag = Locomote(dt);
            Turn(dt);
            Buttons();
            Vignette(moveMag, dt);
        }

        private float Locomote(float dt)
        {
            Vector2 axis = Stick(XRNode.LeftHand);
            Vector3 planar = Vector3.zero;
            float mag = axis.magnitude;
            if (mag > moveDeadzone && head != null)
            {
                Vector3 fwd = head.forward; fwd.y = 0f; fwd.Normalize();
                Vector3 right = head.right; right.y = 0f; right.Normalize();
                planar = (right * axis.x + fwd * axis.y) * moveSpeed;
            }
            else mag = 0f;

            if (_cc.isGrounded && _verticalVel < 0f) _verticalVel = -2f;
            _verticalVel += gravity * dt;
            Vector3 motion = planar;
            motion.y = _verticalVel;
            _cc.Move(motion * dt);
            return mag;
        }

        private void Turn(float dt)
        {
            float x = Stick(XRNode.RightHand).x;
            if (turnStyle == TurnStyle.Smooth)
            {
                if (Mathf.Abs(x) > turnDeadzone) { RotateAroundHead(x * smoothTurnSpeed * dt); _turnPulse = turnVignette; }
                return;
            }
            if (Mathf.Abs(x) > turnDeadzone && _turnArmed)
            {
                RotateAroundHead(Mathf.Sign(x) * snapAngle);
                _turnArmed = false;
                _turnPulse = turnVignette;
            }
            else if (Mathf.Abs(x) < turnDeadzone * 0.7f)
            {
                _turnArmed = true;
            }
        }

        private void RotateAroundHead(float degrees)
        {
            Vector3 pivot = head != null ? head.position : transform.position;
            transform.RotateAround(pivot, Vector3.up, degrees);
        }

        private void Buttons()
        {
            // Recenter on right thumbstick-click — NOT the A button (A is Dash in combat; sharing it double-fired).
            bool recenter = Button(XRNode.RightHand, CommonUsages.primary2DAxisClick);
            if (recenter && !_prevRecenter) Recenter();
            _prevRecenter = recenter;

            bool calibrate = Button(XRNode.LeftHand, CommonUsages.primaryButton);
            if (calibrate && !_prevCalibrate) CalibrateHeight();
            _prevCalibrate = calibrate;
        }

        /// <summary>Yaw the rig so the headset faces the rig's forward again.</summary>
        public void Recenter()
        {
            if (head == null) return;
            Vector3 hf = head.forward; hf.y = 0f;
            if (hf.sqrMagnitude < 0.0001f) return;
            float headYaw = Quaternion.LookRotation(hf, Vector3.up).eulerAngles.y;
            float rigYaw = transform.eulerAngles.y;
            RotateAroundHead(rigYaw - headYaw);
        }

        /// <summary>Offset the camera so the current head height reads as the standing reference (a seated reset).</summary>
        public void CalibrateHeight()
        {
            if (cameraOffset == null || head == null) return;
            float current = head.position.y - transform.position.y;
            Vector3 p = cameraOffset.localPosition;
            p.y += standingHeight - current;
            cameraOffset.localPosition = p;
        }

        private void Vignette(float moveMag, float dt)
        {
            if (comfort == null) return;
            _turnPulse = Mathf.Max(0f, _turnPulse - turnVignetteDecay * dt);
            float moveAmt = Mathf.Clamp01(moveMag) * moveVignette;
            comfort.SetIntensity(Mathf.Max(moveAmt, _turnPulse));
        }

        private static Vector2 Stick(XRNode node)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            if (d.isValid && d.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis)) return axis;
            return Vector2.zero;
        }

        private static bool Button(XRNode node, InputFeatureUsage<bool> usage)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            return d.isValid && d.TryGetFeatureValue(usage, out bool pressed) && pressed;
        }
    }
}
