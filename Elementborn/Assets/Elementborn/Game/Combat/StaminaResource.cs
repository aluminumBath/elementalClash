using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    public sealed class StaminaResource : MonoBehaviour
    {
        [SerializeField] private CombatStaminaTuning tuning;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private bool regenerate = true;
        [SerializeField] private UnityEvent onExhausted;
        [SerializeField] private UnityEvent onRecovered;

        private float lastSpendTime = -999f;
        private bool exhausted;

        public float MaxStamina => tuning != null ? tuning.MaxStamina : Mathf.Max(1f, maxStamina);
        public float CurrentStamina => Mathf.Clamp(currentStamina, 0f, MaxStamina);
        public float Normalized => MaxStamina <= 0f ? 0f : CurrentStamina / MaxStamina;
        public bool IsExhausted => exhausted;
        public CombatStaminaTuning Tuning => tuning;

        private void Awake()
        {
            if (currentStamina <= 0f)
            {
                currentStamina = MaxStamina;
            }

            currentStamina = Mathf.Clamp(currentStamina, 0f, MaxStamina);
        }

        private void Update()
        {
            if (!regenerate)
            {
                return;
            }

            float delay = tuning != null ? tuning.RegenDelayAfterSpend : 0.75f;
            if (Time.unscaledTime < lastSpendTime + delay)
            {
                return;
            }

            float regen = tuning != null ? tuning.RegenPerSecond : 18f;
            Restore(regen * Time.deltaTime, quiet: true);
        }

        public bool CanSpend(float amount)
        {
            return !exhausted && CurrentStamina >= Mathf.Max(0f, amount);
        }

        public bool TrySpend(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (!CanSpend(amount))
            {
                return false;
            }

            currentStamina = Mathf.Max(0f, CurrentStamina - amount);
            lastSpendTime = Time.unscaledTime;
            RefreshExhaustion();
            return true;
        }

        public void Restore(float amount, bool quiet = false)
        {
            if (amount <= 0f)
            {
                return;
            }

            currentStamina = Mathf.Min(MaxStamina, CurrentStamina + amount);
            RefreshExhaustion();

            if (!quiet)
            {
                NotificationFeed.Post($"Stamina restored: {Mathf.RoundToInt(currentStamina)}/{Mathf.RoundToInt(MaxStamina)}", NotificationType.Info);
            }
        }

        public void Fill()
        {
            currentStamina = MaxStamina;
            RefreshExhaustion();
        }

        public void SetTuning(CombatStaminaTuning value)
        {
            tuning = value;
            currentStamina = Mathf.Clamp(currentStamina, 0f, MaxStamina);
        }

        public void Import(float current, float max)
        {
            maxStamina = Mathf.Max(1f, max);
            currentStamina = Mathf.Clamp(current, 0f, MaxStamina);
            RefreshExhaustion();
        }

        private void RefreshExhaustion()
        {
            float exhaustedThreshold = tuning != null ? tuning.ExhaustedThreshold : 1f;
            float recoveryThreshold = tuning != null ? tuning.ExhaustedRecoveryThreshold : 20f;

            if (!exhausted && CurrentStamina <= exhaustedThreshold)
            {
                exhausted = true;
                onExhausted?.Invoke();
                NotificationFeed.Post("Exhausted.", NotificationType.Warning);
            }
            else if (exhausted && CurrentStamina >= recoveryThreshold)
            {
                exhausted = false;
                onRecovered?.Invoke();
                NotificationFeed.Post("Recovered stamina.", NotificationType.Info);
            }
        }
    }
}
