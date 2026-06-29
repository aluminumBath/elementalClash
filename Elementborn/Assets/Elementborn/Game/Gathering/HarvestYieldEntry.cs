using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class HarvestYieldEntry
    {
        public InventoryItemDefinition Item;
        public string FallbackItemId = "";
        [Range(0f, 1f)]
        public float Chance = 1f;
        public int MinQuantity = 1;
        public int MaxQuantity = 1;
        public bool RareYield = false;

        public string ItemId => Item != null ? Item.ItemId : FallbackItemId;
        public string DisplayName => Item != null ? Item.DisplayName : ItemId;

        public int RollQuantity()
        {
            int min = Mathf.Max(1, MinQuantity);
            int max = Mathf.Max(min, MaxQuantity);
            return UnityEngine.Random.Range(min, max + 1);
        }

        public bool ShouldDrop(float bonusChance = 0f)
        {
            return UnityEngine.Random.value <= Mathf.Clamp01(Chance + bonusChance);
        }
    }
}
