namespace Elementborn.Game
{
    public struct AdminActionResult
    {
        public bool Success;
        public string Message;

        public static AdminActionResult Ok(string message)
        {
            return new AdminActionResult { Success = true, Message = message };
        }

        public static AdminActionResult Fail(string message)
        {
            return new AdminActionResult { Success = false, Message = message };
        }
    }
}
