using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>A boss's telegraphed special, layered on top of the normal <see cref="EnemyController"/> attacks so
    /// bosses fight differently: every few seconds it winds up — a growing, element-tinted ring on the ground —
    /// then unleashes a shockwave that hits the player if they're inside its radius (a flat element-typed blow
    /// that respects the matchup). Read the tell and back off to dodge it. Added to the boss in
    /// <see cref="SiteInteriorController"/>; the cadence is the pure <see cref="BossAttackPattern"/>.</summary>
    public sealed class BossController : MonoBehaviour
    {
        private BossAttackPattern _pattern;
        private Element _element;
        private float _radius;
        private float _damage;
        private float _engageRange;
        private string _bossName;
        private int _silverReward;
        private int _gemReward;
        private Damageable _selfBody;

        private Transform _player;
        private Damageable _playerBody;
        private Transform _ring;

        public void Configure(Element element, string bossName, int silverReward, int gemReward,
                              float radius = 6f, float damage = 26f, float engageRange = 22f,
                              float cooldown = 7f, float telegraph = 1.3f)
        {
            _element = element;
            _bossName = bossName;
            _silverReward = silverReward;
            _gemReward = gemReward;
            _radius = radius;
            _damage = damage;
            _engageRange = engageRange;
            _pattern = new BossAttackPattern(cooldown, telegraph);

            _selfBody = GetComponent<Damageable>();
            if (_selfBody != null && _selfBody.Health != null) _selfBody.Health.Died += OnBossDied;
        }

        private void OnBossDied()
        {
            var inv = PlayerInventory.Instance;
            if (inv != null)
            {
                if (_silverReward > 0) inv.AddCurrency(Currency.Silver, _silverReward);
                if (_gemReward > 0) inv.AddCurrency(Currency.Ruby, _gemReward);
            }
            string reward = _silverReward + " Silver" + (_gemReward > 0 ? " and " + _gemReward + " Ruby" : "");
            string who = string.IsNullOrEmpty(_bossName) ? "The boss" : _bossName;
            GameHud.Instance?.Toast(who + " falls! You claim " + reward + ".");

            // The first boss felled after the tower blast pushes the investigation onward.
            if (StoryController.Instance != null && StoryController.Instance.Chapter == StoryChapter.TheTowerBlast)
                StoryController.Instance.Advance();
        }

        private void Update()
        {
            if (_pattern == null) return;
            if (_player == null)
            {
                _player = RigTeleporter.Rig;
                if (_player != null) _playerBody = _player.GetComponentInParent<Damageable>();
            }
            if (_player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > _engageRange) { _pattern.Reset(); HideRing(); return; }

            bool struck = _pattern.Tick(Time.deltaTime);

            if (_pattern.Phase == BossAttackPhase.Telegraph) ShowRing(_pattern.TelegraphProgress);
            else HideRing();

            if (struck) Unleash(dist);
        }

        private void Unleash(float dist)
        {
            HideRing();
            SpawnBurst();
            if (dist <= _radius && _playerBody != null && _playerBody.Health != null && !_playerBody.Health.IsDead)
            {
                _playerBody.Apply(new DamageInfo(_damage, _element));
                GameHud.Instance?.Toast("The shockwave slams into you!");
            }
        }

        private void ShowRing(float progress)
        {
            if (_ring == null)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var col = q.GetComponent<Collider>();
                if (col != null) Destroy(col);
                q.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                var mr = q.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(ElementColor.For(_element));
                _ring = q.transform;
            }
            _ring.position = transform.position + Vector3.up * 0.05f;
            float s = Mathf.Lerp(0.4f, _radius * 2f, progress);
            _ring.localScale = new Vector3(s, s, 1f);
        }

        private void HideRing()
        {
            if (_ring != null) { Destroy(_ring.gameObject); _ring = null; }
        }

        private void SpawnBurst()
        {
            var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var col = b.GetComponent<Collider>();
            if (col != null) Destroy(col);
            b.transform.position = transform.position + Vector3.up * 0.4f;
            b.transform.localScale = Vector3.one * (_radius * 1.4f);
            var mr = b.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(ElementColor.For(_element));
            Destroy(b, 0.18f); // a quick shockwave flash
        }

        private void OnDestroy()
        {
            HideRing();
            if (_selfBody != null && _selfBody.Health != null) _selfBody.Health.Died -= OnBossDied;
        }
    }
}
