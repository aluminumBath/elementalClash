using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// One-stop scene look — the Wind-Waker lighting pass. Applies a procedural toon sky
    /// (Elementborn/ToonSky), a warm "sun" directional light, gradient ambient, and stylised distance
    /// fog. Drop on any object; it applies on Start. Call <see cref="Apply"/> again after edits.
    /// </summary>
    public sealed class SceneStyleController : MonoBehaviour
    {
        [Header("Sky")]
        [SerializeField] private Color skyTop = new Color(0.20f, 0.45f, 0.85f);
        [SerializeField] private Color skyHorizon = new Color(0.72f, 0.86f, 0.95f);
        [SerializeField] private Color skyGround = new Color(0.45f, 0.50f, 0.48f);
        [SerializeField] private Color cloudColor = Color.white;
        [Range(0f, 1f)] [SerializeField] private float cloudAmount = 0.45f;

        [Header("Sun")]
        [SerializeField] private Color sunColor = new Color(1f, 0.96f, 0.82f);
        [SerializeField] private float sunIntensity = 1.15f;
        [SerializeField] private float sunPitch = 50f;
        [SerializeField] private float sunYaw = 30f;
        [SerializeField] private bool sunShadows = true;

        [Header("Ambient (gradient)")]
        [SerializeField] private Color ambientSky = new Color(0.55f, 0.65f, 0.80f);
        [SerializeField] private Color ambientEquator = new Color(0.55f, 0.55f, 0.50f);
        [SerializeField] private Color ambientGround = new Color(0.32f, 0.30f, 0.26f);

        [Header("Fog")]
        [SerializeField] private bool enableFog = true;
        [SerializeField] private Color fogColor = new Color(0.74f, 0.86f, 0.95f);
        [SerializeField] private float fogDensity = 0.0015f;

        [SerializeField] private bool applyOnStart = true;

        private Material _skyMat;
        private Light _sun;

        private void Start()
        {
            if (applyOnStart) Apply();
        }

        public void Apply()
        {
            SetupSun();
            SetupSky();
            SetupAmbient();
            SetupFog();
        }

        private void SetupSun()
        {
            if (_sun == null)
                foreach (var l in FindObjectsOfType<Light>())
                    if (l.type == LightType.Directional) { _sun = l; break; }

            if (_sun == null)
            {
                var go = new GameObject("Sun (Directional)");
                go.transform.SetParent(transform, false);
                _sun = go.AddComponent<Light>();
                _sun.type = LightType.Directional;
            }

            _sun.color = sunColor;
            _sun.intensity = sunIntensity;
            _sun.shadows = sunShadows ? LightShadows.Soft : LightShadows.None;
            _sun.transform.rotation = Quaternion.Euler(sunPitch, sunYaw, 0f);
        }

        private void SetupSky()
        {
            var shader = Shader.Find("Elementborn/ToonSky");
            if (shader == null) return;
            if (_skyMat == null) _skyMat = new Material(shader);

            _skyMat.SetColor("_TopColor", skyTop);
            _skyMat.SetColor("_HorizonColor", skyHorizon);
            _skyMat.SetColor("_GroundColor", skyGround);
            _skyMat.SetColor("_SunColor", sunColor);
            _skyMat.SetColor("_CloudColor", cloudColor);
            _skyMat.SetFloat("_CloudAmount", cloudAmount);
            if (_sun != null) _skyMat.SetVector("_SunDirection", -_sun.transform.forward);

            RenderSettings.skybox = _skyMat;
            DynamicGI.UpdateEnvironment();
        }

        private void SetupAmbient()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSky;
            RenderSettings.ambientEquatorColor = ambientEquator;
            RenderSettings.ambientGroundColor = ambientGround;
        }

        private void SetupFog()
        {
            RenderSettings.fog = enableFog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
        }
    }
}
