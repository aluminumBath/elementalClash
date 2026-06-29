using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Gathering/Resource Node Definition", fileName = "ResourceNodeDefinition")]
    public sealed class ResourceNodeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string nodeId = "";
        [SerializeField] private string displayName = "Resource Node";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Classification")]
        [SerializeField] private ResourceNodeType nodeType = ResourceNodeType.Unknown;
        [SerializeField] private string region = "";
        [SerializeField] private bool important = false;

        [Header("Harvesting")]
        [SerializeField] private HarvestRequirement requirement = new HarvestRequirement();
        [SerializeField] private int maxHarvestsBeforeDepleted = 1;
        [SerializeField] private float respawnSeconds = 300f;
        [SerializeField] private float rareYieldBonus = 0f;
        [SerializeField] private List<HarvestYieldEntry> yields = new List<HarvestYieldEntry>();

        [Header("Map / Journal")]
        [SerializeField] private bool addMapMarker = true;
        [SerializeField] private MapMarkerType markerType = MapMarkerType.ResourceNode;
        [SerializeField] private bool addJournalOnFirstHarvest = true;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject worldPrefab;

        public string NodeId => string.IsNullOrWhiteSpace(nodeId) ? name : nodeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? NodeId : displayName;
        public string Description => description;
        public ResourceNodeType NodeType => nodeType;
        public string Region => region;
        public bool Important => important;
        public HarvestRequirement Requirement => requirement;
        public int MaxHarvestsBeforeDepleted => Mathf.Max(1, maxHarvestsBeforeDepleted);
        public float RespawnSeconds => respawnSeconds;
        public float RareYieldBonus => rareYieldBonus;
        public IReadOnlyList<HarvestYieldEntry> Yields => yields;
        public bool AddMapMarker => addMapMarker;
        public MapMarkerType MarkerType => markerType;
        public bool AddJournalOnFirstHarvest => addJournalOnFirstHarvest;
        public Sprite Icon => icon;
        public GameObject WorldPrefab => worldPrefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                nodeId = name;
            }

            maxHarvestsBeforeDepleted = Mathf.Max(1, maxHarvestsBeforeDepleted);
        }
    }
}
