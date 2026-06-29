using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Unity 6 removed Arial.ttf as a valid built-in font resource.
    /// Use this helper for generated UGUI text so editor/runtime tools are version tolerant.
    /// </summary>
    public static class ElementbornBuiltinFontUtility
    {
        public static Font GetDefaultFont()
        {
            Font font = null;

            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
                font = null;
            }

            if (font != null)
            {
                return font;
            }

            try
            {
                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
                return null;
            }
        }
    }
}
