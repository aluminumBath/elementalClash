using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Version-tolerant font helper for generated UGUI text.
    /// Unity 6 removed Arial.ttf as a valid built-in font resource, but some editor
    /// generation code still needs a concrete non-null Font for Text components.
    /// </summary>
    public static class ElementbornBuiltinFontUtility
    {
        private static Font cachedDefaultFont;

        public static Font GetDefaultFont()
        {
            if (cachedDefaultFont != null)
            {
                return cachedDefaultFont;
            }

            cachedDefaultFont = TryBuiltin("LegacyRuntime.ttf");
            if (cachedDefaultFont != null)
            {
                return cachedDefaultFont;
            }

            cachedDefaultFont = TryBuiltin("Arial.ttf");
            if (cachedDefaultFont != null)
            {
                return cachedDefaultFont;
            }

            cachedDefaultFont = TryOsFont("Segoe UI", 16);
            if (cachedDefaultFont != null)
            {
                return cachedDefaultFont;
            }

            cachedDefaultFont = TryOsFont("Arial", 16);
            if (cachedDefaultFont != null)
            {
                return cachedDefaultFont;
            }

            cachedDefaultFont = TryOsFont("Liberation Sans", 16);
            return cachedDefaultFont;
        }

        public static void ApplyDefaultFont(UnityEngine.UI.Text text)
        {
            if (text == null)
            {
                return;
            }

            Font font = GetDefaultFont();
            if (font != null)
            {
                text.font = font;
            }
        }

        private static Font TryBuiltin(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return null;
            }

            try
            {
                return Resources.GetBuiltinResource<Font>(resourceName);
            }
            catch
            {
                return null;
            }
        }

        private static Font TryOsFont(string fontName, int size)
        {
            if (string.IsNullOrWhiteSpace(fontName))
            {
                return null;
            }

            try
            {
                return Font.CreateDynamicFontFromOSFont(fontName, size);
            }
            catch
            {
                return null;
            }
        }
    }
}
