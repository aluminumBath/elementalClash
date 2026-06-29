using System;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SimpleCombatHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private bool destroyOnDeath = false;
        [SerializeField] private bool invulnerable = false;

        public float MaxHealth => Mathf.Max(1f, maxHealth);
        public float CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0f;

        public event Action<float> Damaged;
        public event Action Died;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, MaxHealth);
        }

        public void ApplyDamage(float amount)
        {
            if (invulnerable || amount <= 0f || IsDead) return;
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            Damaged?.Invoke(amount);
            if (currentHealth <= 0f)
            {
                Died?.Invoke();
                if (destroyOnDeath) Destroy(gameObject);
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        }

        public void Revive()
        {
            currentHealth = MaxHealth;
        }
    }
}
