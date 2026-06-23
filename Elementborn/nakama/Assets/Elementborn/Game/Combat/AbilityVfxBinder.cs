using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Listens to <see cref="PlayerCombatController.OutcomeReady"/> and turns resolved
    /// outcomes into visible effects. Projectiles spawn from an assigned prefab (per element)
    /// if one is set, otherwise from a code-built one via <see cref="ProceduralProjectiles"/>.
    /// This is the fire-VFX hook the scaffold left open, now connected.
    /// </summary>
    public sealed class AbilityVfxBinder : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;

        [Header("Optional designer prefabs (leave empty to use procedural visuals)")]
        [SerializeField] private GameObject fireProjectilePrefab;
        [SerializeField] private GameObject waterProjectilePrefab;
        [SerializeField] private GameObject impactEffectPrefab;

        [SerializeField] private float projectileLifetime = 4f;

        private void Reset() => combat = GetComponent<PlayerCombatController>();

        private void OnEnable()
        {
            if (combat == null) combat = GetComponent<PlayerCombatController>();
            if (combat != null) combat.OutcomeReady += HandleOutcome;
        }

        private void OnDisable()
        {
            if (combat != null) combat.OutcomeReady -= HandleOutcome;
        }

        private void HandleOutcome(AbilityOutcome outcome, Vector3 origin)
        {
            AudioController.Instance?.PlayAbility(outcome, origin);

            // Projectiles are spawned here; barrier and movement effects are owned by
            // BarrierResponder and DashResponder respectively (gameplay plus their own visuals).
            if (outcome.Kind == OutcomeKind.Projectile)
                SpawnProjectile(outcome, origin);
        }

        private void SpawnProjectile(AbilityOutcome outcome, Vector3 origin)
        {
            GameObject prefab = outcome.Element == Element.Water ? waterProjectilePrefab : fireProjectilePrefab;
            Quaternion rot = Quaternion.LookRotation(SafeDir(outcome.Direction));

            GameObject go = prefab != null
                ? Instantiate(prefab, origin, rot)
                : ProceduralProjectiles.BuildProjectile(outcome);

            if (prefab == null)
            {
                go.transform.SetPositionAndRotation(origin, rot);
            }

            var projectile = go.GetComponent<Projectile>();
            if (projectile == null) projectile = go.AddComponent<Projectile>();
            projectile.Initialize(outcome, projectileLifetime, impactEffectPrefab, gameObject);
        }

        private static Vector3 SafeDir(Vector3 d) =>
            d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
    }
}
