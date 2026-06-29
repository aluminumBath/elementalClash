using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypePlayerVisual : MonoBehaviour
    {
        public Renderer sashRenderer;

        private void Awake()
        {
            Resolve();
        }

        public void ApplyElement(ElementbornPrototypeElementType element)
        {
            Resolve();

            if (sashRenderer != null)
            {
                sashRenderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial(
                    "Player Sash " + element,
                    ElementbornPrototypeVisualUtility.GetElementColor(element));
            }
        }

        private void Resolve()
        {
            if (sashRenderer != null)
            {
                return;
            }

            Transform sash = transform.Find("Elemental Sash");
            if (sash != null)
            {
                sashRenderer = sash.GetComponent<Renderer>();
            }
        }
    }
}
