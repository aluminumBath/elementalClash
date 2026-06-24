using UnityEngine;
using TMPro;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Spawns a one-shot world-space label that floats up and fades (a <see cref="FloatingNumber"/>). Used for
    /// damage numbers and defeat score popups. Asset-free: text uses TMP's default font (the one the UI relies on);
    /// if no TMP font is configured it returns without spawning, so nothing renders broken.
    /// </summary>
    public static class FloatingText
    {
        public static void Spawn(Vector3 worldPos, string text, Color color, float fontSize, float baseScale)
        {
            var font = TMP_Settings.defaultFontAsset;
            if (font == null) return;

            var go = new GameObject("FloatingText");
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = text;
            tmp.color = color;

            go.AddComponent<FloatingNumber>().Play(worldPos, baseScale);
        }
    }
}
