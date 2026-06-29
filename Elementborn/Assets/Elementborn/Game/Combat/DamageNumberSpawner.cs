using UnityEngine;

namespace Elementborn.Game
{
    public static class DamageNumberSpawner
    {
        public static void Spawn(Vector3 position, float amount, AbilityElementType element, bool critical)
        {
            var go = new GameObject("DamageNumber");
            go.transform.position = position + Vector3.up * 1.2f;
            var popup = go.AddComponent<DamageNumberPopup>();
            string text = critical ? $"!{Mathf.RoundToInt(amount)}!" : Mathf.RoundToInt(amount).ToString();
            popup.Initialize(text, GetColor(element, critical));
        }

        private static Color GetColor(AbilityElementType element, bool critical)
        {
            if (critical) return new Color(1f, 0.92f, 0.4f);
            return element switch
            {
                AbilityElementType.Fire => new Color(1f, 0.45f, 0.25f),
                AbilityElementType.Water => new Color(0.45f, 0.75f, 1f),
                AbilityElementType.Earth => new Color(0.75f, 0.63f, 0.35f),
                AbilityElementType.Air => new Color(0.8f, 0.95f, 1f),
                AbilityElementType.Ice => new Color(0.7f, 0.92f, 1f),
                AbilityElementType.Plant => new Color(0.55f, 0.95f, 0.48f),
                AbilityElementType.Lightning => new Color(1f, 0.9f, 0.3f),
                _ => Color.white
            };
        }
    }
}
