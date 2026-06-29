namespace Elementborn.Game
{
    public readonly struct ShopTransactionResult
    {
        public readonly bool Success;
        public readonly ShopTransactionType Type;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly int TotalPrice;
        public readonly string Message;

        public ShopTransactionResult(bool success, ShopTransactionType type, string itemId, int quantity, int totalPrice, string message)
        {
            Success = success;
            Type = type;
            ItemId = itemId ?? "";
            Quantity = quantity;
            TotalPrice = totalPrice;
            Message = message ?? "";
        }

        public static ShopTransactionResult Ok(ShopTransactionType type, string itemId, int quantity, int totalPrice, string message)
        {
            return new ShopTransactionResult(true, type, itemId, quantity, totalPrice, message);
        }

        public static ShopTransactionResult Fail(ShopTransactionType type, string itemId, int quantity, string message)
        {
            return new ShopTransactionResult(false, type, itemId, quantity, 0, message);
        }
    }
}
