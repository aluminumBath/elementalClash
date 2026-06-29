using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeProjectile : MonoBehaviour
    {
        public ElementbornPrototypeElementType element = ElementbornPrototypeElementType.Fire;
        public float speed = 14f;
        public float lifetime = 3f;
        public float damage = 25f;
        public float hitRadius = 0.65f;

        private Vector3 direction = Vector3.forward;
        private Vector3 previousPosition;
        private float destroyAt;
        private bool hasHit;

        public void Configure(ElementbornPrototypeElementType newElement, Vector3 newDirection, float newDamage)
        {
            element = newElement;
            direction = newDirection.sqrMagnitude > 0.001f ? newDirection.normalized : Vector3.forward;
            damage = newDamage;
            destroyAt = Time.time + lifetime;
            previousPosition = transform.position;
            EnsurePhysicsSetup();
            ApplyElementVisual();
        }

        private void Awake()
        {
            destroyAt = Time.time + lifetime;
            previousPosition = transform.position;
            EnsurePhysicsSetup();
            ApplyElementVisual();
        }

        private void Update()
        {
            if (hasHit)
            {
                return;
            }

            previousPosition = transform.position;
            Vector3 nextPosition = transform.position + direction * speed * Time.deltaTime;

            if (TryHitBetween(previousPosition, nextPosition))
            {
                return;
            }

            transform.position = nextPosition;

            if (TryHitAt(transform.position))
            {
                return;
            }

            if (Time.time >= destroyAt)
            {
                Destroy(gameObject);
            }
        }

        private bool TryHitBetween(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            float distance = delta.magnitude;
            if (distance <= 0.001f)
            {
                return TryHitAt(to);
            }

            RaycastHit[] hits = Physics.SphereCastAll(
                from,
                hitRadius,
                delta.normalized,
                distance,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (TryHitDamageTarget(collider, hits[i].point))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryHitAt(Vector3 position)
        {
            Collider[] hits = Physics.OverlapSphere(position, hitRadius, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i];
                if (collider == null || collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (TryHitDamageTarget(collider, position))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryHitDamageTarget(Collider collider, Vector3 impactPoint)
        {
            if (collider == null || hasHit)
            {
                return false;
            }

            ElementbornPrototypeDummyEnemy dummy = collider.GetComponentInParent<ElementbornPrototypeDummyEnemy>();
            if (dummy != null)
            {
                HitDummy(dummy, impactPoint);
                return true;
            }

            ElementbornPrototypeHostileEnemy hostile = collider.GetComponentInParent<ElementbornPrototypeHostileEnemy>();
            if (hostile != null)
            {
                HitHostile(hostile, impactPoint);
                return true;
            }

            return false;
        }

        private void HitDummy(ElementbornPrototypeDummyEnemy dummy, Vector3 impactPoint)
        {
            if (dummy == null || hasHit)
            {
                return;
            }

            hasHit = true;
            dummy.TakeDamage(damage, element);
            dummy.ApplyElementalEffect(element, direction);
            CreateImpact(impactPoint);
            Destroy(gameObject);
        }

        private void HitHostile(ElementbornPrototypeHostileEnemy hostile, Vector3 impactPoint)
        {
            if (hostile == null || hasHit)
            {
                return;
            }

            hasHit = true;
            hostile.TakeDamage(damage, element);
            hostile.ApplyElementalEffect(element, direction);
            CreateImpact(impactPoint);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || hasHit)
            {
                return;
            }

            TryHitDamageTarget(other, transform.position);
        }

        private void EnsurePhysicsSetup()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void CreateImpact(Vector3 point)
        {
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.name = "Prototype " + element + " Impact";
            impact.transform.position = point;
            impact.transform.localScale = Vector3.one * 0.85f;

            Collider collider = impact.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = impact.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial(
                    "Impact " + element,
                    ElementbornPrototypeVisualUtility.GetElementColor(element));
            }

            Destroy(impact, 0.35f);
        }

        private void ApplyElementVisual()
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial(
                "Prototype Projectile " + element,
                ElementbornPrototypeVisualUtility.GetElementColor(element));
        }
    }
}
