using System;
using UnityEngine.Events;

namespace Elementborn.Game
{
    [Serializable]
    public sealed class InventoryChangedEvent : UnityEvent
    {
    }

    [Serializable]
    public sealed class InventoryItemStackEvent : UnityEvent<InventoryItemStack>
    {
    }
}
