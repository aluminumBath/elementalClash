using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeElementalAbility : MonoBehaviour
    {
        [Header("Attunement")]
        public ElementbornPrototypeElementType currentElement = ElementbornPrototypeElementType.Fire;

        [Header("Ability")]
        public KeyCode abilityKey = KeyCode.Q;
        public float cooldownSeconds = 0.65f;
        public float damage = 25f;
        public float spawnForwardOffset = 1.25f;
        public float spawnHeight = 1.05f;
        public float aimAssistRange = 22f;
        public float aimAssistConeDot = 0.25f;

        private float nextCastTime;
        private Camera playCamera;

        public float CooldownRemaining => Mathf.Max(0f, nextCastTime - Time.time);
        public float Cooldown01 => cooldownSeconds <= 0f ? 0f : Mathf.Clamp01(CooldownRemaining / cooldownSeconds);

        private void Update()
        {
            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                currentElement = manager.selectedElement;
                if (!manager.HasStarted)
                {
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetElement(ElementbornPrototypeElementType.Fire);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetElement(ElementbornPrototypeElementType.Water);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetElement(ElementbornPrototypeElementType.Earth);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetElement(ElementbornPrototypeElementType.Air);
            }

            if (Input.GetKeyDown(abilityKey))
            {
                TryCast();
            }
        }

        public void SetElement(ElementbornPrototypeElementType element)
        {
            currentElement = element;
            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.SetElement(element);
            }
        }

        public bool TryCast()
        {
            if (Time.time < nextCastTime)
            {
                return false;
            }

            nextCastTime = Time.time + cooldownSeconds;
            playCamera = playCamera != null ? playCamera : Camera.main;

            Vector3 direction = GetCastDirection();
            Vector3 spawn = transform.position + Vector3.up * spawnHeight + direction * spawnForwardOffset;

            GameObject projectile = GameObject.CreatePrimitive(GetProjectilePrimitive(currentElement));
            projectile.name = "Prototype " + ElementbornPrototypeVisualUtility.GetElementName(currentElement) + " Bolt";
            projectile.transform.position = spawn;
            projectile.transform.localScale = GetProjectileScale(currentElement);

            ElementbornPrototypeProjectile bolt = projectile.AddComponent<ElementbornPrototypeProjectile>();
            bolt.hitRadius = currentElement == ElementbornPrototypeElementType.Water ? 0.95f : 0.65f;
            bolt.Configure(currentElement, direction, damage);

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("Cast " + ElementbornPrototypeVisualUtility.GetElementName(currentElement) + " bolt.");
            }

            return true;
        }

        private Vector3 GetCastDirection()
        {
            Vector3 forward = playCamera != null ? playCamera.transform.forward : transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = transform.forward;
            }

            forward.Normalize();

            ElementbornPrototypeDummyEnemy bestDummy = null;
            float bestScore = float.MaxValue;
            ElementbornPrototypeDummyEnemy[] dummies =
                FindObjectsByType<ElementbornPrototypeDummyEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < dummies.Length; i++)
            {
                ElementbornPrototypeDummyEnemy dummy = dummies[i];
                if (dummy == null || dummy.currentHealth <= 0f)
                {
                    continue;
                }

                Vector3 toDummy = dummy.transform.position - transform.position;
                toDummy.y = 0f;
                float distance = toDummy.magnitude;
                if (distance <= 0.001f || distance > aimAssistRange)
                {
                    continue;
                }

                Vector3 toDummyDir = toDummy / distance;
                float dot = Vector3.Dot(forward, toDummyDir);
                if (dot < aimAssistConeDot)
                {
                    continue;
                }

                float score = distance - dot * 4f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestDummy = dummy;
                }
            }

            if (bestDummy != null)
            {
                Vector3 assisted = bestDummy.transform.position + Vector3.up * 0.8f - (transform.position + Vector3.up * spawnHeight);
                assisted.y = 0f;
                if (assisted.sqrMagnitude > 0.001f)
                {
                    return assisted.normalized;
                }
            }

            return forward;
        }

        private PrimitiveType GetProjectilePrimitive(ElementbornPrototypeElementType element)
        {
            switch (element)
            {
                case ElementbornPrototypeElementType.Earth:
                    return PrimitiveType.Cube;
                case ElementbornPrototypeElementType.Air:
                    return PrimitiveType.Capsule;
                default:
                    return PrimitiveType.Sphere;
            }
        }

        private Vector3 GetProjectileScale(ElementbornPrototypeElementType element)
        {
            switch (element)
            {
                case ElementbornPrototypeElementType.Earth:
                    return new Vector3(0.5f, 0.5f, 0.5f);
                case ElementbornPrototypeElementType.Air:
                    return new Vector3(0.32f, 0.72f, 0.32f);
                case ElementbornPrototypeElementType.Water:
                    return new Vector3(0.7f, 0.7f, 0.7f);
                default:
                    return new Vector3(0.48f, 0.48f, 0.48f);
            }
        }
    }
}
