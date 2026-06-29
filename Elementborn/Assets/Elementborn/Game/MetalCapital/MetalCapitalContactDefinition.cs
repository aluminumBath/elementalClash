using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Metal Capital/Contact", fileName = "MetalCapitalContact")]
    public sealed class MetalCapitalContactDefinition : ScriptableObject
    {
        [SerializeField] private string contactId = "";
        [SerializeField] private string displayName = "Contact";
        [SerializeField] private MetalCapitalContactType contactType = MetalCapitalContactType.BlackMarketVendor;
        [SerializeField] private MetalCapitalDistrict district = MetalCapitalDistrict.BlackMarket;
        [SerializeField] private Vector3 worldPosition;
        [SerializeField] private NpcWorldEntryDefinition npc;
        [TextArea]
        [SerializeField] private string description = "";
        [TextArea]
        [SerializeField] private string secret = "";
        [SerializeField] private List<BlackMarketListingDefinition> listings = new List<BlackMarketListingDefinition>();

        public string ContactId => string.IsNullOrWhiteSpace(contactId) ? name : contactId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ContactId : displayName;
        public MetalCapitalContactType ContactType => contactType;
        public MetalCapitalDistrict District => district;
        public Vector3 WorldPosition => worldPosition;
        public NpcWorldEntryDefinition Npc => npc;
        public string Description => description;
        public string Secret => secret;
        public IReadOnlyList<BlackMarketListingDefinition> Listings => listings;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(contactId))
            {
                contactId = name;
            }
        }
    }
}
