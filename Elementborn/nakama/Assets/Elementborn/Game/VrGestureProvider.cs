using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Motion-first VR combat input with a stance layer. Samples both controllers, runs the engine-agnostic
    /// <see cref="GestureRecognizer"/>, and builds a per-hand <see cref="HandInput"/> (pose from grip+trigger,
    /// height, hold-to-charge timer, recognized motion). A <see cref="StanceResolver"/> handles two-handed
    /// stances and stance-modified combos first (a held Guard block, and Water's fist+paddle "ice flow"
    /// channel); otherwise single-hand strikes flow through the active element's <see cref="GestureProfile"/>.
    /// Drops into <see cref="PlayerCombatController"/> like any other <see cref="IPlayerInputProvider"/>,
    /// superseding the simpler <see cref="VrInputProvider"/>. Buttons remain reliable fallbacks (A = dash,
    /// B = defend). Comfort: one-shot casts are rate-limited; channels tick on <c>channelInterval</c>.
    /// </summary>
    public sealed class VrGestureProvider : MonoBehaviour, IPlayerInputProvider
    {
        public event Action<ChannelingIntent> IntentProduced;

        [SerializeField] private Transform head;
        [SerializeField] private Element element = Element.Fire;

        [Header("Motion")]
        [SerializeField] private float minSpeed = 1.8f;
        [SerializeField] private float maxSpeed = 6f;        // speed mapped to full charge
        [SerializeField] private float castCooldown = 0.4f;
        [SerializeField] private float windowSeconds = 0.28f;

        [Header("Stance")]
        [Tooltip("grip & trigger above this (0-1) = a clenched fist.")]
        [SerializeField] private float fistThreshold = 0.6f;
        [Tooltip("grip & trigger below this (0-1) = an open palm.")]
        [SerializeField] private float openThreshold = 0.25f;
        [Tooltip("A held pose only charges while the hand is slower than this.")]
        [SerializeField] private float holdSpeedMax = 0.5f;
        [Tooltip("Seconds between repeats while channeling (ice flow / guard).")]
        [SerializeField] private float channelInterval = 0.22f;
        [SerializeField] private bool buttonFallbacks = true; // A = dash, B = defend

        [Header("Hidden moves")]
        [Tooltip("Wrist angular speed (rad/s) that triggers Water's spin-dash underwater.")]
        [SerializeField] private float spinAngularSpeed = 8f;
        [Tooltip("Accumulated body-turn degrees that trigger Air's tornado.")]
        [SerializeField] private float fullTurnDegrees = 330f;
        [SerializeField] private float spinWindow = 2f;
        [Tooltip("Hand-to-head distance for Fire's breath (palm at the mouth).")]
        [SerializeField] private float mouthDistance = 0.3f;
        [SerializeField] private float signatureCooldown = 1.2f;

        private GestureRecognizer _recognizer;
        private GestureProfile _profile;
        private readonly StanceResolver _stance = new StanceResolver();
        private readonly List<GestureSample> _right = new List<GestureSample>(64);
        private readonly List<GestureSample> _left = new List<GestureSample>(64);
        private float _cooldown;
        private float _channelTimer;
        private float _rightHold;
        private float _leftHold;
        private bool _prevPrimaryBtn, _prevSecondaryBtn;
        private float _sigCooldown;
        private float _lastYaw;
        private float _yawAccum;
        private float _yawWindow;

        /// <summary>Switch the active fighting style (e.g., when a Confluence player changes element).</summary>
        public void SetElement(Element e)
        {
            element = e;
            _profile = GestureProfile.For(e);
        }

        private void OnEnable()
        {
            _recognizer = new GestureRecognizer(minSpeed);
            var inv = PlayerInventory.Instance;
            if (inv != null && inv.Loadout != null && inv.Loadout.IsChanneler)
                element = inv.Loadout.Elements[0];
            _profile = GestureProfile.For(element);
            if (head != null) _lastYaw = head.eulerAngles.y;
        }

        private void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (_channelTimer > 0f) _channelTimer -= Time.deltaTime;
            if (_sigCooldown > 0f) _sigCooldown -= Time.deltaTime;

            Vector3 aim = head ? head.forward : transform.forward;
            Vector3 up = head ? head.up : Vector3.up;
            float headY = head ? head.position.y : 0f;

            Sample(XRNode.RightHand, _right);
            Sample(XRNode.LeftHand, _left);

            HandInput rh = BuildHand(XRNode.RightHand, _right, ref _rightHold, aim, up, headY);
            HandInput lh = BuildHand(XRNode.LeftHand, _left, ref _leftHold, aim, up, headY);

            // 0. Hidden signature move (a special per-element gesture). Checked first.
            if (TryDetectSignature(lh, rh, aim)) return;

            // 1. Stances + stance-modified combos (guard block, ice flow). May channel.
            var stance = _stance.Resolve(element, lh, rh);
            if (!stance.IsNone)
            {
                if (stance.Sustained)
                {
                    if (_channelTimer <= 0f)
                    {
                        Emit(stance.Intent, aim, stance.Charge);
                        _channelTimer = channelInterval;
                    }
                }
                else if (_cooldown <= 0f)
                {
                    Fire(stance.Intent, aim, stance.Charge);
                }
                return;
            }

            // 2. Air two-hand push.
            if (_cooldown <= 0f && TryPush(aim, up)) return;

            // 3. Single-hand strikes, with hold-to-charge.
            if (_cooldown <= 0f)
            {
                if (TryStrike(rh, aim, ref _rightHold, _right)) return;
                if (TryStrike(lh, aim, ref _leftHold, _left)) return;
            }

            if (buttonFallbacks) ReadButtons(aim);
        }

        private void Sample(XRNode node, List<GestureSample> buf)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (!device.isValid) { buf.Clear(); return; }
            device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 vel);
            device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos);
            buf.Add(new GestureSample(pos, vel, Time.time));

            float cutoff = Time.time - windowSeconds;
            while (buf.Count > 0 && buf[0].Time < cutoff) buf.RemoveAt(0);
            if (buf.Count > 64) buf.RemoveAt(0);
        }

        private HandInput BuildHand(XRNode node, List<GestureSample> buf, ref float hold,
            Vector3 aim, Vector3 up, float headY)
        {
            HandPose pose = HandPose.Neutral;
            bool raised = false;

            var device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid)
            {
                device.TryGetFeatureValue(CommonUsages.grip, out float grip);
                device.TryGetFeatureValue(CommonUsages.trigger, out float trig);
                if (grip > fistThreshold && trig > fistThreshold) pose = HandPose.Fist;
                else if (grip < openThreshold && trig < openThreshold) pose = HandPose.OpenPalm;

                device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos);
                raised = pos.y > headY - 0.1f;
            }

            GestureKind motion = _recognizer.Recognize(buf, aim, up);
            float speed = PeakSpeed(buf);

            // Hold-to-charge: build while a pose is held still; release on a neutral hand. A strike consumes it.
            if (pose == HandPose.Neutral) hold = 0f;
            else if (motion == GestureKind.None && speed < holdSpeedMax) hold += Time.deltaTime;

            return new HandInput(pose, motion, speed, raised, hold);
        }

        private bool TryStrike(HandInput hand, Vector3 aim, ref float hold, List<GestureSample> buf)
        {
            if (hand.Motion == GestureKind.None) return false;
            IntentType intent = _profile.IntentFor(hand.Motion);
            if (intent == IntentType.None) return false;

            float charge = Mathf.Max(VelocityCharge(hand.MotionSpeed), StanceCharge(hold));
            Fire(intent, aim, charge);
            hold = 0f;
            buf.Clear();
            return true;
        }

        private bool TryPush(Vector3 aim, Vector3 up)
        {
            if (!_profile.Handles(GestureKind.Push)) return false;
            if (_recognizer.Recognize(_right, aim, up) == GestureKind.Thrust &&
                _recognizer.Recognize(_left, aim, up) == GestureKind.Thrust)
            {
                Fire(_profile.IntentFor(GestureKind.Push), aim, Mathf.Max(ChargeFrom(_right), ChargeFrom(_left)));
                _right.Clear(); _left.Clear();
                return true;
            }
            return false;
        }

        private void Emit(IntentType intent, Vector3 aim, float charge)
            => IntentProduced?.Invoke(new ChannelingIntent(intent, aim, charge));

        private void Fire(IntentType intent, Vector3 aim, float charge)
        {
            Emit(intent, aim, charge);
            _cooldown = castCooldown;
        }

        private float VelocityCharge(float speed) => Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        private static float StanceCharge(float hold) => Mathf.Clamp01(hold / 1.5f);
        private float ChargeFrom(List<GestureSample> buf) => VelocityCharge(PeakSpeed(buf));

        private static float PeakSpeed(List<GestureSample> buf)
        {
            float peak = 0f;
            for (int i = 0; i < buf.Count; i++) peak = Mathf.Max(peak, buf[i].Velocity.magnitude);
            return peak;
        }

        private void ReadButtons(Vector3 aim)
        {
            var right = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (!right.isValid) return;
            right.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryNow);
            right.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryNow);
            if (primaryNow && !_prevPrimaryBtn) IntentProduced?.Invoke(new ChannelingIntent(IntentType.Dash, aim));
            if (secondaryNow && !_prevSecondaryBtn) IntentProduced?.Invoke(new ChannelingIntent(IntentType.Defend, aim));
            _prevPrimaryBtn = primaryNow;
            _prevSecondaryBtn = secondaryNow;
        }

        // ---- Hidden signature moves ----

        private bool TryDetectSignature(HandInput lh, HandInput rh, Vector3 aim)
        {
            // Track body-turn for Air's tornado (only while both hands are raised).
            if (head != null)
            {
                float yaw = head.eulerAngles.y;
                float delta = Mathf.DeltaAngle(_lastYaw, yaw);
                _lastYaw = yaw;
                if (element == Element.Air && lh.Raised && rh.Raised)
                {
                    _yawWindow -= Time.deltaTime;
                    if (_yawWindow <= 0f) { _yawAccum = 0f; _yawWindow = spinWindow; }
                    _yawAccum += delta;
                }
                else { _yawAccum = 0f; _yawWindow = spinWindow; }
            }

            if (_sigCooldown > 0f) return false;

            switch (element)
            {
                case Element.Water: // wrist-spin propels you forward — underwater only
                    if (WaterVolume.Submerged(transform.position) != null &&
                        (Spinning(XRNode.RightHand) || Spinning(XRNode.LeftHand)))
                        return EmitSignature(aim);
                    break;

                case Element.Earth: // crossed arms = rock armor
                    if (head != null && TryHandPos(XRNode.LeftHand, out var lp) && TryHandPos(XRNode.RightHand, out var rp)
                        && HiddenGestures.HandsCrossedHigh(lp, rp, head.position))
                        return EmitSignature(aim);
                    break;

                case Element.Air: // full-body spin with arms raised = tornado
                    if (lh.Raised && rh.Raised && HiddenGestures.CompletedFullTurn(_yawAccum, fullTurnDegrees))
                    {
                        _yawAccum = 0f;
                        return EmitSignature(aim);
                    }
                    break;

                case Element.Fire: // hand at the mouth + a button = fire breath
                    if (head != null && TryHandPos(XRNode.RightHand, out var hp)
                        && HiddenGestures.HandNearHead(hp, head.position, mouthDistance) && ButtonHeld(XRNode.RightHand))
                        return EmitSignature(aim);
                    break;
            }
            return false;
        }

        private bool Spinning(XRNode node)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            return d.isValid && d.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out Vector3 ang)
                && HiddenGestures.WristSpinning(ang, spinAngularSpeed);
        }

        private static bool TryHandPos(XRNode node, out Vector3 pos)
        {
            pos = Vector3.zero;
            var d = InputDevices.GetDeviceAtXRNode(node);
            return d.isValid && d.TryGetFeatureValue(CommonUsages.devicePosition, out pos);
        }

        private static bool ButtonHeld(XRNode node)
        {
            var d = InputDevices.GetDeviceAtXRNode(node);
            return d.isValid && d.TryGetFeatureValue(CommonUsages.triggerButton, out bool t) && t;
        }

        private bool EmitSignature(Vector3 aim)
        {
            Emit(IntentType.Signature, aim, 0f);
            _sigCooldown = signatureCooldown;
            return true;
        }
    }
}
