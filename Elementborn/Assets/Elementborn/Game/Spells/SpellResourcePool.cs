using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    public sealed class SpellResourcePool : MonoBehaviour
    {
        [SerializeField] private SpellResourceType resourceType = SpellResourceType.Focus;
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float currentValue = 100f;
        [SerializeField] private float regenPerSecond = 10f;
        [SerializeField] private bool regenerate = true;
        [SerializeField] private UnityEvent onEmpty;
        [SerializeField] private UnityEvent onRefilled;

        public SpellResourceType ResourceType => resourceType;
        public float MaxValue => Mathf.Max(1f, maxValue);
        public float CurrentValue => Mathf.Clamp(currentValue, 0f, MaxValue);
        public float Normalized => MaxValue <= 0f ? 0f : CurrentValue / MaxValue;

        private void Awake()
        {
            if (currentValue <= 0f)
            {
                currentValue = MaxValue;
            }

            currentValue = Mathf.Clamp(currentValue, 0f, MaxValue);
        }

        private void Update()
        {
            if (regenerate && regenPerSecond > 0f)
            {
                Restore(regenPerSecond * Time.deltaTime, quiet: true);
            }
        }

        public bool CanSpend(float amount)
        {
            return CurrentValue >= Mathf.Max(0f, amount);
        }

        public bool TrySpend(float amount)
        {
            amount = Mathf.Max(0f, amount);
            if (!CanSpend(amount))
            {
                return false;
            }

            currentValue = Mathf.Max(0f, CurrentValue - amount);
            if (currentValue <= 0f)
            {
                onEmpty?.Invoke();
            }
            return true;
        }

        public void Restore(float amount, bool quiet = false)
        {
            if (amount <= 0f)
            {
                return;
            }

            bool wasNotFull = CurrentValue < MaxValue;
            currentValue = Mathf.Min(MaxValue, CurrentValue + amount);
            if (wasNotFull && CurrentValue >= MaxValue)
            {
                onRefilled?.Invoke();
            }

            if (!quiet)
            {
                NotificationFeed.Post($"{resourceType} restored: {Mathf.RoundToInt(currentValue)}/{Mathf.RoundToInt(MaxValue)}", NotificationType.Info);
            }
        }

        public void Fill()
        {
            currentValue = MaxValue;
            onRefilled?.Invoke();
        }

        public void Import(float current, float max)
        {
            maxValue = Mathf.Max(1f, max);
            currentValue = Mathf.Clamp(current, 0f, MaxValue);
        }
    }
}
