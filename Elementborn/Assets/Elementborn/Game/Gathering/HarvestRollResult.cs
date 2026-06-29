using System.Collections.Generic;

namespace Elementborn.Game
{
    public sealed class HarvestRollResult
    {
        public readonly List<InventoryTransactionResult> AddedItems = new List<InventoryTransactionResult>();
        public bool AnyYield;
        public bool AnyRareYield;
        public string Message = "";
    }
}
