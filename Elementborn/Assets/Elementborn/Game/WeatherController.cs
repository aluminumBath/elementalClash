using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Drives weather. Every so often it rolls a new weather for the biome the player is standing in
    /// (nearest world region), shows code-built particles + fog for it, and exposes a slight per-element
    /// power multiplier that <see cref="PlayerCombatController"/> applies to a channeler's attacks — so
    /// rain favours water, a heat haze favours fire, a blizzard dampens fire, and so on. Singleton so
    /// combat can find it. Particle/fog visuals are placeholders until art.
    /// </summary>
    public sealed class WeatherController : MonoBehaviour
    {
        public static WeatherController Instance { get; private set; }

        [SerializeField] private WorldMapController worldSource;
        [SerializeField] private Transform player;
        [SerializeField] private Camera weatherCamera;
        [SerializeField] private float mapToWorldScale = 1f;
        [SerializeField] private float minDuration = 22f;
        [SerializeField] private float maxDuration = 48f;

        public WeatherKind Current { get; private set; } = WeatherKind.Clear;

        private readonly UnityRandomSource _rng = new UnityRandomSource();
        private ParticleSystem _fx;
        private float _timer;

        public float ElementMultiplier(Element element) => WeatherEffects.ElementMultiplier(Current, element);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (player == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null) player = tagged.transform;
            }
            if (weatherCamera == null) weatherCamera = Camera.main;
            SetWeather(WeatherKind.Clear);
            _timer = Random.Range(minDuration, maxDuration);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                SetWeather(WeatherProfiles.Pick(CurrentBiome(), _rng));
                _timer = Random.Range(minDuration, maxDuration);
            }
        }

        private BiomeType CurrentBiome()
        {
            var world = worldSource != null ? worldSource.World : null;
            if (world == null || player == null) return BiomeType.Plains;

            WorldRegion best = null;
            float bestSqr = float.MaxValue;
            Vector3 p = player.position;
            foreach (var r in world.Regions)
            {
                Vector3 rp = new Vector3(r.MapPosition.x * mapToWorldScale, p.y, r.MapPosition.y * mapToWorldScale);
                float d = (rp - p).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = r; }
            }
            return best != null ? best.Biome : BiomeType.Plains;
        }

        private void SetWeather(WeatherKind weather)
        {
            Current = weather;
            if (_fx != null) Destroy(_fx.gameObject);
            _fx = null;

            switch (weather)
            {
                case WeatherKind.Rain:
                    _fx = MakeParticles(new Color(0.6f, 0.7f, 0.9f, 0.5f), 600, 0.06f, 18f, 1.2f, 0f);
                    Fog(new Color(0.5f, 0.55f, 0.6f), 0.012f); break;
                case WeatherKind.Blizzard:
                    _fx = MakeParticles(new Color(1f, 1f, 1f, 0.8f), 500, 0.12f, 4f, 3f, 3f);
                    Fog(new Color(0.85f, 0.9f, 0.95f), 0.03f); break;
                case WeatherKind.Sandstorm:
                    _fx = MakeParticles(new Color(0.8f, 0.7f, 0.45f, 0.5f), 700, 0.15f, 1f, 2.5f, 12f);
                    Fog(new Color(0.8f, 0.7f, 0.45f), 0.035f); break;
                case WeatherKind.HeatHaze:
                    _fx = MakeParticles(new Color(1f, 0.8f, 0.5f, 0.12f), 60, 0.5f, -1f, 2f, 1f);
                    Fog(new Color(0.9f, 0.7f, 0.5f), 0.008f); break;
                case WeatherKind.Tornado:
                    _fx = MakeParticles(new Color(0.6f, 0.6f, 0.6f, 0.5f), 800, 0.2f, 2f, 2f, 18f);
                    Fog(new Color(0.55f, 0.55f, 0.58f), 0.02f); break;
                case WeatherKind.Hurricane:
                    _fx = MakeParticles(new Color(0.6f, 0.65f, 0.75f, 0.6f), 900, 0.1f, 14f, 1.2f, 16f);
                    Fog(new Color(0.5f, 0.55f, 0.6f), 0.025f); break;
                default: // Clear — light atmospheric fog
                    Fog(new Color(0.7f, 0.8f, 0.9f), 0.004f); break;
            }
        }

        private static void Fog(Color color, float density)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = color;
            RenderSettings.fogDensity = density;
        }

        private ParticleSystem MakeParticles(Color color, float rate, float size, float downSpeed, float lifetime, float horizontal)
        {
            var go = new GameObject("WeatherFX");
            if (weatherCamera != null) go.transform.SetParent(weatherCamera.transform, false);
            else if (player != null) go.transform.SetParent(player, false);
            go.transform.localPosition = new Vector3(0f, 12f, 8f);

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startColor = color;
            main.startSize = size;
            main.startSpeed = 0f;
            main.startLifetime = lifetime;
            main.maxParticles = 2000;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = rate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 1f, 40f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(horizontal);
            vel.y = new ParticleSystem.MinMaxCurve(-downSpeed);
            vel.z = new ParticleSystem.MinMaxCurve(0f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = ParticleMaterial();
            return ps;
        }

        private static Material ParticleMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            return shader != null ? new Material(shader) : null;
        }
    }
}
