using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CombatResistanceProfile : MonoBehaviour
    {
        [SerializeField] private string profileName = "";
        [SerializeField] private float flatDefense = 0f;
        [SerializeField] private List<CombatResistanceEntry> entries = new List<CombatResistanceEntry>();

        public string ProfileName => string.IsNullOrWhiteSpace(profileName) ? gameObject.name : profileName;
        public float FlatDefense => flatDefense;
        public IReadOnlyList<CombatResistanceEntry> Entries => entries;

        public float GetPercent(AbilityElementType element)
        {
            float total = 0f;
            foreach (var e in entries)
            {
                if (e != null && e.Element == element) total += e.Percent;
            }
            return total;
        }
    }
}
