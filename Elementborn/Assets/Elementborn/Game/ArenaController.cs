using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The dedicated VR combat mode: a sparring arena that throws escalating waves at the player. It reuses
    /// the existing enemy stack — <see cref="EnemyComposition"/> rolls each fighter, <see cref="EnemyController"/>
    /// runs the AI, and enemies here get a dodgeable telegraph (windup) so footwork matters. Kills and clean
    /// dodges feed <see cref="ScoreController"/>'s combo, casts cost <see cref="StaminaController"/> stamina,
    /// and the run ends on a player death or after <see cref="ArenaProgression.TotalWaves"/> cleared waves.
    ///
    /// Setup: drop on an empty GameObject, assign an enemy prefab (the same one a WorldSpawnPlacer uses:
    /// EnemyController + Damageable + FactionMember + CharacterController), and place it where you want the
    /// ring centered. The player is found by the "Player" tag. It builds its own HUD and stamina pool.
    /// </summary>
    public sealed class ArenaController : MonoBehaviour
    {
        public enum Phase { Ready, Active, Cleared, Defeated }

        [Header("Spawning")]
        [Tooltip("Enemy prefab: EnemyController + Damageable + FactionMember + CharacterController.")]
        [SerializeField] private GameObject enemyPrefab;
        [Tooltip("Ring centre. Defaults to this object's position.")]
        [SerializeField] private Transform arenaCenter;
        [SerializeField] private float spawnRadius = 12f;
        [Tooltip("Biome flavour for which archetypes/elements appear.")]
        [SerializeField] private BiomeType biome = BiomeType.Mountains;
        [Tooltip("Windup before enemy hits land, so they can be dodged.")]
        [SerializeField] private float telegraphTime = 0.7f;
        [SerializeField] private int seed = 1337;
        [SerializeField] private bool autoStart = true;

        public Phase Current { get; private set; } = Phase.Ready;
        public int Wave { get; private set; }
        public int LivingCount { get; private set; }

        private readonly List<Damageable> _alive = new List<Damageable>();
        private Damageable _player;
        private bool _awaitingClear;

        // HUD
        private UiLabel _waveText;
        private UiLabel _banner;
        private RectTransform _staminaFill;

        private void Start()
        {
            if (arenaCenter == null) arenaCenter = transform;
            ScoreController.EnsureInstance();
            StaminaController.EnsureInstance();
            AcquirePlayer();
            BuildHud();
            SetBanner(autoStart ? "" : "Press Enter to begin");
            UpdateHud();
            if (autoStart) StartRun();
        }

        private void AcquirePlayer()
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null) _player = tagged.GetComponentInParent<Damageable>();
            if (_player == null) _player = FindObjectOfType<PlayerCombatController>()?.GetComponent<Damageable>();
        }

        public void StartRun()
        {
            ClearEnemies();
            ScoreController.Instance?.Score.Reset();
            StaminaController.Instance?.Refill();
            Wave = 0;
            Current = Phase.Active;
            SetBanner("");
            NextWave();
        }

        private void Update()
        {
            // Allow a quick restart from the keyboard once a run has ended (VR can rebind to a button later).
            if ((Current == Phase.Defeated || Current == Phase.Cleared) &&
                Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                StartRun();
                return;
            }

            if (Current != Phase.Active) return;

            // Lose check: the player's health can be rebuilt by other systems, so read it fresh.
            if (_player != null && _player.Health != null && _player.Health.IsDead)
            {
                Defeat();
                return;
            }

            if (!_awaitingClear) return;

            int n = 0;
            for (int i = 0; i < _alive.Count; i++)
                if (_alive[i] != null && _alive[i].Health != null && !_alive[i].Health.IsDead) n++;

            if (n != LivingCount) { LivingCount = n; UpdateHud(); }

            if (n == 0)
            {
                _awaitingClear = false;
                NextWave();
            }
        }

        private void NextWave()
        {
            Wave++;
            if (Wave > ArenaProgression.TotalWaves)
            {
                Victory();
                return;
            }
            SpawnWave(ArenaProgression.For(Wave));
            UpdateHud();
        }

        private void SpawnWave(WavePlan plan)
        {
            _alive.Clear();
            if (enemyPrefab == null)
            {
                Debug.LogError("ArenaController: no enemy prefab assigned.");
                Current = Phase.Ready;
                return;
            }

            var rng = new SystemRandomSource(seed + Wave * 31);
            Vector3 center = arenaCenter.position;
            float jitter = Random.Range(0f, 360f);

            for (int i = 0; i < plan.EnemyCount; i++)
            {
                float angle = jitter + (360f / plan.EnemyCount) * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 pos = center + dir * spawnRadius;

                var go = Instantiate(enemyPrefab, pos, Quaternion.LookRotation(-dir), transform);
                var ec = go.GetComponent<EnemyController>();
                if (ec != null)
                {
                    EnemyPlan ep = EnemyComposition.Pick(biome, plan.DangerLevel, rng);
                    ec.Configure(ep.Kind, ep.Faction, ep.Element);
                    ec.SetTelegraph(telegraphTime);
                    if (_player != null) ec.SetTarget(_player.transform);
                }
                var dmg = go.GetComponent<Damageable>();
                if (dmg != null) _alive.Add(dmg);
            }

            LivingCount = _alive.Count;
            _awaitingClear = true;
            SetBanner($"Wave {Wave}");
            CancelInvoke(nameof(ClearBanner));
            Invoke(nameof(ClearBanner), 1.25f);
        }

        private void Victory()
        {
            Current = Phase.Cleared;
            SetBanner("Arena cleared!  (Enter to replay)");
            UpdateHud();
        }

        private void Defeat()
        {
            Current = Phase.Defeated;
            ClearEnemies();
            SetBanner("Defeated  (Enter to retry)");
            UpdateHud();
        }

        private void ClearEnemies()
        {
            for (int i = 0; i < _alive.Count; i++)
                if (_alive[i] != null) Destroy(_alive[i].gameObject);
            _alive.Clear();
            LivingCount = 0;
            _awaitingClear = false;
        }

        // ---- HUD (built via UiTheme, like ScoreController) ----

        private void BuildHud()
        {
            var canvas = UiTheme.Canvas("ArenaHud", sortOrder: 12);
            canvas.transform.SetParent(transform, false);

            _waveText = UiTheme.Label(canvas.transform, "", 26, Color.white, TextAnchor.UpperCenter);
            var wr = _waveText.Rect;
            wr.anchorMin = wr.anchorMax = new Vector2(0.5f, 1f);
            wr.pivot = new Vector2(0.5f, 1f);
            wr.sizeDelta = new Vector2(420, 36);
            wr.anchoredPosition = new Vector2(0, -20);

            _banner = UiTheme.Label(canvas.transform, "", 40, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
            var br = _banner.Rect;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.sizeDelta = new Vector2(900, 80);
            br.anchoredPosition = new Vector2(0, 120);

            // Stamina bar: a track with a left-anchored fill we scale each frame.
            var track = UiTheme.Panel(canvas.transform, UiTheme.TrackColor);
            var tr = track.rectTransform;
            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0f);
            tr.pivot = new Vector2(0.5f, 0f);
            tr.sizeDelta = new Vector2(360, 16);
            tr.anchoredPosition = new Vector2(0, 92);

            var fill = UiTheme.Panel(track.transform, new Color(0.35f, 0.75f, 1f, 1f));
            _staminaFill = fill.rectTransform;
            _staminaFill.anchorMin = Vector2.zero;
            _staminaFill.anchorMax = Vector2.one;
            _staminaFill.offsetMin = Vector2.zero;
            _staminaFill.offsetMax = Vector2.zero;
            _staminaFill.pivot = new Vector2(0f, 0.5f);
        }

        private void LateUpdate()
        {
            if (_staminaFill != null)
            {
                float f = StaminaController.Instance != null ? StaminaController.Instance.Fraction : 1f;
                _staminaFill.localScale = new Vector3(Mathf.Clamp01(f), 1f, 1f);
            }
        }

        private void UpdateHud()
        {
            if (_waveText == null) return;
            _waveText.text = Current == Phase.Active
                ? $"Wave {Wave} / {ArenaProgression.TotalWaves}   ·   Enemies {LivingCount}"
                : "";
        }

        private void SetBanner(string text) { if (_banner != null) _banner.text = text; }
        private void ClearBanner() { if (Current == Phase.Active) SetBanner(""); }
    }
}
