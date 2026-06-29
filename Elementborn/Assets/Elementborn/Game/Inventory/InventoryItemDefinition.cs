using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// ScriptableObject definition for inventory items.
    /// Create via: Assets > Create > Elementborn > Inventory > Item Definition.
    /// </summary>
    [CreateAssetMenu(menuName = "Elementborn/Inventory/Item Definition", fileName = "ItemDefinition")]
    public sealed class InventoryItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId = "";
        [SerializeField] private string displayName = "Item";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Classification")]
        [SerializeField] private InventoryItemCategory category = InventoryItemCategory.Unknown;
        [SerializeField] private InventoryItemRarity rarity = InventoryItemRarity.Common;
        [SerializeField] private bool questItem;
        [SerializeField] private bool important;

        [Header("Stacking")]
        [SerializeField] private bool stackable = true;
        [SerializeField] private int maxStack = 99;

        [Header("Economy")]
        [SerializeField] private int baseValue = 1;
        [SerializeField] private bool sellable = true;

        [Header("Use")]
        [SerializeField] private bool usable;
        [SerializeField] private bool consumedOnUse = true;
        [SerializeField] private string useVerb = "Use";

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject worldPrefab;

        public string ItemId => string.IsNullOrWhiteSpace(itemId) ? name : itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ItemId : displayName;
        public string Description => description;
        public InventoryItemCategory Category => category;
        public InventoryItemRarity Rarity => rarity;
        public bool QuestItem => questItem || rarity == InventoryItemRarity.Quest || category == InventoryItemCategory.Quest;
        public bool Important => important || QuestItem || rarity >= InventoryItemRarity.Rare;
        public bool Stackable => stackable;
        public int MaxStack => Mathf.Max(1, stackable ? maxStack : 1);
        public int BaseValue => Mathf.Max(0, baseValue);
        public bool Sellable => sellable && !QuestItem;
        public bool Usable => usable;
        public bool ConsumedOnUse => consumedOnUse;
        public string UseVerb => string.IsNullOrWhiteSpace(useVerb) ? "Use" : useVerb;
        public Sprite Icon => icon;
        public GameObject WorldPrefab => worldPrefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = name;
            }

            maxStack = Mathf.Max(1, maxStack);
            if (!stackable)
            {
                maxStack = 1;
            }

            baseValue = Mathf.Max(0, baseValue);
        }
    }
}
