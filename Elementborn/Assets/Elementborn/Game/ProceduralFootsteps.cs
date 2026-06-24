using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Footsteps without authored clip events: measures how far the body moves and emits a step every stride,
    /// gated on being grounded and actually moving (<see cref="FootstepCadence"/>). It routes through an
    /// <see cref="AnimationEventReceiver"/> when one is present — sharing its sound/VFX path — otherwise plays the
    /// footstep sound directly. <see cref="PlayerModelBinder"/> adds this for the static-mesh fallback; drop it on
    /// any body that lacks authored footstep events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProceduralFootsteps : MonoBehaviour
    {
        [SerializeField] private float strideLength = 1.6f;
        [SerializeField] private float minSpeed = 0.4f;

        private FootstepCadence _gait;
        private AnimationEventReceiver _receiver;
        private CharacterController _controller;
        private Vector3 _lastPos;

        private void Start()
        {
            _gait = new FootstepCadence(strideLength);
            _receiver = GetComponentInChildren<AnimationEventReceiver>();
            _controller = GetComponentInParent<CharacterController>();
            _lastPos = transform.position;
        }

        private void Update()
        {
            Vector3 pos = transform.position;
            Vector3 delta = pos - _lastPos; delta.y = 0f;
            _lastPos = pos;

            float dt = Time.deltaTime;
            float speed = dt > 0f ? delta.magnitude / dt : 0f;
            if (speed < minSpeed) return;
            if (_controller != null && !_controller.isGrounded) return;

            int steps = _gait.Accumulate(delta.magnitude);
            for (int i = 0; i < steps; i++) Step();
        }

        private void Step()
        {
            if (_receiver != null) { _receiver.Footstep(); return; }
            AudioController.Instance?.Footstep(transform.position);
        }
    }
}
