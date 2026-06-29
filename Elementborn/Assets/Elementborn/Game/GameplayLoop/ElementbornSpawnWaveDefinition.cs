using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Gameplay Loop/Spawn Wave", fileName = "ElementbornSpawnWave")]
    public sealed class ElementbornSpawnWaveDefinition : ScriptableObject
    {
        [SerializeField] private string waveId = "";
        [SerializeField] private string displayName = "Spawn Wave";
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private List<ElementbornSpawnWaveEntry> entries = new List<ElementbornSpawnWaveEntry>();

        public string WaveId => string.IsNullOrWhiteSpace(waveId) ? name : waveId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? WaveId : displayName;
        public string Summary => summary;
        public IReadOnlyList<ElementbornSpawnWaveEntry> Entries => entries;
    }
}
