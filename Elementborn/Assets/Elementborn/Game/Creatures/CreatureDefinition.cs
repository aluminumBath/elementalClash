using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Creatures/Creature Definition", fileName = "CreatureDefinition")]
    public sealed class CreatureDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string creatureId = "";
        [SerializeField] private string displayName = "Creature";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Behavior")]
        [SerializeField] private CreatureTraversalType traversalType = CreatureTraversalType.Unknown;
        [SerializeField] private CreatureTemperament temperament = CreatureTemperament.Unknown;
        [SerializeField] private int tameDifficulty = 25;
        [SerializeField] private int baseBondGain = 5;

        [Header("Preferences")]
        [SerializeField] private List<string> favoriteTreatItemIds = new List<string>();

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;

        public string CreatureId => string.IsNullOrWhiteSpace(creatureId) ? name : creatureId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? CreatureId : displayName;
        public string Description => description;
        public CreatureTraversalType TraversalType => traversalType;
        public CreatureTemperament Temperament => temperament;
        public int TameDifficulty => Mathf.Clamp(tameDifficulty, 0, 100);
        public int BaseBondGain => Mathf.Max(1, baseBondGain);
        public IReadOnlyList<string> FavoriteTreatItemIds => favoriteTreatItemIds;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;

        public bool LikesTreat(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            foreach (string favorite in favoriteTreatItemIds)
            {
                if (favorite == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(creatureId))
            {
                creatureId = name;
            }

            tameDifficulty = Mathf.Clamp(tameDifficulty, 0, 100);
            baseBondGain = Mathf.Max(1, baseBondGain);
        }
    }
}
