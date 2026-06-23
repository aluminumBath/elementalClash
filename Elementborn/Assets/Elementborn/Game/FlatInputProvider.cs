using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Flat combat input. Reads the rebindable <see cref="InputBindings"/> actions (keyboard/mouse and
    /// gamepad): PrimaryCast/SecondaryCast are hold-to-charge, release-to-cast; Defend and Dash are taps.
    /// Holding the rebindable <b>power modifier</b> (Left Shift / Right Bumper) turns those casts into the
    /// advanced moves — Primary→Heavy, Secondary→Sweep, Defend→Signature (see <see cref="Core.ExtendedCast"/>).
    /// Movement and look live in <see cref="FirstPersonRig"/>/<see cref="ThirdPersonRig"/>; this only emits
    /// combat intents. Aim comes from the camera's forward, so the third-person rig just points it elsewhere.
    /// </summary>
    public sealed class FlatInputProvider : MonoBehaviour, IPlayerInputProvider
    {
        public event Action<ChannelingIntent> IntentProduced;

        [SerializeField] private Camera aimCamera;
        [SerializeField] private float maxChargeSeconds = 1.2f;

        private float _primaryHeld;
        private float _secondaryHeld;

        private void Reset() => aimCamera = Camera.main;

        private void OnEnable() { InputBindings.Enable(); ControlGlyphs.EnsureMonitor(); }

        private void Update()
        {
            Vector3 aim = aimCamera ? aimCamera.transform.forward : transform.forward;
            bool mod = InputBindings.ExtendedCast.IsPressed(); // hold to throw Heavy / Sweep / Signature

            var primary = InputBindings.PrimaryCast;
            if (primary.IsPressed()) _primaryHeld += Time.deltaTime;
            else if (primary.WasReleasedThisFrame()) { Emit(ExtendedCast.Remap(IntentType.PrimaryCast, mod), aim, _primaryHeld); _primaryHeld = 0f; }

            var secondary = InputBindings.SecondaryCast;
            if (secondary.IsPressed()) _secondaryHeld += Time.deltaTime;
            else if (secondary.WasReleasedThisFrame()) { Emit(ExtendedCast.Remap(IntentType.SecondaryCast, mod), aim, _secondaryHeld); _secondaryHeld = 0f; }

            if (InputBindings.Defend.WasPressedThisFrame()) Emit(ExtendedCast.Remap(IntentType.Defend, mod), aim, 0f);
            if (InputBindings.Dash.WasPressedThisFrame()) Emit(IntentType.Dash, aim, 0f);
        }

        private void Emit(IntentType type, Vector3 aim, float heldSeconds)
        {
            float charge = Mathf.Clamp01(heldSeconds / maxChargeSeconds);
            IntentProduced?.Invoke(new ChannelingIntent(type, aim, charge));
        }
    }
}
