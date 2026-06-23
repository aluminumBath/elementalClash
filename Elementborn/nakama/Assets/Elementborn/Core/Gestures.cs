using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>A recognized hand motion. Single-hand strikes plus two-hand/stance kinds resolved by the provider.</summary>
    public enum GestureKind
    {
        None,
        Thrust,    // straight forward punch
        Jab,       // short fast forward (folded into Thrust by the default recognizer)
        Whip,      // lateral sweeping arc
        Hook,      // curved forward+lateral
        Uppercut,  // rising strike
        Slam,      // downward strike
        Push,      // two-hand forward shove (provider-derived)
        Guard,     // both hands raised, held (provider-derived)
        DashStep,  // quick step/lunge (provider- or button-derived)
        Paddle     // forward-and-down scooping arc (canoe-paddle stroke)
    }

    /// <summary>One motion sample for a hand: world position + velocity at a moment.</summary>
    public readonly struct GestureSample
    {
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }
        public float Time { get; }

        public GestureSample(Vector3 position, Vector3 velocity, float time)
        {
            Position = position;
            Velocity = velocity;
            Time = time;
        }
    }

    /// <summary>
    /// Motion-only, single-hand gesture recognizer. Given a short window of samples and a reference frame
    /// (the head's forward/up), it classifies the dominant strike at peak velocity: forward = Thrust, lateral
    /// = Whip, up = Uppercut, down = Slam, and below the speed floor = None. Pure and engine-agnostic so it is
    /// unit-tested with recorded motion — no headset required. Stance/two-hand kinds (Push, Guard) are layered
    /// on top by the VR provider; richer pose recognition can replace this later without touching callers.
    /// </summary>
    public sealed class GestureRecognizer
    {
        public float MinSpeed { get; }

        public GestureRecognizer(float minSpeed = 1.5f)
        {
            MinSpeed = minSpeed;
        }

        public GestureKind Recognize(IReadOnlyList<GestureSample> window, Vector3 forward, Vector3 up)
        {
            if (window == null || window.Count < 2) return GestureKind.None;

            int peak = -1;
            float best = 0f;
            for (int i = 0; i < window.Count; i++)
            {
                float s = window[i].Velocity.magnitude;
                if (s > best) { best = s; peak = i; }
            }
            if (peak < 0 || best < MinSpeed) return GestureKind.None;

            Vector3 f = forward.sqrMagnitude > 1e-4f ? forward.normalized : Vector3.forward;
            Vector3 u = up.sqrMagnitude > 1e-4f ? up.normalized : Vector3.up;
            Vector3 r = Vector3.Cross(u, f);
            r = r.sqrMagnitude > 1e-4f ? r.normalized : Vector3.right;

            Vector3 dir = window[peak].Velocity / best;
            float df = Vector3.Dot(dir, f);
            float du = Vector3.Dot(dir, u);
            float dr = Vector3.Dot(dir, r);
            float af = Mathf.Abs(df), au = Mathf.Abs(du), ar = Mathf.Abs(dr);

            // A forward-and-down scoop is a canoe-paddle stroke (used by stance combos like ice flow).
            if (df > 0.35f && du < -0.35f) return GestureKind.Paddle;

            if (au >= af && au >= ar)
                return du >= 0f ? GestureKind.Uppercut : GestureKind.Slam;
            if (af >= ar)
                return df >= 0f ? GestureKind.Thrust : GestureKind.None; // backward retract = nothing
            return GestureKind.Whip; // lateral dominant
        }
    }

    /// <summary>
    /// The per-element style table: which gestures an element responds to, and the intent each maps to.
    /// One fixed style per element gives each its own motion vocabulary — Fire is linear (thrust/uppercut),
    /// Water flowing (whip/slam), Earth heavy (slam/uppercut), Air light (two-hand push/whip). A gesture not
    /// in the active element's table does nothing, which keeps each style distinct. The intents flow through
    /// the existing <see cref="AbilitySystem"/>; deeper per-gesture abilities are a future extension.
    /// </summary>
    public sealed class GestureProfile
    {
        private readonly Dictionary<GestureKind, IntentType> _map;

        public GestureProfile(Dictionary<GestureKind, IntentType> map)
        {
            _map = map ?? new Dictionary<GestureKind, IntentType>();
        }

        public IntentType IntentFor(GestureKind gesture)
            => _map.TryGetValue(gesture, out var intent) ? intent : IntentType.None;

        public bool Handles(GestureKind gesture)
            => _map.TryGetValue(gesture, out var intent) && intent != IntentType.None;

        public static GestureProfile For(Element element)
        {
            switch (element)
            {
                case Element.Fire: // aggressive, linear
                    return new GestureProfile(new Dictionary<GestureKind, IntentType>
                    {
                        { GestureKind.Thrust, IntentType.PrimaryCast },     // straight punch = fire blast
                        { GestureKind.Uppercut, IntentType.SecondaryCast }, // rising strike = lightning
                        { GestureKind.Slam, IntentType.Heavy },             // overhead = meteor lob
                        { GestureKind.Whip, IntentType.Sweep },             // arc = fire sweep
                        { GestureKind.DashStep, IntentType.Dash },
                        { GestureKind.Guard, IntentType.Defend },
                    });
                case Element.Water: // flowing, circular
                    return new GestureProfile(new Dictionary<GestureKind, IntentType>
                    {
                        { GestureKind.Whip, IntentType.PrimaryCast },       // sweeping arc = water jet
                        { GestureKind.Slam, IntentType.SecondaryCast },     // downward = ice / sanguine grip
                        { GestureKind.Uppercut, IntentType.Heavy },         // rising = ice geyser
                        { GestureKind.Thrust, IntentType.Sweep },           // straight = water shove
                        { GestureKind.DashStep, IntentType.Dash },
                        { GestureKind.Guard, IntentType.Defend },
                    });
                case Element.Earth: // grounded, heavy
                    return new GestureProfile(new Dictionary<GestureKind, IntentType>
                    {
                        { GestureKind.Slam, IntentType.PrimaryCast },       // heavy downward = rock hurl
                        { GestureKind.Uppercut, IntentType.SecondaryCast }, // rising = boulder
                        { GestureKind.Thrust, IntentType.Heavy },           // drive = ground slam
                        { GestureKind.Whip, IntentType.Sweep },             // arc = rock wall sweep
                        { GestureKind.DashStep, IntentType.Dash },
                        { GestureKind.Guard, IntentType.Defend },
                    });
                case Element.Air: // light, evasive
                    return new GestureProfile(new Dictionary<GestureKind, IntentType>
                    {
                        { GestureKind.Push, IntentType.PrimaryCast },       // two-hand shove = gust
                        { GestureKind.Whip, IntentType.SecondaryCast },     // arc = air scythe
                        { GestureKind.Uppercut, IntentType.Heavy },         // rising = updraft launch
                        { GestureKind.Slam, IntentType.Sweep },             // downward = downburst
                        { GestureKind.DashStep, IntentType.Dash },
                        { GestureKind.Guard, IntentType.Defend },
                    });
                default:
                    return new GestureProfile(new Dictionary<GestureKind, IntentType>());
            }
        }
    }

    /// <summary>How a hand is held this frame. On controllers this is a proxy: grip+trigger clenched = Fist,
    /// fully relaxed = OpenPalm. Hand-tracking could feed real finger poses into the same enum later.</summary>
    public enum HandPose { Neutral, Fist, OpenPalm }

    /// <summary>A per-hand snapshot the VR provider builds each frame, consumed by <see cref="StanceResolver"/>.</summary>
    public readonly struct HandInput
    {
        public HandPose Pose { get; }
        public GestureKind Motion { get; }
        public float MotionSpeed { get; }
        public bool Raised { get; }       // hand at/above head height
        public float HoldSeconds { get; } // how long a pose has been held still (hold-to-charge)

        public HandInput(HandPose pose, GestureKind motion, float motionSpeed, bool raised, float holdSeconds)
        {
            Pose = pose;
            Motion = motion;
            MotionSpeed = motionSpeed;
            Raised = raised;
            HoldSeconds = holdSeconds;
        }
    }

    /// <summary>The action a stance or stance-combo produces: an intent, extra hold-charge (0-1), whether it
    /// channels (the provider repeats it while it persists), and a label for VFX/debug hooks.</summary>
    public readonly struct StanceOutcome
    {
        public IntentType Intent { get; }
        public float Charge { get; }
        public bool Sustained { get; }
        public string Label { get; }
        public bool IsNone => Intent == IntentType.None;

        public StanceOutcome(IntentType intent, float charge, bool sustained, string label)
        {
            Intent = intent;
            Charge = Mathf.Clamp01(charge);
            Sustained = sustained;
            Label = label;
        }

        public static StanceOutcome None => new StanceOutcome(IntentType.None, 0f, false, null);
    }

    /// <summary>
    /// Resolves two-handed stances and stance-modified combos on top of the single-hand motion table.
    /// Universal: both hands raised and still = Guard, a held block (sustained). Per element: Water's
    /// "ice flow" — one hand held in a fist (forming ice) while the other paddles in a forward-down arc
    /// (propelling it), like canoe paddling — channels ice while both persist, charged by how long the fist
    /// has been held. Pure and unit-tested; the provider supplies the snapshots and handles channel timing.
    /// </summary>
    public sealed class StanceResolver
    {
        public StanceOutcome Resolve(Element element, HandInput left, HandInput right)
        {
            var combo = ResolveCombo(element, left, right);
            if (!combo.IsNone) return combo;

            // Universal block: both hands up and not striking.
            bool striking = left.Motion != GestureKind.None || right.Motion != GestureKind.None;
            if (left.Raised && right.Raised && !striking)
                return new StanceOutcome(IntentType.Defend, 0f, true, "Guard");

            return StanceOutcome.None;
        }

        private static StanceOutcome ResolveCombo(Element element, HandInput a, HandInput b)
        {
            if (element == Element.Water)
            {
                // Ice flow: a fist forms the ice, a paddle stroke from the other hand sends it. Either hand may
                // hold the fist. Sustained, so each paddle stroke pushes the flow; charge builds with the hold.
                if (a.Pose == HandPose.Fist && b.Motion == GestureKind.Paddle)
                    return new StanceOutcome(IntentType.SecondaryCast, HoldCharge(a), true, "IceFlow");
                if (b.Pose == HandPose.Fist && a.Motion == GestureKind.Paddle)
                    return new StanceOutcome(IntentType.SecondaryCast, HoldCharge(b), true, "IceFlow");
            }
            return StanceOutcome.None;
        }

        private static float HoldCharge(HandInput h) => Mathf.Clamp01(h.HoldSeconds / 1.5f);
    }
}
