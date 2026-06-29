using TMPro;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Safe TextMeshPro font helper.
    /// Avoids TMP_Settings.defaultFontAsset because projects can have TMP installed without
    /// a configured default font asset. Also avoids creating TMP assets from Unity's
    /// LegacyRuntime built-in font, which logs noisy "Unable to load font face" warnings.
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

            cachedDefaultFontAsset = TryCreateFromOsFont("Segoe UI", 18);
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

            cachedDefaultFontAsset = TryCreateFromOsFont("Arial", 18);
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

            cachedDefaultFontAsset = TryCreateFromOsFont("Liberation Sans", 18);
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

        private static TMP_FontAsset TryCreateFromOsFont(string fontName, int size)
        {
            if (string.IsNullOrWhiteSpace(fontName))
            {
                return null;
            }

            try
            {
                Font osFont = Font.CreateDynamicFontFromOSFont(fontName, size);
                if (osFont == null)
                {
                    return null;
                }

                return TMP_FontAsset.CreateFontAsset(osFont);
            }
            catch
            {
                return null;
            }
        }
    }
}
