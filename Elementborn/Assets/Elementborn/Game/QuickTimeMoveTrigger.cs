using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Optional, designer-droppable hook for a quick-time "complex move": when the trigger button is pressed (and
    /// no QTE is already running) it starts a <see cref="QuickTimeController"/> sequence and fires UnityEvents on
    /// success/failure — wire those to the actual special move in the inspector. Not attached anywhere by default,
    /// so it never conflicts with controls until a designer adds it. Keyboard ('Q') + gamepad (right shoulder) for
    /// now; VR mapping is a follow-up.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuickTimeMoveTrigger : MonoBehaviour
    {
        [SerializeField] private int steps = 4;
        [SerializeField] private float windowSeconds = QuickTimeSequence.DefaultWindowSeconds;
        [SerializeField] private UnityEvent onMoveExecuted;
        [SerializeField] private UnityEvent onMoveFailed;

        private void Update()
        {
            if (QuickTimeController.IsActive) return;
            if (!TriggerPressed()) return;
            QuickTimeController.Begin(steps, windowSeconds,
                () => onMoveExecuted?.Invoke(),
                () => onMoveFailed?.Invoke());
        }

        private static bool TriggerPressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.qKey.wasPressedThisFrame) return true;
            var gp = Gamepad.current;
            return gp != null && gp.rightShoulder.wasPressedThisFrame;
        }
    }
}
