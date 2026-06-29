using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Combat/Loot Drop Table", fileName = "LootDropTable")]
    public sealed class LootDropTableDefinition : ScriptableObject
    {
        [SerializeField] private string tableId = "";
        [SerializeField] private string displayName = "Loot Table";
        [TextArea] [SerializeField] private string description = "";
        [SerializeField] private List<LootDropEntry> entries = new List<LootDropEntry>();

        public string TableId => string.IsNullOrWhiteSpace(tableId) ? name : tableId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? TableId : displayName;
        public string Description => description;
        public IReadOnlyList<LootDropEntry> Entries => entries;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(tableId)) tableId = name;
        }
    }
}
