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
        private bool _counted;
        private void OnEnable() { if (!_counted) { UiGate.Push(); _counted = true; } }
        private void OnDisable() { if (_counted) { UiGate.Pop(); _counted = false; } }
    }
}
