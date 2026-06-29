using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AbilityActivator : MonoBehaviour
    {
        [SerializeField] private Transform aimTransform;
        [SerializeField] private MonoBehaviour executorBehaviour;

        private IAbilityExecutor executor;

        private void Awake()
        {
            executor = executorBehaviour as IAbilityExecutor;
            if (executor == null)
            {
                executor = GetComponent<IAbilityExecutor>();
            }

            if (executor == null)
            {
                executor = gameObject.AddComponent<DefaultAbilityExecutor>();
            }
        }

        public AbilityActivationResult Activate(AbilityDefinition ability)
        {
            if (ability == null)
            {
                return AbilityActivationResult.Fail("No ability selected.");
            }

            if (!PlayerAbilityTracker.HasUnlocked(ability.AbilityId))
            {
                return AbilityActivationResult.Fail($"Ability not unlocked: {ability.DisplayName}");
            }

            var context = AbilityContext.From(gameObject, aimTransform);
            return executor.Activate(ability, context);
        }

        public AbilityActivationResult ActivateByDefinition(AbilityDefinition ability)
        {
            return Activate(ability);
        }
    }
}
