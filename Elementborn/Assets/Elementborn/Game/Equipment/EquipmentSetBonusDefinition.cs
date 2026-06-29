using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Equipment/Equipment Set Bonus", fileName = "EquipmentSetBonus")]
    public sealed class EquipmentSetBonusDefinition : ScriptableObject
    {
        [SerializeField] private string setId = "";
        [SerializeField] private string displayName = "Equipment Set";
        [TextArea]
        [SerializeField] private string description = "";
        [SerializeField] private List<string> requiredEquipmentIds = new List<string>();
        [SerializeField] private List<GearStatModifier> bonusModifiers = new List<GearStatModifier>();

        public string SetId => string.IsNullOrWhiteSpace(setId) ? name : setId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? SetId : displayName;
        public string Description => description;
        public IReadOnlyList<string> RequiredEquipmentIds => requiredEquipmentIds;
        public IReadOnlyList<GearStatModifier> BonusModifiers => bonusModifiers;

        public bool IsActive()
        {
            foreach (string id in requiredEquipmentIds)
            {
                if (!PlayerEquipmentTracker.HasEquippedEquipment(id))
                {
                    return false;
                }
            }

            return requiredEquipmentIds.Count > 0;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(setId))
            {
                setId = name;
            }
        }
    }
}
