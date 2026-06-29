using UnityEngine;

namespace Elementborn.Game
{
    public enum ElementbornPrototypeInteractableKind
    {
        Generic,
        GuideNpc,
        ShardResource,
        ReturnPoint
    }

    public sealed class ElementbornPrototypeInteractable : MonoBehaviour
    {
        private const string RadiusName = "Prototype Interaction Radius";

        public ElementbornPrototypeInteractableKind kind;
        public string displayName = "Interactable";

        [Header("Prototype Interaction")]
        [Min(0.5f)] public float activationRadius = 4f;
        public bool createTriggerRadius = true;
        public bool facePromptTowardCamera = true;

        private SphereCollider radiusCollider;

        private void Awake()
        {
            EnsureInteractionRadius();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureInteractionRadius();
        }
#endif

        public string GetPrompt()
        {
            switch (kind)
            {
                case ElementbornPrototypeInteractableKind.GuideNpc:
                    return "Talk to " + displayName;
                case ElementbornPrototypeInteractableKind.ShardResource:
                    return "Collect " + displayName;
                case ElementbornPrototypeInteractableKind.ReturnPoint:
                    return "Use " + displayName;
                default:
                    return displayName;
            }
        }

        public void Interact()
        {
            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.HandleInteraction(this);
            }
        }

        public bool IsPlayerInRange(Transform player)
        {
            if (player == null)
            {
                return false;
            }

            return Vector3.Distance(player.position, transform.position) <= activationRadius;
        }

        public void EnsureInteractionRadius()
        {
            if (!createTriggerRadius)
            {
                return;
            }

            if (radiusCollider == null)
            {
                Transform existing = transform.Find(RadiusName);
                GameObject radiusObject = existing != null ? existing.gameObject : new GameObject(RadiusName);
                radiusObject.transform.SetParent(transform, false);
                radiusObject.transform.localPosition = Vector3.zero;
                radiusObject.transform.localRotation = Quaternion.identity;
                radiusObject.transform.localScale = Vector3.one;

                radiusCollider = radiusObject.GetComponent<SphereCollider>();
                if (radiusCollider == null)
                {
                    radiusCollider = radiusObject.AddComponent<SphereCollider>();
                }
            }

            radiusCollider.isTrigger = true;
            radiusCollider.radius = Mathf.Max(0.5f, activationRadius);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.5f, activationRadius));
        }
    }
}
