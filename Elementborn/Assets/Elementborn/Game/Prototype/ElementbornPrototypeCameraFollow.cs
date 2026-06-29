using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeCameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 5f, -8f);
        public float followSpeed = 8f;
        public float lookHeight = 1.2f;

        private void LateUpdate()
        {
            if (target == null)
            {
                ElementbornPrototypePlayerController player = FindAnyObjectByType<ElementbornPrototypePlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target == null)
            {
                return;
            }

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);

            Vector3 lookTarget = target.position + Vector3.up * lookHeight;
            if ((lookTarget - transform.position).sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookTarget - transform.position, Vector3.up),
                    Time.deltaTime * followSpeed);
            }
        }
    }
}
