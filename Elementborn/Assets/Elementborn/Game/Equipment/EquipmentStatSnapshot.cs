using System.Collections.Generic;

namespace Elementborn.Game
{
    public sealed class EquipmentStatSnapshot
    {
        private readonly Dictionary<GearStatType, float> flatBonuses = new Dictionary<GearStatType, float>();
        private readonly Dictionary<GearStatType, float> percentBonuses = new Dictionary<GearStatType, float>();

        public void Add(GearStatModifier modifier)
        {
            if (modifier == null)
            {
                return;
            }

            if (!flatBonuses.ContainsKey(modifier.Stat))
            {
                flatBonuses[modifier.Stat] = 0f;
            }

            if (!percentBonuses.ContainsKey(modifier.Stat))
            {
                percentBonuses[modifier.Stat] = 0f;
            }

            flatBonuses[modifier.Stat] += modifier.FlatValue;
            percentBonuses[modifier.Stat] += modifier.PercentValue;
        }

        public float GetFlat(GearStatType stat)
        {
            return flatBonuses.TryGetValue(stat, out float value) ? value : 0f;
        }

        public float GetPercent(GearStatType stat)
        {
            return percentBonuses.TryGetValue(stat, out float value) ? value : 0f;
        }

        public float Apply(GearStatType stat, float baseValue)
        {
            return baseValue + GetFlat(stat) + baseValue * (GetPercent(stat) / 100f);
        }
    }
}
