namespace Elementborn.Core
{
    /// <summary>
    /// Maps a character's planar move speed to a 0..1 locomotion blend (0 = idle, 0.5 = walk, 1 = run) for an
    /// Animator blend tree, plus a frame-rate-independent damped approach so the blend eases instead of snapping.
    /// Pure and unit-tested; the Game-layer <c>PlayerModelBinder</c> feeds it the rig's speed and pushes the
    /// result into the rigged model's Animator.
    /// </summary>
    public static class LocomotionAnimation
    {
        public const float IdleThreshold = 0.1f;

        /// <summary>0 at/below idle, 0.5 at walkSpeed, 1 at/above runSpeed, linear between.</summary>
        public static float Blend(float speed, float walkSpeed, float runSpeed)
        {
            if (speed <= IdleThreshold) return 0f;
            if (walkSpeed <= 0f) walkSpeed = 0.01f;
            if (runSpeed <= walkSpeed) runSpeed = walkSpeed + 0.01f;
            if (speed <= walkSpeed) return 0.5f * (speed / walkSpeed);
            if (speed >= runSpeed) return 1f;
            return 0.5f + 0.5f * ((speed - walkSpeed) / (runSpeed - walkSpeed));
        }

        public static bool IsMoving(float speed) => speed > IdleThreshold;

        /// <summary>Ease <paramref name="current"/> toward <paramref name="target"/> at <paramref name="rate"/>
        /// per second, independent of frame time (exponential smoothing).</summary>
        public static float Damp(float current, float target, float rate, float dt)
        {
            if (rate <= 0f || dt <= 0f) return target;
            float t = 1f - (float)System.Math.Exp(-rate * dt);
            return current + (target - current) * t;
        }
    }
}
