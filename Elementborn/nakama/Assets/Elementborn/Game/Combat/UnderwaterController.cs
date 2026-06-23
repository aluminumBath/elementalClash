using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The environmental side of being under water, per actor. Tracks whether you're submerged, runs your
    /// breath meter (drowning damage when it empties), and reports a movement <see cref="SpeedScale"/> for
    /// locomotion to multiply in: water users swim at full speed, everyone else is slowed, an air
    /// <see cref="BubbleVolume"/> lets you breathe but move slowly, and an <see cref="IceTrap"/> slows you and
    /// suffocates non-water users. On the player rig it also drives the underwater fog. The combat rules
    /// (Water = ice-only, Fire thaws ice) live in <see cref="PlayerCombatController"/>.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class UnderwaterController : MonoBehaviour
    {
        [Tooltip("This actor's element. Water users breathe water and swim at full speed.")]
        [SerializeField] private Element element = Element.Water;
        [Tooltip("Derive the element from the player's loadout at start (use on the player rig).")]
        [SerializeField] private bool fromPlayerLoadout = false;

        [Header("Breath")]
        [SerializeField] private float maxAir = 12f;
        [SerializeField] private float drainPerSecond = 1f;
        [SerializeField] private float refillPerSecond = 4f;
        [SerializeField] private float drowningDamagePerSecond = 8f;

        [Header("Movement scale")]
        [SerializeField] private float submergedSpeedScale = 0.55f; // non-water actors swim slower
        [SerializeField] private float bubbleSpeedScale = 0.5f;
        [SerializeField] private float iceTrapSpeedScale = 0.15f;

        [Header("Atmosphere (player rig only)")]
        [SerializeField] private bool controlsAtmosphere = false;
        [SerializeField] private Color underwaterFog = new Color(0.12f, 0.28f, 0.38f);
        [SerializeField] private float underwaterFogDensity = 0.06f;

        public OxygenModel Air { get; private set; }
        public bool IsSubmerged { get; private set; }
        public bool IsBreathing { get; private set; }
        public bool IsWaterUser => element == Element.Water;
        public float SpeedScale { get; private set; } = 1f;

        private Damageable _self;
        private float _suffocate;
        private bool _captured;
        private bool _fog0;
        private Color _fogC0;
        private float _fogD0;
        private FogMode _fogM0;

        private void Awake()
        {
            _self = GetComponent<Damageable>();
            Air = new OxygenModel(maxAir, drainPerSecond, refillPerSecond);
        }

        /// <summary>Force a hold-breath for a duration (e.g. an octopus grab); refreshes if already suffocating.</summary>
        public void Suffocate(float seconds) => _suffocate = Mathf.Max(_suffocate, seconds);

        private void Start()
        {
            if (fromPlayerLoadout)
            {
                var inv = PlayerInventory.Instance;
                if (inv != null && inv.Loadout != null && inv.Loadout.IsChanneler)
                    element = inv.Loadout.Elements[0];
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            IsSubmerged = WaterVolume.Submerged(transform.position) != null;
            bool inBubble = BubbleVolume.Contains(transform.position);
            bool inIce = IceTrap.Contains(transform.position);

            // Breathing: at the surface, in a bubble, or you're a water user — unless ice traps a non-water user.
            IsBreathing = !IsSubmerged || inBubble || IsWaterUser;
            if (inIce && !IsWaterUser) IsBreathing = false;
            if (_suffocate > 0f) { _suffocate -= dt; IsBreathing = false; } // an octopus grip drowns even water users

            Air.Tick(dt, IsBreathing);
            if (Air.IsEmpty && _self != null)
                _self.Apply(new DamageInfo(drowningDamagePerSecond * dt, Element.Water));

            SpeedScale = 1f;
            if (IsSubmerged && !IsWaterUser) SpeedScale = submergedSpeedScale;
            if (inBubble) SpeedScale = Mathf.Min(SpeedScale, bubbleSpeedScale);
            if (inIce) SpeedScale = Mathf.Min(SpeedScale, IsWaterUser ? bubbleSpeedScale : iceTrapSpeedScale);

            if (controlsAtmosphere) DriveAtmosphere();
        }

        private void DriveAtmosphere()
        {
            if (IsSubmerged)
            {
                if (!_captured)
                {
                    _fog0 = RenderSettings.fog;
                    _fogM0 = RenderSettings.fogMode;
                    _fogC0 = RenderSettings.fogColor;
                    _fogD0 = RenderSettings.fogDensity;
                    _captured = true;
                }
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = underwaterFog;
                RenderSettings.fogDensity = underwaterFogDensity;
            }
            else if (_captured)
            {
                Restore();
            }
        }

        private void Restore()
        {
            RenderSettings.fog = _fog0;
            RenderSettings.fogMode = _fogM0;
            RenderSettings.fogColor = _fogC0;
            RenderSettings.fogDensity = _fogD0;
            _captured = false;
        }

        private void OnDisable()
        {
            if (_captured) Restore();
        }
    }
}
