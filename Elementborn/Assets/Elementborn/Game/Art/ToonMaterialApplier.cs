using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Quick-preview helper: applies the Elementborn/ToonLit shader to this object's renderers at
    /// runtime so you can see the cel look without authoring a material first. For production,
    /// make a real material in the editor and assign it to your meshes.
    ///
    /// NOTE: for Shader.Find to resolve in a build, add "Elementborn/ToonLit" to
    /// Project Settings > Graphics > Always Included Shaders (the editor finds it regardless).
    /// </summary>
    public sealed class ToonMaterialApplier : MonoBehaviour
    {
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField] private Color shadowTint = new Color(0.5f, 0.55f, 0.7f, 1f);
        [SerializeField, Range(1, 5)] private int rampSteps = 2;
        [SerializeField, Range(0f, 0.05f)] private float outlineWidth = 0.012f;
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private bool includeChildren = true;

        private void Start()
        {
            var shader = Shader.Find("Elementborn/ToonLit");
            if (shader == null)
            {
                Debug.LogWarning("ToonMaterialApplier: shader 'Elementborn/ToonLit' not found " +
                                 "(add it to Always Included Shaders for runtime use).");
                return;
            }

            var material = new Material(shader);
            material.SetColor("_BaseColor", baseColor);
            material.SetColor("_ShadowTint", shadowTint);
            material.SetFloat("_RampSteps", rampSteps);
            material.SetFloat("_OutlineWidth", outlineWidth);
            material.SetColor("_OutlineColor", outlineColor);

            var renderers = includeChildren ? GetComponentsInChildren<Renderer>() : GetComponents<Renderer>();
            foreach (var r in renderers) r.sharedMaterial = material;
        }
    }
}
