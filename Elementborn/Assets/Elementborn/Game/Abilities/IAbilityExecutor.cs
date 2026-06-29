namespace Elementborn.Game
{
    public interface IAbilityExecutor
    {
        AbilityActivationResult Activate(AbilityDefinition ability, AbilityContext context);
    }
}
