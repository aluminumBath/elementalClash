using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypePlayerStats : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth = 100f;

        public float maxStamina = 100f;
        public float currentStamina = 100f;
        public float sprintDrainPerSecond = 18f;
        public float jumpCost = 15f;
        public float staminaRegenPerSecond = 26f;
        public float staminaRegenDelay = 0.65f;

        public Vector3 respawnPosition = new Vector3(0f, 1f, -8f);

        private float nextStaminaRegenAt;
        private float invulnerableUntil;
        private ElementbornPrototypePlayerController controller;

        public bool IsDead => currentHealth <= 0f;
        public bool IsInvulnerable => Time.time < invulnerableUntil;
        public float Health01 => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
        public float Stamina01 => maxStamina <= 0f ? 0f : Mathf.Clamp01(currentStamina / maxStamina);

        private void Awake()
        {
            controller = GetComponent<ElementbornPrototypePlayerController>();
            currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
            currentStamina = Mathf.Clamp(currentStamina <= 0f ? maxStamina : currentStamina, 0f, maxStamina);
        }

        private void Update()
        {
            if (IsDead)
            {
                return;
            }

            if (Time.time >= nextStaminaRegenAt && currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenPerSecond * Time.deltaTime);
            }
        }

        public bool TrySpendStamina(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (currentStamina < amount)
            {
                return false;
            }

            currentStamina -= amount;
            nextStaminaRegenAt = Time.time + staminaRegenDelay;
            return true;
        }

        public bool CanSprint()
        {
            return currentStamina > 2f && !IsDead;
        }

        public void ConsumeSprint(float deltaTime)
        {
            if (IsDead)
            {
                return;
            }

            currentStamina = Mathf.Max(0f, currentStamina - sprintDrainPerSecond * Mathf.Max(0f, deltaTime));
            nextStaminaRegenAt = Time.time + staminaRegenDelay;
        }

        public void TakeDamage(float amount, string source)
        {
            if (IsDead || IsInvulnerable)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage(source + " hit you. HP: " + Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth));
            }

            if (currentHealth <= 0f)
            {
                Die();
            }
            else
            {
                invulnerableUntil = Time.time + 0.4f;
            }
        }

        public void HealToFull()
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }

        public void Die()
        {
            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("You were defeated. Respawning...");
            }

            Invoke(nameof(Respawn), 1.25f);
        }

        public void Respawn()
        {
            HealToFull();
            invulnerableUntil = Time.time + 1.25f;

            if (controller != null)
            {
                controller.Teleport(respawnPosition);
            }
            else
            {
                transform.position = respawnPosition;
            }

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("Respawned at the central convergence.");
            }
        }
    }
}
