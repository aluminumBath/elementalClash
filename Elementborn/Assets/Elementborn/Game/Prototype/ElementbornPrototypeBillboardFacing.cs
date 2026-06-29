using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeBillboardFacing : MonoBehaviour
    {
        public bool faceCamera = true;
        public bool onlyYaw = true;
        public Vector3 extraEulerRotation = Vector3.zero;

        private void LateUpdate()
        {
            if (!faceCamera || Camera.main == null)
            {
                return;
            }

            Vector3 toCamera = transform.position - Camera.main.transform.position;
            if (onlyYaw)
            {
                toCamera.y = 0f;
            }

            if (toCamera.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up) * Quaternion.Euler(extraEulerRotation);
        }
    }
}
