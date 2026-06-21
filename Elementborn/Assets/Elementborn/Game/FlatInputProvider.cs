using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Flat, first-person input via mouse and keyboard:
    ///   LMB  (hold to charge, release) = primary cast
    ///   RMB  (hold to charge, release) = secondary cast (lightning for fire, ice for water)
    ///   F                              = defend
    ///   Space                          = dash
    /// Movement and look live in <see cref="FirstPersonRig"/>; this only emits combat intents.
    /// Aim comes from the camera's forward, so a future third-person rig only changes that reference.
    /// </summary>
    public sealed class FlatInputProvider : MonoBehaviour, IPlayerInputProvider
    {
        public event Action<ChannelingIntent> IntentProduced;

        [SerializeField] private Camera aimCamera;
        [SerializeField] private float maxChargeSeconds = 1.2f;

        private float _primaryHeld;
        private float _secondaryHeld;

        private void Reset() => aimCamera = Camera.main;

        private void Update()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null) return;

            Vector3 aim = aimCamera ? aimCamera.transform.forward : transform.forward;

            // Primary (LMB)
            if (mouse.leftButton.isPressed)
            {
                _primaryHeld += Time.deltaTime;
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                Emit(IntentType.PrimaryCast, aim, _primaryHeld);
                _primaryHeld = 0f;
            }

            // Secondary (RMB)
            if (mouse.rightButton.isPressed)
            {
                _secondaryHeld += Time.deltaTime;
            }
            else if (mouse.rightButton.wasReleasedThisFrame)
            {
                Emit(IntentType.SecondaryCast, aim, _secondaryHeld);
                _secondaryHeld = 0f;
            }

            if (keyboard == null) return;
            if (keyboard.fKey.wasPressedThisFrame) Emit(IntentType.Defend, aim, 0f);
            if (keyboard.spaceKey.wasPressedThisFrame) Emit(IntentType.Dash, aim, 0f);
        }

        private void Emit(IntentType type, Vector3 aim, float heldSeconds)
        {
            float charge = Mathf.Clamp01(heldSeconds / maxChargeSeconds);
            IntentProduced?.Invoke(new ChannelingIntent(type, aim, charge));
        }
    }
}
