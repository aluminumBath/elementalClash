namespace Elementborn.Game
{
    public readonly struct CraftingResult
    {
        public readonly bool Success;
        public readonly string Message;

        public CraftingResult(bool success, string message)
        {
            Success = success;
            Message = message ?? "";
        }

        public static CraftingResult Ok(string message)
        {
            return new CraftingResult(true, message);
        }

        public static CraftingResult Fail(string message)
        {
            return new CraftingResult(false, message);
        }
    }
}
