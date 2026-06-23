using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>High-level actions, independent of element or input device.</summary>
    public enum IntentType
    {
        None,
        PrimaryCast,   // fire blast, water whip, earth hurl, air push...
        SecondaryCast, // charged / sub-art variant
        Defend,        // shield / earth wall
        Dash,
        Heavy,         // committed power attack (overhead slam / drive)
        Sweep,         // wide horizontal crowd-control arc
        Signature      // a hidden, element-specific signature move (special gesture)
    }

    /// <summary>
    /// A device-agnostic description of what the player wants to do this moment.
    /// VR providers build these from gestures; flat providers from mouse/keyboard.
    /// Combat interprets them in the context of the player's loadout.
    /// </summary>
    public readonly struct ChannelingIntent
    {
        public IntentType Type { get; }
        public Vector3 Direction { get; }

        /// <summary>Normalized 0-1 charge, for hold-to-power abilities.</summary>
        public float Charge { get; }

        public ChannelingIntent(IntentType type, Vector3 direction, float charge = 0f)
        {
            Type = type;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : direction;
            Charge = Mathf.Clamp01(charge);
        }

        public static ChannelingIntent None => new ChannelingIntent(IntentType.None, Vector3.zero);
    }

    /// <summary>
    /// Anything that turns player action into intents. Implemented once for VR and
    /// once for flat play; the rest of the game depends only on this interface.
    /// </summary>
    public interface IPlayerInputProvider
    {
        /// <summary>Raised whenever the player triggers an action.</summary>
        event System.Action<ChannelingIntent> IntentProduced;
    }
}
