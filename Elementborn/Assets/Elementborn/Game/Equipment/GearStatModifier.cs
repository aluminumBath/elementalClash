using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class GearStatModifier
    {
        public GearStatType Stat = GearStatType.AttackPower;
        public float FlatValue = 0f;
        public float PercentValue = 0f;

        public float Apply(float baseValue)
        {
            return baseValue + FlatValue + baseValue * (PercentValue / 100f);
        }

        public string ToDisplayString()
        {
            string flat = Mathf.Abs(FlatValue) > 0.001f ? $"{(FlatValue >= 0 ? "+" : "")}{FlatValue:0.#}" : "";
            string percent = Mathf.Abs(PercentValue) > 0.001f ? $"{(PercentValue >= 0 ? "+" : "")}{PercentValue:0.#}%" : "";

            if (!string.IsNullOrWhiteSpace(flat) && !string.IsNullOrWhiteSpace(percent))
            {
                return $"{Stat}: {flat}, {percent}";
            }

            if (!string.IsNullOrWhiteSpace(flat))
            {
                return $"{Stat}: {flat}";
            }

            if (!string.IsNullOrWhiteSpace(percent))
            {
                return $"{Stat}: {percent}";
            }

            return $"{Stat}: 0";
        }
    }
}
