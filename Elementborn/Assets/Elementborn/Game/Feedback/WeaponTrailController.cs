using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(TrailRenderer))]
    public sealed class WeaponTrailController : MonoBehaviour
    {
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private float defaultTrailSeconds = 0.18f;

        private float disableAt;

        private void Awake()
        {
            if (trail == null)
            {
                trail = GetComponent<TrailRenderer>();
            }

            SetTrail(false);
        }

        private void Update()
        {
            if (trail != null && trail.emitting && Time.unscaledTime >= disableAt)
            {
                SetTrail(false);
            }
        }

        public void PlayTrail()
        {
            PlayTrail(defaultTrailSeconds);
        }

        public void PlayTrail(float seconds)
        {
            if (trail == null)
            {
                return;
            }

            disableAt = Time.unscaledTime + Mathf.Max(0.01f, seconds);
            trail.Clear();
            SetTrail(true);
        }

        public void StopTrail()
        {
            SetTrail(false);
        }

        private void SetTrail(bool active)
        {
            if (trail != null)
            {
                trail.emitting = active;
            }
        }
    }
}
