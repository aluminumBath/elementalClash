using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Procedural camera shake driven by combat animation events. Lives on a flat / third-person camera (never VR —
    /// shaking a headset causes nausea); <see cref="ThirdPersonRig"/> adds it to its camera. It runs after the rig
    /// has placed the camera each LateUpdate (high execution order) and adds a decaying world-space offset on top,
    /// so it neither fights nor accumulates against the rig's placement (the rig overwrites position next frame).
    /// Subscribes to <see cref="AnimationEventReceiver"/>'s static events, reacting to the player — or any future
    /// entity — without a direct reference.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(2000)]
    public sealed class CameraShaker : MonoBehaviour
    {
        private const float Frequency = 28f;
        private float _elapsed, _duration, _amplitude, _seedX, _seedY;

        private void OnEnable()
        {
            AnimationEventReceiver.AnyImpacted += OnImpact;
            AnimationEventReceiver.AnyLanded += OnLand;
            AnimationEventReceiver.AnyWasHurt += OnHurt;
            CombatFeedback.Hit += OnCombatHit;
        }

        private void OnDisable()
        {
            AnimationEventReceiver.AnyImpacted -= OnImpact;
            AnimationEventReceiver.AnyLanded -= OnLand;
            AnimationEventReceiver.AnyWasHurt -= OnHurt;
            CombatFeedback.Hit -= OnCombatHit;
        }

        private void OnImpact(Vector3 _) => Shake(0.12f, 0.18f);
        private void OnLand(Vector3 _) => Shake(0.22f, 0.30f);
        private void OnHurt(Vector3 _) => Shake(0.28f, 0.35f);

        // Real hits shake the view only when they land close to this camera (you're in the fight) and carry weight,
        // so distant skirmishes stay calm. Strength scales the shake; taking damage yourself is ~0 m away.
        private void OnCombatHit(Vector3 pos, float amount, Element _)
        {
            if ((pos - transform.position).sqrMagnitude > 144f) return; // ignore hits > 12 m away
            float i = HitFeedback.Intensity01(amount, 40f);
            if (i < 0.15f) return;
            Shake(0.06f + 0.16f * i, 0.16f + 0.16f * i);
        }

        /// <summary>Trigger a shake. A stronger incoming shake takes over a weaker one already in progress.</summary>
        public void Shake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;
            if (amplitude < _amplitude && _elapsed < _duration) return; // let the bigger hit win
            _amplitude = amplitude;
            _duration = duration;
            _elapsed = 0f;
            _seedX = Random.value * 10f;
            _seedY = Random.value * 10f + 5f;
        }

        private void LateUpdate()
        {
            if (_elapsed >= _duration) return;
            _elapsed += Time.unscaledDeltaTime; // unscaled so hit-stop slow-mo doesn't stall the shake
            ShakeOffset.Evaluate(_elapsed, _duration, _amplitude, Frequency, _seedX, _seedY, out float x, out float y);
            // Rig already set our world position this frame; add the shake in the camera's screen plane on top.
            transform.position += transform.right * x + transform.up * y;
        }
    }
}
