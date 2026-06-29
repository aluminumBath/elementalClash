using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Generic placeholder ability executor. It provides useful debug behavior until
    /// specific combat/movement/boat/creature effects are wired.
    /// </summary>
    public sealed class DefaultAbilityExecutor : MonoBehaviour, IAbilityExecutor
    {
        [SerializeField] private bool spawnDebugSphere = true;
        [SerializeField] private float debugSphereLifetime = 1.5f;

        public AbilityActivationResult Activate(AbilityDefinition ability, AbilityContext context)
        {
            if (ability == null)
            {
                return AbilityActivationResult.Fail("No ability selected.");
            }

            if (ability.Passive)
            {
                return AbilityActivationResult.Ok($"{ability.DisplayName} is passive.");
            }

            if (!AbilityCooldownTracker.IsReady(ability.AbilityId))
            {
                return AbilityActivationResult.Fail($"{ability.DisplayName} is cooling down: {AbilityCooldownTracker.Remaining(ability.AbilityId):0.0}s.");
            }

            if (ability.Effect.ResourceCost > 0f)
            {
                // Resource pools can be wired here later.
            }

            if (spawnDebugSphere)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "AbilityDebug_" + ability.AbilityId;
                marker.transform.position = context.Origin + context.Direction * Mathf.Max(1.5f, ability.Effect.Radius + 2f);
                marker.transform.localScale = Vector3.one * Mathf.Max(0.25f, ability.Effect.Radius > 0f ? ability.Effect.Radius : 0.5f);
                Destroy(marker, debugSphereLifetime);
            }

            if (!string.IsNullOrWhiteSpace(ability.VfxResourcePath))
            {
                var prefab = Resources.Load<GameObject>(ability.VfxResourcePath);
                if (prefab != null)
                {
                    Instantiate(prefab, context.Origin + context.Direction * 2f, Quaternion.identity);
                }
            }

            AbilityCooldownTracker.StartCooldown(ability.AbilityId, ability.Effect.CooldownSeconds);
            PlayerAbilityTracker.MarkUsed(ability.AbilityId);
            NotificationFeed.Post($"Used {ability.DisplayName}.", NotificationType.Combat);
            return AbilityActivationResult.Ok($"Used {ability.DisplayName}.");
        }
    }
}
