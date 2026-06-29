using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/World Events/Dynamic Encounter", fileName = "DynamicEncounter")]
    public sealed class DynamicEncounterDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string encounterId = "";
        [SerializeField] private string displayName = "Encounter";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Spawn")]
        [SerializeField] private DynamicEncounterSpawnMode spawnMode = DynamicEncounterSpawnMode.SpawnPrefab;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int minCount = 1;
        [SerializeField] private int maxCount = 1;
        [SerializeField] private float spawnRadius = 8f;
        [SerializeField] private bool parentToSpawner = false;

        [Header("Map")]
        [SerializeField] private bool addMapMarker = true;
        [SerializeField] private MapMarkerType markerType = MapMarkerType.DangerZone;
        [SerializeField] private float markerExpiresInSeconds = -1f;

        [Header("Rewards / Side Effects")]
        [SerializeField] private int skillPointReward;
        [SerializeField] private string rumorText = "";

        public string EncounterId => string.IsNullOrWhiteSpace(encounterId) ? name : encounterId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? EncounterId : displayName;
        public string Description => description;
        public DynamicEncounterSpawnMode SpawnMode => spawnMode;
        public GameObject Prefab => prefab;
        public int MinCount => Mathf.Max(0, minCount);
        public int MaxCount => Mathf.Max(MinCount, maxCount);
        public float SpawnRadius => Mathf.Max(0f, spawnRadius);
        public bool ParentToSpawner => parentToSpawner;
        public bool AddMapMarker => addMapMarker;
        public MapMarkerType MarkerType => markerType;
        public float MarkerExpiresInSeconds => markerExpiresInSeconds;
        public int SkillPointReward => Mathf.Max(0, skillPointReward);
        public string RumorText => rumorText;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(encounterId)) encounterId = name;
            minCount = Mathf.Max(0, minCount);
            maxCount = Mathf.Max(minCount, maxCount);
            spawnRadius = Mathf.Max(0f, spawnRadius);
            skillPointReward = Mathf.Max(0, skillPointReward);
        }
    }
}
