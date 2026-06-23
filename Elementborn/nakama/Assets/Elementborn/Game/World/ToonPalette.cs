using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Flat, saturated toon materials for structure parts — a Wind-Waker-leaning palette rendered
    /// with the project's Elementborn/ToonLit cel shader. Materials are cached and shared, so a whole
    /// town reuses a handful of materials.
    /// </summary>
    public static class ToonPalette
    {
        private static readonly Dictionary<PartRole, Material> _byRole = new Dictionary<PartRole, Material>();
        private static readonly Dictionary<int, Material> _byTint = new Dictionary<int, Material>();
        private static Shader _shader;

        private static Shader ToonShader =>
            _shader != null ? _shader
            : (_shader = Shader.Find("Elementborn/ToonLit")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard"));

        public static Material For(PartRole role)
        {
            if (_byRole.TryGetValue(role, out var m) && m != null) return m;
            m = Make(ColorFor(role));
            _byRole[role] = m;
            return m;
        }

        public static Material Tinted(Color color)
        {
            int key = ColorKey(color);
            if (_byTint.TryGetValue(key, out var m) && m != null) return m;
            m = Make(color);
            _byTint[key] = m;
            return m;
        }

        public static Color ColorFor(PartRole role) => role switch
        {
            PartRole.Wall    => new Color(0.93f, 0.86f, 0.70f), // warm plaster
            PartRole.Floor   => new Color(0.74f, 0.72f, 0.66f),
            PartRole.Roof    => new Color(0.80f, 0.24f, 0.18f), // WW red
            PartRole.Wood    => new Color(0.55f, 0.37f, 0.22f),
            PartRole.Stone   => new Color(0.72f, 0.72f, 0.70f),
            PartRole.Accent  => new Color(0.92f, 0.78f, 0.30f),
            PartRole.Foliage => new Color(0.35f, 0.62f, 0.30f),
            PartRole.Cloth   => new Color(0.90f, 0.88f, 0.80f),
            PartRole.Sand    => new Color(0.88f, 0.80f, 0.55f),
            _                => Color.gray
        };

        private static Material Make(Color color)
        {
            var mat = new Material(ToonShader) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        private static int ColorKey(Color c)
        {
            int r = Mathf.RoundToInt(c.r * 31f), g = Mathf.RoundToInt(c.g * 31f), b = Mathf.RoundToInt(c.b * 31f);
            return (r << 10) | (g << 5) | b;
        }
    }
}
