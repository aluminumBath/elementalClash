using UnityEngine;

namespace Elementborn.Game
{
    public static class ElementbornPrototypeVisualUtility
    {
        public static Color GetElementColor(ElementbornPrototypeElementType element)
        {
            switch (element)
            {
                case ElementbornPrototypeElementType.Fire:
                    return new Color(1f, 0.32f, 0.08f);
                case ElementbornPrototypeElementType.Water:
                    return new Color(0.1f, 0.45f, 1f);
                case ElementbornPrototypeElementType.Earth:
                    return new Color(0.28f, 0.62f, 0.22f);
                case ElementbornPrototypeElementType.Air:
                    return new Color(0.85f, 0.95f, 1f);
                default:
                    return Color.white;
            }
        }

        public static string GetElementName(ElementbornPrototypeElementType element)
        {
            switch (element)
            {
                case ElementbornPrototypeElementType.Fire:
                    return "Fire";
                case ElementbornPrototypeElementType.Water:
                    return "Water";
                case ElementbornPrototypeElementType.Earth:
                    return "Earth";
                case ElementbornPrototypeElementType.Air:
                    return "Air";
                default:
                    return "Unknown";
            }
        }

        public static Material CreateRuntimeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("HDRP/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Hidden/InternalErrorShader"));
            material.name = name;
            material.color = color;
            return material;
        }
    }
}
