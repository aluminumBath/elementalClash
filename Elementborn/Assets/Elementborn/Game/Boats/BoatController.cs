using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// Wind-Waker-inspired small boat controller: sail up for wind-driven speed, sail down for slow
    /// manual movement, optional small-boat jump, and deterministic wind response. Wind only affects
    /// this boat because this component explicitly reads WorldWindController.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BoatController : MonoBehaviour
    {
        [Header("Pilot")]
        [SerializeField] private Transform pilotSeat;
        [SerializeField] private bool requirePilot = true;

        [Header("Sail")]
        [SerializeField] private bool sailRaised;
        [SerializeField] private Transform sailVisual;
        [SerializeField] private Vector3 sailLoweredEuler = new Vector3(80f, 0f, 0f);
        [SerializeField] private Vector3 sailRaisedEuler = Vector3.zero;
        [SerializeField] private float sailAcceleration = 5.5f;
        [SerializeField] private float headwindBrake = 2.25f;
        [SerializeField] private float maxSailSpeed = 18f;

        [Header("Manual movement, sail down")]
        [SerializeField] private float paddleAcceleration = 3.25f;
        [SerializeField] private float reverseAcceleration = 1.75f;
        [SerializeField] private float maxPaddleSpeed = 6f;

        [Header("Steering")]
        [SerializeField] private float steerDegreesPerSecond = 95f;
        [SerializeField] private float movingSteerBoost = 0.65f;

        [Header("Small boat jump")]
        [SerializeField] private bool smallBoatCanJump = true;
        [SerializeField] private float jumpImpulse = 5.25f;
        [SerializeField] private float waterY = 0f;
        [SerializeField] private float waterSnapTolerance = 0.55f;

        [Header("Drag / buoyancy")]
        [SerializeField] private float waterDrag = 1.4f;
        [SerializeField] private float idleStopDrag = 4f;
        [SerializeField] private float buoyancySpring = 22f;
        [SerializeField] private float buoyancyDamping = 4f;

        public bool HasPilot { get; private set; }
        public Transform Pilot { get; private set; }
        public bool SailRaised => sailRaised;
        public Transform PilotSeat => pilotSeat != null ? pilotSeat : transform;
        public float WaterY => waterY;
        public Vector3 Velocity => _rb != null ? _rb.velocity : Vector3.zero;
        public float PlanarSpeed => PlanarVelocity().magnitude;
        public float NormalizedSpeed => Mathf.Clamp01(PlanarSpeed / Mathf.Max(0.1f, sailRaised ? maxSailSpeed : maxPaddleSpeed));
        public bool IsOnWaterSurface => IsOnWater();

        private Rigidbody _rb;
        private float _throttle;
        private float _steer;
        private bool _jumpQueued;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.drag = waterDrag;
            ApplySailVisual(true);
        }

        private void Update()
        {
            if (requirePilot && !HasPilot) return;

            ReadMoveInput();

            if (InputBindings.MountSkill.WasPressedThisFrame()) ToggleSail();
            if (smallBoatCanJump && InputBindings.Jump.WasPressedThisFrame()) _jumpQueued = true;
        }

        private void FixedUpdate()
        {
            ApplyBuoyancy();
            if (requirePilot && !HasPilot) return;

            ApplySteering();
            if (sailRaised) ApplySailForces();
            else ApplyPaddleForces();

            ClampPlanarSpeed(sailRaised ? maxSailSpeed : maxPaddleSpeed);
            _jumpQueued = false;
        }

        public void Board(Transform pilot)
        {
            Pilot = pilot;
            HasPilot = pilot != null;
        }

        public void Disembark()
        {
            Pilot = null;
            HasPilot = false;
            _throttle = 0f;
            _steer = 0f;
        }

        public void ToggleSail()
        {
            sailRaised = !sailRaised;
            ApplySailVisual(false);
            GameHud.Instance?.Toast(sailRaised ? "Sail raised" : "Sail lowered");
        }

        public void SetSail(bool raised)
        {
            sailRaised = raised;
            ApplySailVisual(false);
        }

        private void ReadMoveInput()
        {
            float forward = 0f;
            float steer = 0f;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) forward += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) forward -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) steer -= 1f;
            }

            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                forward += stick.y;
                steer += stick.x;
            }

            _throttle = Mathf.Clamp(forward, -1f, 1f);
            _steer = Mathf.Clamp(steer, -1f, 1f);
        }

        private void ApplySailForces()
        {
            _rb.drag = waterDrag;
            Vector3 wind = WorldWindController.ActiveVelocity;
            Vector3 windDir = wind.sqrMagnitude > 0.001f ? wind.normalized : transform.forward;
            float alignment = Vector3.Dot(transform.forward, windDir); // 1 = wind from behind, -1 = headwind
            float tailwind = Mathf.Clamp01(alignment);
            float crosswind = 1f - Mathf.Abs(alignment);
            float headwind = Mathf.Clamp01(-alignment);

            // Tailwind is strongest, crosswind gives a weak readable push, headwind brakes.
            Vector3 force = transform.forward * ((tailwind + crosswind * 0.22f) * wind.magnitude * sailAcceleration);
            _rb.AddForce(force, ForceMode.Acceleration);

            if (headwind > 0f && PlanarSpeed > 0.05f)
                _rb.AddForce(-PlanarVelocity().normalized * (headwind * headwindBrake), ForceMode.Acceleration);

            // Player can still trim/paddle lightly with the sail up.
            if (Mathf.Abs(_throttle) > 0.01f)
                _rb.AddForce(transform.forward * (_throttle * paddleAcceleration * 0.35f), ForceMode.Acceleration);
        }

        private void ApplyPaddleForces()
        {
            float accel = _throttle >= 0f ? paddleAcceleration : reverseAcceleration;
            if (Mathf.Abs(_throttle) > 0.01f)
                _rb.AddForce(transform.forward * (_throttle * accel), ForceMode.Acceleration);

            _rb.drag = Mathf.Abs(_throttle) < 0.05f ? idleStopDrag : waterDrag;

            if (_jumpQueued && IsOnWater())
                _rb.AddForce(Vector3.up * jumpImpulse, ForceMode.VelocityChange);
        }

        private void ApplySteering()
        {
            float speedFactor = Mathf.Clamp01(PlanarSpeed / Mathf.Max(0.1f, maxSailSpeed));
            float steerPower = steerDegreesPerSecond * (1f + speedFactor * movingSteerBoost);
            Quaternion delta = Quaternion.Euler(0f, _steer * steerPower * Time.fixedDeltaTime, 0f);
            _rb.MoveRotation(_rb.rotation * delta);
        }

        private void ApplyBuoyancy()
        {
            float displacement = waterY - transform.position.y;
            if (displacement > -waterSnapTolerance)
            {
                float lift = displacement * buoyancySpring - _rb.velocity.y * buoyancyDamping;
                _rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
            }
        }

        private void ClampPlanarSpeed(float maxSpeed)
        {
            Vector3 planar = PlanarVelocity();
            if (planar.magnitude <= maxSpeed) return;
            Vector3 clamped = planar.normalized * maxSpeed;
            _rb.velocity = new Vector3(clamped.x, _rb.velocity.y, clamped.z);
        }

        private Vector3 PlanarVelocity() => _rb != null ? new Vector3(_rb.velocity.x, 0f, _rb.velocity.z) : Vector3.zero;
        private bool IsOnWater() => Mathf.Abs(transform.position.y - waterY) <= waterSnapTolerance && (_rb == null || _rb.velocity.y <= 0.25f);

        private void ApplySailVisual(bool instant)
        {
            if (sailVisual == null) return;
            Quaternion target = Quaternion.Euler(sailRaised ? sailRaisedEuler : sailLoweredEuler);
            sailVisual.localRotation = target; // replace with animation/tween later
        }
    }
}
