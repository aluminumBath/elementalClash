namespace Elementborn.Game
{
    public readonly struct InventoryTransactionResult
    {
        public readonly bool Success;
        public readonly int Requested;
        public readonly int Moved;
        public readonly string Message;

        public int Remaining => Requested - Moved;

        public InventoryTransactionResult(bool success, int requested, int moved, string message)
        {
            Success = success;
            Requested = requested;
            Moved = moved;
            Message = message ?? "";
        }

        public static InventoryTransactionResult Ok(int requested, int moved, string message = "")
        {
            return new InventoryTransactionResult(true, requested, moved, message);
        }

        public static InventoryTransactionResult Fail(int requested, int moved, string message)
        {
            return new InventoryTransactionResult(false, requested, moved, message);
        }
    }
}
