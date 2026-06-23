using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// A simple day/night cycle: rotates the sun, dims it at night, lerps ambient light between day and
    /// night colours, and feeds the sun direction to the ToonSky skybox so the sun disc tracks across the
    /// sky. Pairs with SceneStyleController (which sets the scene up); this animates it over time. Leave
    /// the sun unassigned to auto-find the directional light.
    /// </summary>
    public sealed class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private float dayLengthSeconds = 180f;
        [SerializeField, Range(0f, 1f)] private float timeOfDay = 0.3f; // 0 = midnight, 0.5 = noon
        [SerializeField] private float sunYaw = 170f;
        [SerializeField] private float maxSunIntensity = 1.15f;
        [SerializeField] private Color dayAmbient = new Color(0.55f, 0.58f, 0.62f);
        [SerializeField] private Color nightAmbient = new Color(0.08f, 0.10f, 0.16f);

        private Material _sky;
        private static readonly int SunDirectionId = Shader.PropertyToID("_SunDirection");

        private void Start()
        {
            if (sun == null)
            {
                if (RenderSettings.sun != null) sun = RenderSettings.sun;
                else
                    foreach (var l in FindObjectsOfType<Light>())
                        if (l.type == LightType.Directional) { sun = l; break; }
            }
            _sky = RenderSettings.skybox;
        }

        private void Update()
        {
            if (dayLengthSeconds > 0f)
                timeOfDay = Mathf.Repeat(timeOfDay + Time.deltaTime / dayLengthSeconds, 1f);

            // Sun sweeps from below the horizon (midnight) up and over (noon).
            float sunPitch = timeOfDay * 360f - 90f;
            Quaternion rot = Quaternion.Euler(sunPitch, sunYaw, 0f);

            float dayFactor;
            if (sun != null)
            {
                sun.transform.rotation = rot;
                dayFactor = Mathf.Clamp01(Vector3.Dot(-sun.transform.forward, Vector3.up));
                sun.intensity = Mathf.Lerp(0.04f, maxSunIntensity, dayFactor);
                sun.enabled = sun.intensity > 0.02f;
            }
            else
            {
                dayFactor = Mathf.Clamp01(Mathf.Sin(timeOfDay * Mathf.PI * 2f - Mathf.PI * 0.5f) * 0.5f + 0.5f);
            }

            RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, dayFactor);

            if (_sky != null && _sky.HasProperty(SunDirectionId))
                _sky.SetVector(SunDirectionId, -(rot * Vector3.forward));
        }
    }
}
