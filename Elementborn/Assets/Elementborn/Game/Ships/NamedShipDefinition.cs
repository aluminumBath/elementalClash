using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Ships/Named Ship", fileName = "NamedShip")]
    public sealed class NamedShipDefinition : ScriptableObject
    {
        [SerializeField] private string shipId = "";
        [SerializeField] private string displayName = "Named Ship";
        [SerializeField] private string region = "";
        [SerializeField] private string dockLocation = "";
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private NpcWorldEntryDefinition captain;
        [SerializeField] private NpcWorldEntryDefinition firstMate;
        [SerializeField] private ShipReputationTier startingReputation = ShipReputationTier.Raucous;
        [SerializeField] private ElementbornFactionId faction = ElementbornFactionId.SeaRaiders;
        [TextArea]
        [SerializeField] private string description = "";
        [TextArea]
        [SerializeField] private string celebrationStyle = "";
        [TextArea]
        [SerializeField] private string raidStyle = "";
        [SerializeField] private List<ShipCrewMemberRecord> crew = new List<ShipCrewMemberRecord>();
        [SerializeField] private List<ShipEventHookDefinition> eventHooks = new List<ShipEventHookDefinition>();

        public string ShipId => string.IsNullOrWhiteSpace(shipId) ? name : shipId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ShipId : displayName;
        public string Region => region;
        public string DockLocation => dockLocation;
        public Vector3 WorldPosition => worldPosition;
        public NpcWorldEntryDefinition Captain => captain;
        public NpcWorldEntryDefinition FirstMate => firstMate;
        public ShipReputationTier StartingReputation => startingReputation;
        public ElementbornFactionId Faction => faction;
        public string Description => description;
        public string CelebrationStyle => celebrationStyle;
        public string RaidStyle => raidStyle;
        public IReadOnlyList<ShipCrewMemberRecord> Crew => crew;
        public IReadOnlyList<ShipEventHookDefinition> EventHooks => eventHooks;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(shipId))
            {
                shipId = name;
            }
        }
    }
}
