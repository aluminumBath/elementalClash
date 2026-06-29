using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Equipment/Equipment Item", fileName = "EquipmentItem")]
    public sealed class EquipmentItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string equipmentId = "";
        [SerializeField] private string displayName = "Equipment";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Inventory Link")]
        [SerializeField] private InventoryItemDefinition sourceItem;
        [SerializeField] private string fallbackItemId = "";

        [Header("Classification")]
        [SerializeField] private EquipmentSlotType slot = EquipmentSlotType.MainHand;
        [SerializeField] private EquipmentCategory category = EquipmentCategory.Unknown;
        [SerializeField] private InventoryItemRarity rarity = InventoryItemRarity.Common;
        [SerializeField] private AbilityElementType element = AbilityElementType.Neutral;

        [Header("Requirements")]
        [SerializeField] private int requiredLevel = 1;
        [SerializeField] private string requiredAbilityId = "";
        [SerializeField] private string requiredItemId = "";
        [SerializeField] private ElementbornFactionId requiredFaction = ElementbornFactionId.Unknown;
        [SerializeField] private int requiredFactionReputation = -999;

        [Header("Stats")]
        [SerializeField] private List<GearStatModifier> statModifiers = new List<GearStatModifier>();

        [Header("Behavior")]
        [SerializeField] private bool removesFromInventoryWhenEquipped = false;
        [SerializeField] private bool returnsToInventoryWhenUnequipped = false;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject equippedPrefab;
        [SerializeField] private string attachPointName = "";

        public string EquipmentId => string.IsNullOrWhiteSpace(equipmentId) ? name : equipmentId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? EquipmentId : displayName;
        public string Description => description;
        public InventoryItemDefinition SourceItem => sourceItem;
        public string ItemId => sourceItem != null ? sourceItem.ItemId : string.IsNullOrWhiteSpace(fallbackItemId) ? EquipmentId : fallbackItemId;
        public EquipmentSlotType Slot => slot;
        public EquipmentCategory Category => category;
        public InventoryItemRarity Rarity => rarity;
        public AbilityElementType Element => element;
        public int RequiredLevel => Mathf.Max(1, requiredLevel);
        public string RequiredAbilityId => requiredAbilityId;
        public string RequiredItemId => requiredItemId;
        public ElementbornFactionId RequiredFaction => requiredFaction;
        public int RequiredFactionReputation => requiredFactionReputation;
        public IReadOnlyList<GearStatModifier> StatModifiers => statModifiers;
        public bool RemovesFromInventoryWhenEquipped => removesFromInventoryWhenEquipped;
        public bool ReturnsToInventoryWhenUnequipped => returnsToInventoryWhenUnequipped;
        public Sprite Icon => icon;
        public GameObject EquippedPrefab => equippedPrefab;
        public string AttachPointName => attachPointName;

        public bool RequirementsMet(out string reason)
        {
            reason = "";

            if (!PlayerInventoryTracker.HasItemId(ItemId, 1))
            {
                reason = $"Requires item: {ItemId}.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredAbilityId) && !PlayerAbilityTracker.HasUnlocked(requiredAbilityId))
            {
                reason = $"Requires ability: {requiredAbilityId}.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredItemId) && !PlayerInventoryTracker.HasItemId(requiredItemId, 1))
            {
                reason = $"Requires {requiredItemId}.";
                return false;
            }

            if (requiredFaction != ElementbornFactionId.Unknown
                && requiredFactionReputation > -999
                && FactionReputationTracker.GetValue(requiredFaction) < requiredFactionReputation)
            {
                reason = $"Requires {requiredFaction} reputation {requiredFactionReputation}.";
                return false;
            }

            return true;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(equipmentId))
            {
                equipmentId = name;
            }

            requiredLevel = Mathf.Max(1, requiredLevel);
        }
    }
}
