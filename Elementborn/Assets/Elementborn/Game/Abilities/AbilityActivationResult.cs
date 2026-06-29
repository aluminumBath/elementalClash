namespace Elementborn.Game
{
    public readonly struct AbilityActivationResult
    {
        public readonly bool Success;
        public readonly string Message;

        public AbilityActivationResult(bool success, string message)
        {
            Success = success;
            Message = message ?? "";
        }

        public static AbilityActivationResult Ok(string message = "")
        {
            return new AbilityActivationResult(true, message);
        }

        public static AbilityActivationResult Fail(string message)
        {
            return new AbilityActivationResult(false, message);
        }
    }
}
