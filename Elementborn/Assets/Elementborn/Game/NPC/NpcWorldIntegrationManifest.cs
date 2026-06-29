using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/NPC/NPC World Integration Manifest", fileName = "NpcWorldIntegrationManifest")]
    public sealed class NpcWorldIntegrationManifest : ScriptableObject
    {
        [SerializeField] private List<NpcWorldEntryDefinition> entries = new List<NpcWorldEntryDefinition>();

        public IReadOnlyList<NpcWorldEntryDefinition> Entries => entries;

        // v54 compatibility alias used by older admin registry code.
        public IReadOnlyList<NpcWorldEntryDefinition> Npcs => entries;

        public void SetEntries(IEnumerable<NpcWorldEntryDefinition> values)
        {
            entries.Clear();
            if (values == null)
            {
                return;
            }

            foreach (var value in values)
            {
                if (value != null && !entries.Contains(value))
                {
                    entries.Add(value);
                }
            }
        }
    }
}
