using System;
using UnityEngine;
using UnityEngine.XR;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// VR input via a simple controller-velocity heuristic plus buttons:
    ///   right-hand forward thrust = primary cast
    ///   left-hand forward thrust  = secondary cast (lightning / ice / boulder / air scythe)
    ///   right primary button (A)  = dash (air dash for air channelers)
    ///   right secondary button (B)= defend
    /// Deliberately minimal to prove the pipeline; replace the thrust heuristic with XR
    /// Interaction Toolkit gesture recognition later.
    /// </summary>
    public sealed class VrInputProvider : MonoBehaviour, IPlayerInputProvider
    {
        public event Action<ChannelingIntent> IntentProduced;

        [SerializeField] private Transform head; // assign the HMD camera for aim direction
        [SerializeField] private float thrustVelocityThreshold = 2.0f;
        [SerializeField] private float castCooldown = 0.35f;

        private float _rightCooldown;
        private float _leftCooldown;
        private bool _prevPrimary;
        private bool _prevSecondary;

        private void Update()
        {
            if (_rightCooldown > 0f) _rightCooldown -= Time.deltaTime;
            if (_leftCooldown > 0f) _leftCooldown -= Time.deltaTime;

            Vector3 aim = head ? head.forward : transform.forward;
            TryThrust(XRNode.RightHand, IntentType.PrimaryCast, ref _rightCooldown, aim);
            TryThrust(XRNode.LeftHand, IntentType.SecondaryCast, ref _leftCooldown, aim);
            ReadButtons(aim);
        }

        private void TryThrust(XRNode node, IntentType type, ref float cooldown, Vector3 aim)
        {
            if (cooldown > 0f) return;
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid) return;
            if (!device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity)) return;

            float forwardSpeed = Vector3.Dot(velocity, aim);
            if (forwardSpeed >= thrustVelocityThreshold)
            {
                float charge = Mathf.InverseLerp(thrustVelocityThreshold, thrustVelocityThreshold * 3f, forwardSpeed);
                IntentProduced?.Invoke(new ChannelingIntent(type, aim, charge));
                cooldown = castCooldown;
            }
        }

        private void ReadButtons(Vector3 aim)
        {
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (!right.isValid) return;

            right.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryNow);
            right.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryNow);

            if (primaryNow && !_prevPrimary) IntentProduced?.Invoke(new ChannelingIntent(IntentType.Dash, aim));
            if (secondaryNow && !_prevSecondary) IntentProduced?.Invoke(new ChannelingIntent(IntentType.Defend, aim));

            _prevPrimary = primaryNow;
            _prevSecondary = secondaryNow;
        }
    }
}
