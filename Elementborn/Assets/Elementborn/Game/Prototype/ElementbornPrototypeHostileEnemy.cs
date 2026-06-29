using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeHostileEnemy : MonoBehaviour
    {
        public float maxHealth = 75f;
        public float currentHealth = 75f;
        public float detectionRange = 12f;
        public float attackRange = 1.9f;
        public float moveSpeed = 2.2f;
        public float attackDamage = 14f;
        public float attackCooldown = 1.2f;
        public float respawnDelay = 4f;

        private Vector3 initialPosition;
        private Renderer[] renderers;
        private Collider[] colliders;
        private TextMesh healthLabel;
        private float nextAttackAt;
        private float respawnAt;
        private float flashUntil;
        private float slowedUntil;
        private float stunnedUntil;
        private float burningUntil;
        private float nextBurnTickAt;
        private string lastStatusText = "";
        private bool cached;
        private bool defeated;
        private readonly Color baseColor = new Color(0.42f, 0.08f, 0.08f);

        private void Awake()
        {
            Cache();
            currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
            EnsureHealthLabel();
            UpdateHealthLabel();
            ApplyColor(baseColor);
        }

        private void Update()
        {
            Cache();
            TickBurning();
            UpdateFlashVisual();
            FaceHealthLabelToCamera();

            if (defeated)
            {
                if (Time.time >= respawnAt)
                {
                    ResetEnemy();
                }
                return;
            }

            ElementbornPrototypePlayerStats player = FindAnyObjectByType<ElementbornPrototypePlayerStats>();
            if (player == null || player.IsDead)
            {
                return;
            }

            if (Time.time < stunnedUntil)
            {
                return;
            }

            Vector3 toPlayer = player.transform.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            if (distance > detectionRange)
            {
                return;
            }

            float effectiveMoveSpeed = Time.time < slowedUntil ? moveSpeed * 0.45f : moveSpeed;

            if (distance > attackRange && distance > 0.05f)
            {
                Vector3 step = toPlayer.normalized * effectiveMoveSpeed * Time.deltaTime;
                transform.position += step;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer.normalized, Vector3.up), Time.deltaTime * 10f);
            }
            else if (Time.time >= nextAttackAt)
            {
                nextAttackAt = Time.time + attackCooldown;
                player.TakeDamage(attackDamage, "Training hostile");
            }
        }

        public void TakeDamage(float amount, ElementbornPrototypeElementType element)
        {
            if (defeated || currentHealth <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));
            flashUntil = Time.time + 0.18f;
            UpdateHealthLabel();

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("Hostile hit by " + ElementbornPrototypeVisualUtility.GetElementName(element) + ". HP: " + Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth));
            }

            if (currentHealth <= 0f)
            {
                Defeat();
            }
        }

        public void ApplyElementalEffect(ElementbornPrototypeElementType element, Vector3 impactDirection)
        {
            if (defeated || currentHealth <= 0f)
            {
                return;
            }

            Vector3 push = SafeHorizontal(impactDirection);

            switch (element)
            {
                case ElementbornPrototypeElementType.Fire:
                    burningUntil = Time.time + 3.0f;
                    nextBurnTickAt = Time.time + 0.7f;
                    lastStatusText = "Burning";
                    break;
                case ElementbornPrototypeElementType.Water:
                    slowedUntil = Time.time + 2.75f;
                    lastStatusText = "Slowed";
                    break;
                case ElementbornPrototypeElementType.Earth:
                    stunnedUntil = Time.time + 0.9f;
                    transform.position += push * 1.3f;
                    lastStatusText = "Stunned";
                    break;
                case ElementbornPrototypeElementType.Air:
                    transform.position += push * 2.8f;
                    lastStatusText = "Pushed";
                    break;
            }

            UpdateHealthLabel();
        }

        public void ResetEnemy()
        {
            Cache();
            defeated = false;
            transform.position = initialPosition;
            currentHealth = maxHealth;
            slowedUntil = 0f;
            stunnedUntil = 0f;
            burningUntil = 0f;
            nextBurnTickAt = 0f;
            lastStatusText = "";
            SetVisibleAndCollidable(true);
            EnsureHealthLabel();
            UpdateHealthLabel();
            ApplyColor(baseColor);
        }

        private void TickBurning()
        {
            if (defeated || currentHealth <= 0f || Time.time >= burningUntil || Time.time < nextBurnTickAt)
            {
                return;
            }

            nextBurnTickAt = Time.time + 0.7f;
            currentHealth = Mathf.Max(0f, currentHealth - 4f);
            flashUntil = Time.time + 0.12f;
            UpdateHealthLabel();

            if (currentHealth <= 0f)
            {
                Defeat();
            }
        }

        private Vector3 SafeHorizontal(Vector3 direction)
        {
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
        }

        private void Defeat()
        {
            defeated = true;
            respawnAt = Time.time + respawnDelay;
            SetVisibleAndCollidable(false);

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.NotifyHostileDefeated(this);
                manager.ShowMessage("Hostile defeated. It will respawn shortly.");
            }
        }

        private void Cache()
        {
            if (!cached)
            {
                initialPosition = transform.position;
                cached = true;
            }

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }

            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponentsInChildren<Collider>(true);
            }
        }

        private void SetVisibleAndCollidable(bool value)
        {
            Cache();

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer != null && (healthLabel == null || renderer.gameObject != healthLabel.gameObject))
                    {
                        renderer.enabled = value;
                    }
                }
            }

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        colliders[i].enabled = value;
                    }
                }
            }

            if (healthLabel != null)
            {
                healthLabel.gameObject.SetActive(true);
            }
        }

        private void EnsureHealthLabel()
        {
            if (healthLabel != null)
            {
                return;
            }

            Transform existing = transform.Find("Hostile Health Label");
            GameObject labelGo = existing != null ? existing.gameObject : new GameObject("Hostile Health Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            labelGo.transform.localScale = Vector3.one * 0.18f;

            healthLabel = labelGo.GetComponent<TextMesh>();
            if (healthLabel == null)
            {
                healthLabel = labelGo.AddComponent<TextMesh>();
            }

            healthLabel.anchor = TextAnchor.MiddleCenter;
            healthLabel.alignment = TextAlignment.Center;
            healthLabel.fontSize = 48;
            healthLabel.characterSize = 0.28f;
            healthLabel.color = Color.white;
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void UpdateHealthLabel()
        {
            EnsureHealthLabel();
            if (healthLabel != null)
            {
                string status = string.IsNullOrWhiteSpace(lastStatusText) ? "" : "\n" + lastStatusText;
                healthLabel.text = "Hostile\nHP " + Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth) + status;
            }
        }

        private void UpdateFlashVisual()
        {
            ApplyColor(Time.time < flashUntil ? Color.white : baseColor);
        }

        private void ApplyColor(Color color)
        {
            Cache();

            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && (healthLabel == null || renderer.gameObject != healthLabel.gameObject))
                {
                    renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial("Hostile Runtime", color);
                }
            }
        }

        private void FaceHealthLabelToCamera()
        {
            if (healthLabel == null || Camera.main == null)
            {
                return;
            }

            Vector3 toCamera = healthLabel.transform.position - Camera.main.transform.position;
            if (toCamera.sqrMagnitude > 0.01f)
            {
                healthLabel.transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
            }
        }
    }
}
