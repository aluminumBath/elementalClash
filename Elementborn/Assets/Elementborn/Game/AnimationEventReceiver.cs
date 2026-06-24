using System;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// The target for Unity Animation Events on the player's rigged clips. Add an event on a clip (e.g. a
    /// "Footstep" event on the frame a foot plants, an "AttackImpact" event on the contact frame) and point it
    /// at the matching method here. Each hook plays the right sound through <see cref="AudioController"/> and
    /// raises a C# event carrying the world position, so VFX, camera shake, hit-stop, or damage timing can hook
    /// in without touching this file. Lives on the same GameObject as the Animator;
    /// <see cref="PlayerModelBinder"/> adds it to the rigged model automatically.
    ///
    /// Author the event names exactly: Footstep, FootstepLeft, FootstepRight, FootstepWater, Jump, Land,
    /// AttackWindup, AttackSwing, AttackImpact, CastCharge, CastRelease, Dodge, Hurt, Vocalize.
    /// </summary>
    public sealed class AnimationEventReceiver : MonoBehaviour
    {
        // Subscribe for polish (dust puffs, slash trails, camera shake, hit-stop, …). Vector3 = where it happened.
        public event Action<Vector3, bool> Stepped;   // bool = left foot
        public event Action<Vector3> Jumped;
        public event Action<Vector3> Landed;
        public event Action<Vector3> AttackWoundUp;
        public event Action<Vector3> Swung;
        public event Action<Vector3> Impacted;        // contact frame — best place for hit-stop / camera shake
        public event Action<Vector3> CastCharged;
        public event Action<Vector3> CastReleased;
        public event Action<Vector3> Dodged;
        public event Action<Vector3> WasHurt;
        public event Action<Vector3> Vocalized;

        // Static broadcast channel for global "game feel" services (camera shake, hit-stop, flashes) so they can
        // react to any character's events without holding a reference. Only the feel-relevant hooks broadcast.
        public static event Action<Vector3> AnyImpacted;
        public static event Action<Vector3> AnyLanded;
        public static event Action<Vector3> AnyCastReleased;
        public static event Action<Vector3> AnyWasHurt;

        public bool LastFootLeft { get; private set; }

        private Vector3 P => transform.position;
        private static AudioController Sfx => AudioController.Instance;

        // ---- footsteps ----
        public void Footstep() { LastFootLeft = !LastFootLeft; Sfx?.Footstep(P); Stepped?.Invoke(P, LastFootLeft); }
        public void FootstepLeft() { LastFootLeft = true; Sfx?.Footstep(P); Stepped?.Invoke(P, true); }
        public void FootstepRight() { LastFootLeft = false; Sfx?.Footstep(P); Stepped?.Invoke(P, false); }
        public void FootstepWater() { LastFootLeft = !LastFootLeft; Sfx?.Footstep(P, water: true); Stepped?.Invoke(P, LastFootLeft); }

        // ---- jump / land ----
        public void Jump() { Sfx?.Jump(P); Jumped?.Invoke(P); }
        public void Land() { Sfx?.Land(P); Landed?.Invoke(P); AnyLanded?.Invoke(P); }

        // ---- melee / weapon swings ----
        public void AttackWindup() { AttackWoundUp?.Invoke(P); }                       // anticipation marker
        public void AttackSwing() { Sfx?.Play(SfxKind.WhooshShort); Swung?.Invoke(P); }
        public void AttackImpact() { Sfx?.PlayAt(SfxKind.HitSoft, P, 0.9f); Impacted?.Invoke(P); AnyImpacted?.Invoke(P); }

        // ---- channeling / casts ----
        public void CastCharge() { CastCharged?.Invoke(P); }                           // charge-up marker
        public void CastRelease() { Sfx?.PlayAt(SfxKind.WindWhoosh, P, 0.9f); CastReleased?.Invoke(P); AnyCastReleased?.Invoke(P); }

        // ---- reactions ----
        public void Dodge() { Sfx?.Play(SfxKind.WhooshShort, 0.6f); Dodged?.Invoke(P); }
        public void Hurt() { Sfx?.PlayAt(SfxKind.HitSoft, P, 1f); WasHurt?.Invoke(P); AnyWasHurt?.Invoke(P); }
        public void Vocalize() { Vocalized?.Invoke(P); }                               // effort/voice marker
    }
}
