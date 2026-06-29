using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeDummyEnemy : MonoBehaviour
    {
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public bool respawnWhenDefeated = true;
        public float respawnDelay = 2.5f;

        private Vector3 initialPosition;
        private Renderer[] renderers;
        private Collider[] colliders;
        private TextMesh healthLabel;
        private float respawnAt;
        private float flashUntil;
        private bool cachedInitialPosition;
        private bool flashVisualActive;
        private readonly Color baseColor = new Color(0.18f, 0.12f, 0.1f);

        private void Awake()
        {
            CacheSceneReferencesIfNeeded();
            currentHealth = Mathf.Clamp(currentHealth <= 0f ? maxHealth : currentHealth, 0f, maxHealth);
            EnsureHealthLabel();
            UpdateHealthLabel();
            ApplyBodyColor(baseColor);
        }

        private void OnEnable()
        {
            CacheSceneReferencesIfNeeded();
            EnsureHealthLabel();
            UpdateHealthLabel();
        }

        private void Update()
        {
            CacheSceneReferencesIfNeeded();

            if (currentHealth <= 0f && respawnWhenDefeated && Time.time >= respawnAt)
            {
                Respawn();
            }

            UpdateFlashVisual();
            FaceHealthLabelToCamera();
        }

        /// <summary>
        /// Safe in Play Mode and Edit Mode. Earlier v82 called this from an editor menu before Awake,
        /// which meant renderers/colliders were still null.
        /// </summary>
        public void ResetDummy()
        {
            CacheSceneReferencesIfNeeded();

            if (cachedInitialPosition)
            {
                transform.position = initialPosition;
            }

            currentHealth = maxHealth;
            flashUntil = 0f;
            flashVisualActive = false;

            EnsureHealthLabel();
            UpdateHealthLabel();
            SetVisibleAndCollidable(true);
            ApplyBodyColor(baseColor);
        }

        public void TakeDamage(float amount, ElementbornPrototypeElementType element)
        {
            CacheSceneReferencesIfNeeded();

            if (currentHealth <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));
            flashUntil = Time.time + 0.22f;
            flashVisualActive = false;

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("Training dummy hit by " + ElementbornPrototypeVisualUtility.GetElementName(element) + ". HP: " + Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth));
            }

            UpdateHealthLabel();

            if (currentHealth <= 0f)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            SetVisibleAndCollidable(false);
            respawnAt = Time.time + respawnDelay;

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("Training dummy defeated. It will reset shortly.");
            }
        }

        private void Respawn()
        {
            ResetDummy();

            ElementbornPrototypeGameManager manager = ElementbornPrototypeGameManager.Instance;
            if (manager != null)
            {
                manager.ShowMessage("Training dummy reset.");
            }
        }

        private void CacheSceneReferencesIfNeeded()
        {
            if (!cachedInitialPosition)
            {
                initialPosition = transform.position;
                cachedInitialPosition = true;
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
            CacheSceneReferencesIfNeeded();

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null || IsHealthLabelRenderer(renderer))
                    {
                        continue;
                    }

                    renderer.enabled = value;
                }
            }

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    Collider collider = colliders[i];
                    if (collider != null)
                    {
                        collider.enabled = value;
                    }
                }
            }

            if (healthLabel != null)
            {
                healthLabel.gameObject.SetActive(true);
            }
        }

        private bool IsHealthLabelRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            return healthLabel != null &&
                   renderer.gameObject == healthLabel.gameObject;
        }

        private void EnsureHealthLabel()
        {
            if (healthLabel != null)
            {
                return;
            }

            Transform existing = transform.Find("Dummy Health Label");
            GameObject labelGo = existing != null ? existing.gameObject : new GameObject("Dummy Health Label");
            labelGo.transform.SetParent(transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 2.9f, 0f);
            labelGo.transform.localRotation = Quaternion.identity;
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

            // New child label means cached renderer list may need refreshing.
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void UpdateHealthLabel()
        {
            EnsureHealthLabel();

            if (healthLabel != null)
            {
                healthLabel.text = "Training Dummy\nHP " + Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth);
            }
        }

        private void UpdateFlashVisual()
        {
            bool shouldFlash = Time.time < flashUntil;
            if (shouldFlash == flashVisualActive)
            {
                return;
            }

            flashVisualActive = shouldFlash;
            ApplyBodyColor(shouldFlash ? Color.white : baseColor);
        }

        private void ApplyBodyColor(Color color)
        {
            CacheSceneReferencesIfNeeded();

            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsHealthLabelRenderer(renderer))
                {
                    continue;
                }

                renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial("Training Dummy Runtime", color);
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
