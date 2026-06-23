using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// Watches input each frame and latches whether the player is currently on a gamepad or on
    /// keyboard/mouse, so <see cref="ControlGlyphs"/> can show matching prompts. Auto-spawned via
    /// <see cref="ControlGlyphs.EnsureMonitor"/>; survives scene loads.
    /// </summary>
    public sealed class InputDeviceMonitor : MonoBehaviour
    {
        private int _lastPadId = -1;

        private void Update()
        {
            var pad = Gamepad.current;
            if (pad != null && pad.deviceId != _lastPadId)
            {
                _lastPadId = pad.deviceId;
                ControlGlyphs.SetBrand(ControlGlyphs.DetectBrand(pad));
            }

            if (pad != null && PadActuated(pad)) { ControlGlyphs.SetUsingGamepad(true); return; }
            if (KeyboardMouseActuated()) ControlGlyphs.SetUsingGamepad(false);
        }

        private static bool PadActuated(Gamepad g) =>
            g.leftStick.ReadValue().sqrMagnitude > 0.1f ||
            g.rightStick.ReadValue().sqrMagnitude > 0.1f ||
            g.leftTrigger.ReadValue() > 0.2f || g.rightTrigger.ReadValue() > 0.2f ||
            g.dpad.ReadValue().sqrMagnitude > 0.1f ||
            g.buttonSouth.isPressed || g.buttonEast.isPressed || g.buttonWest.isPressed || g.buttonNorth.isPressed ||
            g.leftShoulder.isPressed || g.rightShoulder.isPressed ||
            g.startButton.isPressed || g.selectButton.isPressed;

        private static bool KeyboardMouseActuated()
        {
            var k = Keyboard.current;
            if (k != null && k.anyKey.isPressed) return true;
            var m = Mouse.current;
            if (m != null && (m.leftButton.isPressed || m.rightButton.isPressed || m.middleButton.isPressed
                              || m.delta.ReadValue().sqrMagnitude > 4f)) return true;
            return false;
        }
    }
}
