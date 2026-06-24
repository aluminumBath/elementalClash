using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// The flat/gamepad control scheme, built in code (no .inputactions asset needed). Each discrete action
    /// carries a keyboard/mouse binding (index 0) and a gamepad binding (index 1); both are rebindable at
    /// runtime via <see cref="StartRebind"/> (the Input System's interactive rebinding), and overrides persist
    /// to JSON next to the save/settings files. Movement and camera look are handled directly by the rigs
    /// (WASD/stick, mouse/stick) and are intentionally not part of the rebindable set. VR (Quest) uses XRI
    /// controller bindings and is unaffected by any of this.
    /// </summary>
    public static class InputBindings
    {
        public sealed class Entry
        {
            public readonly string Label;
            public readonly InputAction Action;
            public Entry(string label, InputAction action) { Label = label; Action = action; }
        }

        private const string FileName = "elementborn_bindings.json";
        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private static InputActionMap _map;

        /// <summary>Raised after bindings change (rebind, reset) so a rebinding menu can refresh.</summary>
        public static event Action Changed;

        public static InputAction PrimaryCast { get; private set; }
        public static InputAction SecondaryCast { get; private set; }
        public static InputAction Defend { get; private set; }
        public static InputAction Dash { get; private set; }
        public static InputAction Jump { get; private set; }            // tap: jump; hold while falling: glide
        public static InputAction Interact { get; private set; }
        public static InputAction ElementTravel { get; private set; }
        public static InputAction Mount { get; private set; }
        public static InputAction Companion { get; private set; }
        public static InputAction Menu { get; private set; }
        public static InputAction Slots { get; private set; }
        public static InputAction ExtendedCast { get; private set; } // hold: Primary->Heavy, Secondary->Sweep, Defend->Signature
        public static InputAction Guard { get; private set; }         // hold to raise guard (parry window on raise, else block)
        public static InputAction MountSkill { get; private set; }    // while mounted: the mount's special move (Charge/Surge/Divebomb)

        public static IReadOnlyList<Entry> Rebindable { get; private set; }

        public static InputActionMap Map { get { EnsureBuilt(); return _map; } }

        // A static ctor guarantees the map exists and is enabled before any action property is read.
        static InputBindings() { EnsureBuilt(); }

        public static void Enable() { EnsureBuilt(); try { _map.Enable(); } catch { /* edit-mode / no input system */ } }

        private static void EnsureBuilt()
        {
            if (_map != null) return;
            _map = new InputActionMap("Gameplay");

            PrimaryCast   = Button("PrimaryCast",   "<Mouse>/leftButton",  "<Gamepad>/rightTrigger");
            SecondaryCast = Button("SecondaryCast", "<Mouse>/rightButton", "<Gamepad>/leftTrigger");
            Defend        = Button("Defend",        "<Keyboard>/f",        "<Gamepad>/leftShoulder");
            Dash          = Button("Dash",          "<Keyboard>/leftAlt",  "<Gamepad>/dpad/left");
            Jump          = Button("Jump",          "<Keyboard>/space",    "<Gamepad>/buttonSouth");
            Interact      = Button("Interact",      "<Keyboard>/e",        "<Gamepad>/buttonWest");
            ElementTravel = Button("ElementTravel", "<Keyboard>/f",        "<Gamepad>/buttonNorth");
            Mount         = Button("Mount",         "<Keyboard>/m",        "<Gamepad>/dpad/up");
            Companion     = Button("Companion",     "<Keyboard>/c",        "<Gamepad>/dpad/down");
            Menu          = Button("Menu",          "<Keyboard>/escape",   "<Gamepad>/start");
            Slots         = Button("Slots",         "<Keyboard>/f8",       "<Gamepad>/select");
            ExtendedCast  = Button("ExtendedCast",  "<Keyboard>/leftShift","<Gamepad>/rightShoulder");
            Guard         = Button("Guard",         "<Keyboard>/leftCtrl", "<Gamepad>/buttonEast");
            MountSkill    = Button("MountSkill",    "<Keyboard>/q",        "<Gamepad>/rightStickPress");

            Rebindable = new List<Entry>
            {
                new Entry("Primary cast", PrimaryCast),
                new Entry("Secondary cast", SecondaryCast),
                new Entry("Defend", Defend),
                new Entry("Guard", Guard),
                new Entry("Dash", Dash),
                new Entry("Jump / glide", Jump),
                new Entry("Interact", Interact),
                new Entry("Element travel", ElementTravel),
                new Entry("Summon mount", Mount),
                new Entry("Summon companions", Companion),
                new Entry("Open settings", Menu),
                new Entry("Save slots", Slots),
                new Entry("Heavy/Sweep/Signature (hold)", ExtendedCast),
                new Entry("Mount skill", MountSkill),
            };

            LoadOverrides();
            try { _map.Enable(); } catch { /* enabled at runtime; harmless to skip in edit mode */ }
        }

        private static InputAction Button(string name, string keyboardMouse, string gamepad)
        {
            var a = _map.AddAction(name, InputActionType.Button);
            a.AddBinding(keyboardMouse); // index 0 = keyboard/mouse
            a.AddBinding(gamepad);       // index 1 = gamepad
            return a;
        }

        // ---- display + rebinding ------------------------------------------------------
        /// <summary>Human-readable control name for a binding (e.g., "Left Button", "Right Trigger").</summary>
        public static string Display(InputAction action, int bindingIndex)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) return "-";
            string path = action.bindings[bindingIndex].effectivePath;
            if (string.IsNullOrEmpty(path)) return "Unbound";
            return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        /// <summary>Listen for the next control and rebind it (index 0 = keyboard/mouse, 1 = gamepad).</summary>
        public static void StartRebind(InputAction action, int bindingIndex, Action onComplete)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count) { onComplete?.Invoke(); return; }
            EnsureBuilt();

            bool wasEnabled = _map.enabled;
            action.Disable();

            var op = action.PerformInteractiveRebinding(bindingIndex)
                .WithExpectedControlType("Button")
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Mouse>/scroll")
                .WithCancelingThrough("<Keyboard>/escape");

            if (bindingIndex == 1)
            {
                op = op.WithControlsHavingToMatchPath("<Gamepad>");
            }
            else
            {
                op = op.WithControlsHavingToMatchPath("<Keyboard>");
                op = op.WithControlsHavingToMatchPath("<Mouse>");
            }

            op.OnComplete(o =>
                {
                    o.Dispose();
                    if (wasEnabled) action.Enable();
                    SaveOverrides();
                    Changed?.Invoke();
                    onComplete?.Invoke();
                })
              .OnCancel(o =>
                {
                    o.Dispose();
                    if (wasEnabled) action.Enable();
                    Changed?.Invoke();
                    onComplete?.Invoke();
                })
              .Start();
        }

        /// <summary>Clear every override, restoring the default scheme, and delete the saved overrides.</summary>
        public static void ResetAll()
        {
            EnsureBuilt();
            foreach (var e in Rebindable) e.Action.RemoveAllBindingOverrides();
            try { if (File.Exists(FilePath)) File.Delete(FilePath); }
            catch (Exception ex) { Debug.LogWarning($"[Elementborn] Bindings reset failed: {ex.Message}"); }
            Changed?.Invoke();
        }

        // ---- persistence (asset-free; stores per-action override paths) ---------------
        [Serializable] private class Ovr { public string action; public int index; public string path; }
        [Serializable] private class OvrList { public List<Ovr> items = new List<Ovr>(); }

        public static void SaveOverrides()
        {
            EnsureBuilt();
            var list = new OvrList();
            foreach (var e in Rebindable)
            {
                var a = e.Action;
                for (int i = 0; i < a.bindings.Count; i++)
                    if (!string.IsNullOrEmpty(a.bindings[i].overridePath))
                        list.items.Add(new Ovr { action = a.name, index = i, path = a.bindings[i].overridePath });
            }
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(list, true)); }
            catch (Exception ex) { Debug.LogWarning($"[Elementborn] Bindings save failed: {ex.Message}"); }
        }

        public static void LoadOverrides()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                var list = JsonUtility.FromJson<OvrList>(File.ReadAllText(FilePath));
                if (list?.items == null) return;
                foreach (var o in list.items)
                {
                    var a = _map.FindAction(o.action);
                    if (a != null && o.index >= 0 && o.index < a.bindings.Count)
                        a.ApplyBindingOverride(o.index, o.path);
                }
            }
            catch (Exception ex) { Debug.LogWarning($"[Elementborn] Bindings load failed: {ex.Message}"); }
        }
    }
}
