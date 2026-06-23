using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>Which controller family the active gamepad belongs to (drives face-button labels/glyphs).</summary>
    public enum GamepadBrand { Xbox, PlayStation, Switch, Generic }

    /// <summary>
    /// Turns a rebindable <see cref="InputBindings"/> action into the glyph the player should see right now:
    /// a short token ("A", "X", "RT", "E", "LMB") for either the keyboard/mouse or the gamepad binding,
    /// picked by which device was used most recently (tracked by <see cref="InputDeviceMonitor"/>) and, for
    /// gamepads, by the detected <see cref="Brand"/> (Xbox A/B/X/Y, PlayStation ✕/○/□/△, Switch positions).
    /// Because it reads each binding's <c>effectivePath</c>, tokens follow any rebind automatically. Also
    /// exposes the sprite name an artist's glyph set would use, and an optional <see cref="Sprite"/> lookup
    /// against <c>Resources/ElementbornUI/glyphs/</c> (null until those sprites are imported).
    /// </summary>
    public static class ControlGlyphs
    {
        /// <summary>True when the most recent input came from a gamepad (drives which token prompts show).</summary>
        public static bool UsingGamepad { get; private set; }
        public static void SetUsingGamepad(bool value) => UsingGamepad = value;

        /// <summary>The detected family of the active gamepad. Defaults to Xbox-style labels.</summary>
        public static GamepadBrand Brand { get; private set; } = GamepadBrand.Xbox;
        public static void SetBrand(GamepadBrand brand) => Brand = brand;

        private static InputDeviceMonitor _monitor;

        /// <summary>Spawn the active-device watcher once (idempotent).</summary>
        public static void EnsureMonitor()
        {
            if (_monitor != null) return;
            var go = new GameObject("InputDeviceMonitor");
            Object.DontDestroyOnLoad(go);
            _monitor = go.AddComponent<InputDeviceMonitor>();
        }

        private static readonly Dictionary<string, string> Pad = new Dictionary<string, string>
        {
            { "buttonSouth", "A" }, { "buttonEast", "B" }, { "buttonWest", "X" }, { "buttonNorth", "Y" },
            { "leftShoulder", "LB" }, { "rightShoulder", "RB" }, { "leftTrigger", "LT" }, { "rightTrigger", "RT" },
            { "start", "Start" }, { "select", "Select" }, { "leftStick", "L-Stick" }, { "rightStick", "R-Stick" },
            { "dpad/up", "D-Up" }, { "dpad/down", "D-Down" }, { "dpad/left", "D-Left" }, { "dpad/right", "D-Right" },
        };

        private static readonly Dictionary<string, string> PadPS = new Dictionary<string, string>
        {
            { "buttonSouth", "✕" }, { "buttonEast", "○" }, { "buttonWest", "□" }, { "buttonNorth", "△" },
            { "leftShoulder", "L1" }, { "rightShoulder", "R1" }, { "leftTrigger", "L2" }, { "rightTrigger", "R2" },
            { "start", "Options" }, { "select", "Share" },
        };

        private static readonly Dictionary<string, string> PadSwitch = new Dictionary<string, string>
        {
            { "buttonSouth", "B" }, { "buttonEast", "A" }, { "buttonWest", "Y" }, { "buttonNorth", "X" },
            { "leftShoulder", "L" }, { "rightShoulder", "R" }, { "leftTrigger", "ZL" }, { "rightTrigger", "ZR" },
            { "start", "+" }, { "select", "−" },
        };

        private static string PadToken(string control)
        {
            if (Brand == GamepadBrand.PlayStation && PadPS.TryGetValue(control, out var p)) return p;
            if (Brand == GamepadBrand.Switch && PadSwitch.TryGetValue(control, out var s)) return s;
            return Pad.TryGetValue(control, out var g) ? g : Prettify(control);
        }

        private static readonly Dictionary<string, string> Key = new Dictionary<string, string>
        {
            { "space", "Space" }, { "escape", "Esc" }, { "enter", "Enter" }, { "tab", "Tab" },
            { "leftShift", "Shift" }, { "rightShift", "Shift" }, { "leftCtrl", "Ctrl" }, { "rightCtrl", "Ctrl" },
            { "leftAlt", "Alt" }, { "rightAlt", "Alt" },
            { "upArrow", "Up" }, { "downArrow", "Down" }, { "leftArrow", "Left" }, { "rightArrow", "Right" },
        };

        private static readonly Dictionary<string, string> MouseBtn = new Dictionary<string, string>
        {
            { "leftButton", "LMB" }, { "rightButton", "RMB" }, { "middleButton", "MMB" },
        };

        /// <summary>Token for the binding matching the currently-active device.</summary>
        public static string Token(InputAction action) => Token(action, ActiveIndex(action));

        /// <summary>Token for a specific binding (0 = keyboard/mouse, 1 = gamepad).</summary>
        public static string Token(InputAction action, int bindingIndex)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) return "?";
            return Token(action.bindings[bindingIndex].effectivePath);
        }

        public static string Token(string effectivePath)
        {
            if (string.IsNullOrEmpty(effectivePath)) return "—";
            Split(effectivePath, out string device, out string control);
            switch (device)
            {
                case "Gamepad": return PadToken(control);
                case "Mouse": return MouseBtn.TryGetValue(control, out var m) ? m : Prettify(control);
                case "Keyboard":
                    if (Key.TryGetValue(control, out var k)) return k;
                    if (control.Length == 1) return control.ToUpperInvariant();
                    if (control.Length >= 2 && control.Length <= 3 && control[0] == 'f' && char.IsDigit(control[1]))
                        return control.ToUpperInvariant(); // f1..f12
                    return Prettify(control);
                default: return Prettify(control);
            }
        }

        /// <summary>"[A] Ride" / "[E] Ride" — bracketed, device-aware, rebind-aware.</summary>
        public static string Prompt(InputAction action, string verb) => $"[{Token(action)}] {verb}";

        public static string SpriteName(InputAction action)
        {
            int i = ActiveIndex(action);
            return (action == null || i >= action.bindings.Count) ? null : SpriteName(action.bindings[i].effectivePath);
        }

        public static string SpriteName(string effectivePath)
        {
            if (string.IsNullOrEmpty(effectivePath)) return null;
            Split(effectivePath, out string device, out string control);
            switch (device)
            {
                case "Gamepad": return PadSprite(control);
                case "Mouse": return "mouse_" + control.Replace("Button", "").ToLowerInvariant();
                case "Keyboard": return "key_" + control.ToLowerInvariant();
                default: return null;
            }
        }

        /// <summary>Sprite for the active-device binding, or null if the glyph set isn't imported.</summary>
        public static Sprite Sprite(InputAction action)
        {
            string name = SpriteName(action);
            return string.IsNullOrEmpty(name) ? null : Resources.Load<Sprite>("ElementbornUI/glyphs/" + name);
        }

        private static string FaceSprite(string control)
        {
            if (Brand == GamepadBrand.PlayStation)
                switch (control)
                {
                    case "buttonSouth": return "gp_ps_cross";
                    case "buttonEast": return "gp_ps_circle";
                    case "buttonWest": return "gp_ps_square";
                    case "buttonNorth": return "gp_ps_triangle";
                }
            if (Brand == GamepadBrand.Switch)
                switch (control)
                {
                    case "buttonSouth": return "gp_sw_b";
                    case "buttonEast": return "gp_sw_a";
                    case "buttonWest": return "gp_sw_y";
                    case "buttonNorth": return "gp_sw_x";
                }
            switch (control)
            {
                case "buttonSouth": return "gp_a";
                case "buttonEast": return "gp_b";
                case "buttonWest": return "gp_x";
                case "buttonNorth": return "gp_y";
                default: return null;
            }
        }

        private static string PadSprite(string control)
        {
            switch (control)
            {
                case "buttonSouth":
                case "buttonEast":
                case "buttonWest":
                case "buttonNorth": return FaceSprite(control);
            }
            if (Brand == GamepadBrand.PlayStation)
                switch (control)
                {
                    case "leftShoulder": return "gp_ps_l1";
                    case "rightShoulder": return "gp_ps_r1";
                    case "leftTrigger": return "gp_ps_l2";
                    case "rightTrigger": return "gp_ps_r2";
                    case "start": return "gp_ps_options";
                    case "select": return "gp_ps_share";
                }
            if (Brand == GamepadBrand.Switch)
                switch (control)
                {
                    case "leftShoulder": return "gp_sw_l";
                    case "rightShoulder": return "gp_sw_r";
                    case "leftTrigger": return "gp_sw_zl";
                    case "rightTrigger": return "gp_sw_zr";
                    case "start": return "gp_sw_plus";
                    case "select": return "gp_sw_minus";
                }
            switch (control)
            {
                case "leftShoulder": return "gp_lb";
                case "rightShoulder": return "gp_rb";
                case "leftTrigger": return "gp_lt";
                case "rightTrigger": return "gp_rt";
                case "start": return "gp_start";
                case "select": return "gp_select";
                case "dpad/up": return "gp_dup";
                case "dpad/down": return "gp_ddown";
                case "dpad/left": return "gp_dleft";
                case "dpad/right": return "gp_dright";
                default: return "gp_" + control.Replace("/", "_").ToLowerInvariant();
            }
        }

        /// <summary>Best-effort controller-family detection from the device's layout/name/description.</summary>
        public static GamepadBrand DetectBrand(Gamepad g)
        {
            if (g == null) return GamepadBrand.Xbox;
            string hay = string.Join(" ", new[] { g.layout, g.name, g.displayName, g.description.product, g.description.manufacturer })
                .ToLowerInvariant();
            if (Has(hay, "dualsense", "dualshock", "playstation", "ps4", "ps5", "sony")) return GamepadBrand.PlayStation;
            if (Has(hay, "switch", "joy-con", "joycon", "nintendo", "npad", "pro controller")) return GamepadBrand.Switch;
            if (Has(hay, "xbox", "xinput")) return GamepadBrand.Xbox;
            return GamepadBrand.Generic;
        }

        private static bool Has(string hay, params string[] needles)
        {
            foreach (var n in needles) if (hay.Contains(n)) return true;
            return false;
        }

        private static int ActiveIndex(InputAction action)
        {
            if (action == null) return 0;
            int idx = UsingGamepad ? 1 : 0;
            if (idx >= action.bindings.Count) idx = 0;
            return idx;
        }

        private static void Split(string path, out string device, out string control)
        {
            device = string.Empty; control = path;
            int gt = path.IndexOf('>');
            if (path.Length > 1 && path[0] == '<' && gt > 0)
            {
                device = path.Substring(1, gt - 1);
                control = (gt + 2 <= path.Length) ? path.Substring(gt + 2) : string.Empty; // skip ">/"
            }
        }

        private static string Prettify(string control)
        {
            if (string.IsNullOrEmpty(control)) return "—";
            var sb = new StringBuilder();
            for (int i = 0; i < control.Length; i++)
            {
                char ch = control[i];
                if (i > 0 && char.IsUpper(ch)) sb.Append(' ');
                sb.Append(i == 0 ? char.ToUpperInvariant(ch) : ch);
            }
            return sb.ToString();
        }
    }
}
