using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// If the boat is damaged or struck hard enough, the current pilot is launched into nearby water.
    /// Add alongside BoatController and BoatBoardingStation. Add Damageable to the boat for projectile hits.
    /// </summary>
    public sealed class BoatKnockoffOnHit : MonoBehaviour
    {
        [SerializeField] private BoatController boat;
        [SerializeField] private BoatBoardingStation boarding;
        [SerializeField] private Damageable boatDamageable;
        [SerializeField] private float minDamageToEject = 1f;
        [SerializeField] private float collisionEjectVelocity = 6f;
        [SerializeField] private float ejectImpulse = 7f;

        private void Reset()
        {
            boat = GetComponent<BoatController>();
            boarding = GetComponentInChildren<BoatBoardingStation>();
            boatDamageable = GetComponent<Damageable>();
        }

        private void Awake()
        {
            if (boat == null) boat = GetComponent<BoatController>();
            if (boarding == null) boarding = GetComponentInChildren<BoatBoardingStation>();
            if (boatDamageable == null) boatDamageable = GetComponent<Damageable>();
        }

        private void OnEnable()
        {
            if (boatDamageable != null && boatDamageable.Health != null)
                boatDamageable.Health.Damaged += OnBoatDamaged;
        }

        private void OnDisable()
        {
            if (boatDamageable != null && boatDamageable.Health != null)
                boatDamageable.Health.Damaged -= OnBoatDamaged;
        }

        private void OnBoatDamaged(DamageInfo info)
        {
            if (info.Amount < minDamageToEject) return;
            Vector3 dir = boat != null && boat.Pilot != null ? boat.Pilot.position - transform.position : transform.right;
            Eject(dir.normalized * ejectImpulse + Vector3.up * 2f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (boat == null || !boat.HasPilot) return;
            if (collision.relativeVelocity.magnitude < collisionEjectVelocity) return;
            Vector3 dir = collision.contactCount > 0 ? -collision.GetContact(0).normal : transform.right;
            Eject(dir.normalized * ejectImpulse + Vector3.up * 2f);
        }

        private void Eject(Vector3 impulse)
        {
            if (boarding != null) boarding.ForceEject(impulse);
        }
    }
}
