using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Adds Wind-Waker-like bobbing/rocking to a visual child only. It does not rotate the Rigidbody root,
    /// so boat control remains stable while the mesh looks alive on the water.
    /// </summary>
    public sealed class BoatWaveVisuals : MonoBehaviour
    {
        [SerializeField] private BoatController boat;
        [Tooltip("The mesh/visual root to rock. If empty, the first child is used; avoid using the Rigidbody root.")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float stillBobAmplitude = 0.08f;
        [SerializeField] private float movingBobAmplitude = 0.16f;
        [SerializeField] private float stillRockDegrees = 2.5f;
        [SerializeField] private float movingRockDegrees = 5f;
        [SerializeField] private float waveFrequency = 0.75f;
        [SerializeField] private float speedFrequencyBoost = 1.2f;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;

        private void Reset()
        {
            boat = GetComponent<BoatController>();
            visualRoot = transform.childCount > 0 ? transform.GetChild(0) : null;
        }

        private void Awake()
        {
            if (boat == null) boat = GetComponent<BoatController>();
            if (visualRoot == null && transform.childCount > 0) visualRoot = transform.GetChild(0);
            if (visualRoot == null) visualRoot = transform;
            _baseLocalPosition = visualRoot.localPosition;
            _baseLocalRotation = visualRoot.localRotation;
        }

        private void LateUpdate()
        {
            if (visualRoot == null) return;

            float speed01 = boat != null ? boat.NormalizedSpeed : 0f;
            float amp = Mathf.Lerp(stillBobAmplitude, movingBobAmplitude, speed01);
            float rock = Mathf.Lerp(stillRockDegrees, movingRockDegrees, speed01);
            float t = Time.time * Mathf.PI * 2f * (waveFrequency + speed01 * speedFrequencyBoost);

            float bob = Mathf.Sin(t) * amp;
            float roll = Mathf.Sin(t * 0.73f + transform.position.x * 0.05f) * rock;
            float pitch = Mathf.Cos(t * 0.61f + transform.position.z * 0.05f) * rock * 0.65f;

            visualRoot.localPosition = _baseLocalPosition + Vector3.up * bob;
            visualRoot.localRotation = _baseLocalRotation * Quaternion.Euler(pitch, 0f, -roll);
        }
    }
}
