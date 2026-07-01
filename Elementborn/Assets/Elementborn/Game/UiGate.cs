using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>A tiny shared gate so only one screen owns the Escape key at a time. Modal screens raise the count
    /// while they're on-screen (via <see cref="UiGateToken"/>); <see cref="PauseMenu"/> consults <see cref="IsBlocking"/>
    /// so it never opens on top of an already-open menu, and each open menu keeps its own Escape-to-close behaviour.</summary>
    public static class UiGate
    {
        private static int _count;
        private static bool _cursorSaved;
        private static bool _savedVisible;
        private static CursorLockMode _savedLock;

        public static bool IsBlocking => _count > 0;
        public static int OpenCount => _count;

        public static void Push()
        {
            // First modal on-screen: free the cursor so its buttons are clickable (the gameplay rig locks it for
            // mouselook), remembering the current state so the last modal to close can hand it back to gameplay.
            if (_count++ == 0)
            {
                _savedVisible = Cursor.visible;
                _savedLock = Cursor.lockState;
                _cursorSaved = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public static void Pop()
        {
            if (_count > 0 && --_count == 0 && _cursorSaved)
            {
                Cursor.visible = _savedVisible;
                Cursor.lockState = _savedLock;
                _cursorSaved = false;
            }
        }
    }

    /// <summary>Attached to a modal screen's canvas: raises the <see cref="UiGate"/> while the canvas is active and
    /// lowers it when the canvas is hidden or destroyed (OnDisable covers both paths), so the count always matches
    /// what's actually on-screen — whether a panel is toggled with SetActive or torn down entirely.</summary>
    public sealed class UiGateToken : MonoBehaviour
    {
        // Every on-screen modal registers here so a newly opened one can close the previous (single-modal).
        private static readonly List<UiGateToken> Active = new List<UiGateToken>();

        /// <summary>When true (default), enabling this modal closes any other open <b>exclusive</b> modal first, so
        /// only one is up at a time. Screens that intentionally host sub-overlays (the title menu) set this false.</summary>
        public bool exclusive = true;

        /// <summary>The owning panel's close callback, so a forced close runs the owner's real teardown (keeping its
        /// open/closed flag in sync) rather than hiding the canvas behind its back. Set by <see cref="OverlayUi.Panel"/>.</summary>
        public System.Action onForceClose;

        private bool _counted;

        private void OnEnable()
        {
            if (exclusive)
            {
                foreach (var other in Active.ToArray()) // snapshot: closing one mutates Active mid-loop
                {
                    if (other == null || other == this || !other.exclusive) continue;
                    if (other.onForceClose != null) other.onForceClose.Invoke();
                    else other.gameObject.SetActive(false);
                }
            }
            if (!_counted) { UiGate.Push(); _counted = true; }
            if (!Active.Contains(this)) Active.Add(this);
        }

        private void OnDisable()
        {
            Active.Remove(this);
            if (_counted) { UiGate.Pop(); _counted = false; }
        }
    }
}
