using TMPro;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Safe TextMeshPro font helper.
    ///
    /// v73 note:
    /// Do not manufacture TMP font assets from OS/dynamic fonts at runtime.
    /// In this Unity 6 project that generated "Unable to load font face" warnings for
    /// Segoe UI / Arial / Liberation Sans. If no TMP resource asset exists, leave TMP's
    /// component font unchanged instead of manufacturing one.
    /// </summary>
    public static class ElementbornTmpFontUtility
    {
        private static TMP_FontAsset cachedDefaultFontAsset;
        private static bool searched;

        public static TMP_FontAsset GetDefaultFontAsset()
        {
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

            if (searched)
            {
                return null;
            }

            searched = true;

            cachedDefaultFontAsset = TryLoadTmpFont("Fonts & Materials/LiberationSans SDF");
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

            cachedDefaultFontAsset = TryLoadTmpFont("Fonts/LiberationSans SDF");
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

            cachedDefaultFontAsset = TryLoadTmpFont("LiberationSans SDF");
            return cachedDefaultFontAsset;
        }

        public static void ApplyDefaultFont(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            TMP_FontAsset fontAsset = GetDefaultFontAsset();
            if (fontAsset != null)
            {
                text.font = fontAsset;
            }
        }

        private static TMP_FontAsset TryLoadTmpFont(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return null;
            }

            try
            {
                return Resources.Load<TMP_FontAsset>(resourcePath);
            }
            catch
            {
                return null;
            }
        }
    }
}
