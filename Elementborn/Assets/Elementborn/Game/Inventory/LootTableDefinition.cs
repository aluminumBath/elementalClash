using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Inventory/Loot Table", fileName = "LootTable")]
    public sealed class LootTableDefinition : ScriptableObject
    {
        [SerializeField] private List<LootEntry> entries = new List<LootEntry>();

        public IReadOnlyList<LootEntry> Entries => entries;

        public List<LootRollResult> Roll()
        {
            var results = new List<LootRollResult>();

            foreach (var entry in entries)
            {
                if (entry == null || entry.Item == null)
                {
                    continue;
                }

                if (UnityEngine.Random.value > Mathf.Clamp01(entry.Chance))
                {
                    continue;
                }

                int quantity = UnityEngine.Random.Range(Mathf.Max(1, entry.MinQuantity), Mathf.Max(entry.MinQuantity, entry.MaxQuantity) + 1);
                results.Add(new LootRollResult(entry.Item, quantity));
            }

            return results;
        }
    }

    [Serializable]
    public sealed class LootEntry
    {
        public InventoryItemDefinition Item;
        [Range(0f, 1f)]
        public float Chance = 1f;
        public int MinQuantity = 1;
        public int MaxQuantity = 1;
    }

    public readonly struct LootRollResult
    {
        public readonly InventoryItemDefinition Item;
        public readonly int Quantity;

        public LootRollResult(InventoryItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }
}
