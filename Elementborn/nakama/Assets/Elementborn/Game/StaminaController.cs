using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Holds the run's <see cref="StaminaModel"/>, regenerates it each frame, and answers cast requests. When
    /// one of these exists in the scene (the arena spawns it), <see cref="PlayerCombatController"/> checks it
    /// before every cast, so motion combat is paced instead of a button/flail mash. Defends are free.
    /// </summary>
    public sealed class StaminaController : MonoBehaviour
    {
        public static StaminaController Instance { get; private set; }

        [SerializeField] private float max = 100f;
        [SerializeField] private float regenPerSecond = 22f;

        public StaminaModel Model { get; private set; }
        public float Fraction => Model?.Current01 ?? 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Model = new StaminaModel(max, regenPerSecond);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update() => Model.Tick(Time.deltaTime);

        /// <summary>Pay for an action; returns false (and spends nothing) if too winded.</summary>
        public bool TrySpend(IntentType intent) => Model.TrySpend(StaminaCost.For(intent));

        public void Refill() => Model.Refill();

        public static StaminaController EnsureInstance()
        {
            if (Instance == null)
                Instance = new GameObject("StaminaController").AddComponent<StaminaController>();
            return Instance;
        }
    }
}
