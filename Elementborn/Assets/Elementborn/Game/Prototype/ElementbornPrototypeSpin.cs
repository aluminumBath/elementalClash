using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeSpin : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0f, 70f, 0f);
        public float bobAmplitude;
        public float bobSpeed = 2f;

        private Vector3 startPosition;
        private bool captured;

        private void OnEnable()
        {
            if (!captured)
            {
                startPosition = transform.localPosition;
                captured = true;
            }
        }

        private void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);

            if (bobAmplitude > 0f)
            {
                transform.localPosition = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            }
        }
    }
}
