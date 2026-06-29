using System.Collections;
using UnityEngine;

namespace Elementborn.Game
{
    public enum EnemyRespawnRarity { Typical, Rare, Never }

    /// <summary>
    /// Overrides instant destroy-on-death. Enemies turn stone, crumble to dust, then respawn at their original
    /// spot after 60s for typical enemies or 10m for rare enemies.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public sealed class EnemyRespawnOnDeath : MonoBehaviour
    {
        [SerializeField] private EnemyRespawnRarity rarity = EnemyRespawnRarity.Typical;
        [SerializeField] private float typicalRespawnSeconds = 60f;
        [SerializeField] private float rareRespawnSeconds = 600f;
        [SerializeField] private float stoneSeconds = 0.5f;
        [SerializeField] private float crumbleSeconds = 1.0f;

        private Damageable _damageable;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _spawnScale;
        private Renderer[] _renderers;
        private Collider[] _colliders;
        private Behaviour[] _behaviours;
        private bool _dying;

        public void SetRare(bool rare) => rarity = rare ? EnemyRespawnRarity.Rare : EnemyRespawnRarity.Typical;

        private void Awake()
        {
            _damageable = GetComponent<Damageable>();
            _damageable.DestroyOnDeath = false;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _spawnScale = transform.localScale;
            CaptureParts();
        }

        private void Start()
        {
            if (_damageable != null && _damageable.Health != null)
                _damageable.Health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_damageable != null && _damageable.Health != null)
                _damageable.Health.Died -= OnDied;
        }

        private void CaptureParts()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _colliders = GetComponentsInChildren<Collider>(true);
            _behaviours = GetComponentsInChildren<Behaviour>(true);
        }

        private void OnDied()
        {
            if (_dying) return;
            _dying = true;
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            SetCombatEnabled(false);
            SetStoneTint(1f);
            yield return new WaitForSeconds(stoneSeconds);

            Vector3 startScale = transform.localScale;
            float t = 0f;
            while (t < crumbleSeconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, crumbleSeconds));
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
                if (Random.value < 0.25f) SpawnDust();
                yield return null;
            }

            SetVisible(false);
            float delay = rarity == EnemyRespawnRarity.Rare ? rareRespawnSeconds : typicalRespawnSeconds;
            if (rarity == EnemyRespawnRarity.Never) yield break;
            yield return new WaitForSeconds(delay);

            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            transform.localScale = _spawnScale;
            SetVisible(true);
            SetStoneTint(0f);
            _damageable.Health.Revive();
            SetCombatEnabled(true);
            _dying = false;
        }

        private void SetCombatEnabled(bool enabled)
        {
            foreach (var c in _colliders) if (c != null) c.enabled = enabled;
            foreach (var b in _behaviours)
            {
                if (b == null || b == this || b == _damageable) continue;
                if (b is Renderer) continue;
                b.enabled = enabled;
            }
        }

        private void SetVisible(bool visible)
        {
            foreach (var r in _renderers) if (r != null) r.enabled = visible;
        }

        private void SetStoneTint(float amount)
        {
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                foreach (var mat in r.materials)
                {
                    if (mat == null) continue;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.Lerp(mat.color, Color.gray, amount));
                    else if (mat.HasProperty("_Color")) mat.color = Color.Lerp(mat.color, Color.gray, amount);
                }
            }
        }

        private void SpawnDust()
        {
            GameObject dust = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dust.name = "EnemyDust";
            dust.transform.position = transform.position + Random.insideUnitSphere * 0.6f + Vector3.up * 0.5f;
            dust.transform.localScale = Vector3.one * Random.Range(0.04f, 0.1f);
            var col = dust.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var rb = dust.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.AddForce((Vector3.up + WorldWindController.ActiveDirection * 1.5f + Random.insideUnitSphere).normalized * Random.Range(0.8f, 1.8f), ForceMode.VelocityChange);
            Destroy(dust, Random.Range(0.6f, 1.2f));
        }
    }
}
