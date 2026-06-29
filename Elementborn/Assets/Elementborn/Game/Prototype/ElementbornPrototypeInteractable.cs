using UnityEngine;

namespace Elementborn.Game
{
    public enum ElementbornPrototypeInteractableKind
    {
        Generic,
        GuideNpc,
        ShardResource,
        ReturnPoint,
        ElementGate,
        ResourceNode,
        HealingShrine,
        LootChest,
        LoreStone,
        EnvoyNpc
    }

    public sealed class ElementbornPrototypeInteractable : MonoBehaviour
    {
        private const string RadiusName = "Prototype Interaction Radius";

        public ElementbornPrototypeInteractableKind kind;
        public string displayName = "Interactable";

        [Header("Element")]
        public ElementbornPrototypeElementType element = ElementbornPrototypeElementType.Fire;

        [Header("Element Gate")]
        public ElementbornPrototypeElementType gateElement = ElementbornPrototypeElementType.Fire;
        public ElementbornPrototypeElementGate gateController;

        [Header("One-shot Interaction")]
        public bool consumed;
        public int amount = 1;

        [TextArea(2, 5)]
        public string customText;

        [Header("Prototype Interaction")]
        [Min(0.5f)] public float activationRadius = 4f;
        public bool createTriggerRadius = true;
        public bool facePromptTowardCamera = true;

        private SphereCollider radiusCollider;

        private void Awake()
        {
            ResolveGateController();
            EnsureInteractionRadius();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveGateController();
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
                case ElementbornPrototypeInteractableKind.ElementGate:
                    return "Open " + ElementbornPrototypeVisualUtility.GetElementName(gateElement) + " Gate";
                case ElementbornPrototypeInteractableKind.ResourceNode:
                    return consumed ? displayName + " depleted" : "Harvest " + displayName;
                case ElementbornPrototypeInteractableKind.HealingShrine:
                    return "Rest at " + displayName;
                case ElementbornPrototypeInteractableKind.LootChest:
                    return consumed ? displayName + " opened" : "Open " + displayName;
                case ElementbornPrototypeInteractableKind.LoreStone:
                    return "Read " + displayName;
                case ElementbornPrototypeInteractableKind.EnvoyNpc:
                    return "Talk to " + displayName;
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

        public void ResolveGateController()
        {
            if (gateController == null)
            {
                gateController = GetComponent<ElementbornPrototypeElementGate>();
            }
        }

        public void ResetInteractable()
        {
            consumed = false;

            if (kind == ElementbornPrototypeInteractableKind.ResourceNode ||
                kind == ElementbornPrototypeInteractableKind.LootChest)
            {
                gameObject.SetActive(true);
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            Color color = GetResetColor();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.GetComponent<TextMesh>() == null)
                {
                    renderer.enabled = true;
                    renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial(name + " Reset", color);
                }
            }
        }

        public void MarkConsumed()
        {
            consumed = true;

            if (kind == ElementbornPrototypeInteractableKind.ResourceNode)
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer != null && renderer.GetComponent<TextMesh>() == null)
                    {
                        renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial(name + " Depleted", Color.gray);
                    }
                }
            }
        }

        private Color GetResetColor()
        {
            if (kind == ElementbornPrototypeInteractableKind.ResourceNode)
            {
                return ElementbornPrototypeVisualUtility.GetElementColor(element);
            }

            if (kind == ElementbornPrototypeInteractableKind.LootChest)
            {
                return new Color(0.72f, 0.45f, 0.16f);
            }

            return ElementbornPrototypeVisualUtility.GetElementColor(element);
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
