using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Lets the pilot fire while moving or stationary. Primary = arrow/standard shot. Secondary = elemental
    /// ranged shot. Uses the existing Projectile component when possible.
    /// </summary>
    public sealed class BoatRangedCombat : MonoBehaviour
    {
        [SerializeField] private BoatController boat;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private float projectileLifetime = 4f;
        [SerializeField] private float arrowDamage = 14f;
        [SerializeField] private float arrowSpeed = 28f;
        [SerializeField] private Element secondaryElement = Element.Air;
        [SerializeField] private float secondaryDamage = 12f;
        [SerializeField] private float secondarySpeed = 30f;
        [SerializeField] private float secondaryKnockback = 4f;

        private float _readyAt;

        private void Reset()
        {
            boat = GetComponent<BoatController>();
            firePoint = transform;
            aimCamera = Camera.main;
        }

        private void Update()
        {
            if (boat != null && !boat.HasPilot) return;
            if (Time.time < _readyAt) return;

            if (InputBindings.PrimaryCast.WasPressedThisFrame())
                Fire(BuildArrowOutcome());
            else if (InputBindings.SecondaryCast.WasPressedThisFrame())
                Fire(BuildElementalOutcome());
        }

        private AbilityOutcome BuildArrowOutcome()
        {
            Vector3 direction = AimDirection();
            return new AbilityOutcome(OutcomeKind.Projectile, Element.Earth, AbilityVariant.Standard,
                direction, arrowDamage, arrowSpeed, StatusEffect.None);
        }

        private AbilityOutcome BuildElementalOutcome()
        {
            Vector3 direction = AimDirection();
            return new AbilityOutcome(OutcomeKind.Projectile, secondaryElement, AbilityVariant.Standard,
                direction, secondaryDamage, secondarySpeed, StatusEffect.None, secondaryKnockback);
        }

        private void Fire(AbilityOutcome outcome)
        {
            _readyAt = Time.time + cooldown;
            Transform origin = firePoint != null ? firePoint : transform;
            GameObject go = projectilePrefab != null
                ? Instantiate(projectilePrefab, origin.position, Quaternion.LookRotation(outcome.Direction))
                : CreateFallbackProjectile(origin.position, outcome.Direction);

            var projectile = go.GetComponent<Projectile>();
            if (projectile != null)
            {
                GameObject owner = boat != null ? boat.gameObject : gameObject;
                projectile.Initialize(outcome, projectileLifetime, null, owner);
            }
        }

        private Vector3 AimDirection()
        {
            if (aimCamera == null) aimCamera = Camera.main;
            if (aimCamera != null) return aimCamera.transform.forward.normalized;
            Transform origin = firePoint != null ? firePoint : transform;
            return origin.forward.normalized;
        }

        private GameObject CreateFallbackProjectile(Vector3 position, Vector3 direction)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "BoatProjectile";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.18f;

            Collider col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            go.AddComponent<Projectile>();
            go.transform.forward = direction;
            return go;
        }
    }
}
