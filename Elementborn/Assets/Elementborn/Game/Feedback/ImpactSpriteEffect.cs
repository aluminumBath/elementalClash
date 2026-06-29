using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ImpactSpriteEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float lifetimeSeconds = 0.35f;
        [SerializeField] private float growAmount = 0.45f;
        [SerializeField] private bool faceCamera = true;

        private float startedAt;
        private Vector3 startScale;
        private Color startColor;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            startedAt = Time.unscaledTime;
            startScale = transform.localScale;
            startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        }

        private void LateUpdate()
        {
            float t = Mathf.Clamp01((Time.unscaledTime - startedAt) / Mathf.Max(0.01f, lifetimeSeconds));

            if (faceCamera && Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
            }

            transform.localScale = startScale * (1f + growAmount * t);

            if (spriteRenderer != null)
            {
                Color c = startColor;
                c.a *= 1f - t;
                spriteRenderer.color = c;
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        public void Configure(Sprite sprite, Color tint, float scale, float lifetime, bool shouldFaceCamera)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = tint;
                startColor = tint;
            }

            transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
            startScale = transform.localScale;
            lifetimeSeconds = Mathf.Max(0.01f, lifetime);
            faceCamera = shouldFaceCamera;
            startedAt = Time.unscaledTime;
        }
    }
}
