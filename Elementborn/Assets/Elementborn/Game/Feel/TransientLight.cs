using UnityEngine;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Spawns a short-lived point light that fades out — an asset-free "spark"/"flash" for impacts and cast
    /// releases. Lights render with no material, so this is URP-safe with nothing to author or assign.
    /// </summary>
    public static class TransientLight
    {
        public static void Flash(Vector3 position, Color color, float intensity, float range, float life)
        {
            var go = new GameObject("FeelFlash");
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            go.AddComponent<LightFade>().Begin(light, life);
        }
    }
}
