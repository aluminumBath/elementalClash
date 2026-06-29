using TMPro;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Safe TextMeshPro font helper.
    /// Some Unity 6 projects can have TextMeshPro installed but no TMP_Settings default font asset.
    /// Accessing the TMP default font setting can throw a NullReferenceException, so generated
    /// Elementborn UI should use this helper instead.
    /// </summary>
    public static class ElementbornTmpFontUtility
    {
        private static TMP_FontAsset cachedDefaultFontAsset;

        public static TMP_FontAsset GetDefaultFontAsset()
        {
            if (cachedDefaultFontAsset != null)
            {
                return cachedDefaultFontAsset;
            }

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

            Font font = ElementbornBuiltinFontUtility.GetDefaultFont();
            if (font != null)
            {
                try
                {
                    cachedDefaultFontAsset = TMP_FontAsset.CreateFontAsset(font);
                }
                catch
                {
                    cachedDefaultFontAsset = null;
                }
            }

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
