using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Rare at-sea encounter spawner. It only rolls while the player is piloting the boat.
    /// Keep the chance low so attacks feel surprising instead of constant.
    /// </summary>
    public sealed class BoatCreatureEncounterDirector : MonoBehaviour
    {
        [SerializeField] private BoatController boat;
        [SerializeField] private GameObject creaturePrefab;
        [SerializeField] private float chancePerMinute = 0.08f;
        [SerializeField] private float minSecondsBetweenEncounters = 75f;
        [SerializeField] private float spawnRadius = 28f;
        [SerializeField] private float waterY = 0f;
        [SerializeField] private bool spawnedCreatureIsRare = true;

        private float _nextAllowedAt;

        private void Reset()
        {
            boat = GetComponent<BoatController>();
        }

        private void Update()
        {
            if (boat == null || !boat.HasPilot || creaturePrefab == null) return;
            if (Time.time < _nextAllowedAt) return;

            float chanceThisFrame = chancePerMinute / 60f * Time.deltaTime;
            if (Random.value > chanceThisFrame) return;

            Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 position = boat.transform.position + new Vector3(circle.x, 0f, circle.y);
            position.y = waterY;
            var spawned = Instantiate(creaturePrefab, position, Quaternion.LookRotation((boat.transform.position - position).normalized));
            var respawn = spawned.GetComponent<EnemyRespawnOnDeath>();
            if (respawn == null) respawn = spawned.AddComponent<EnemyRespawnOnDeath>();
            respawn.SetRare(spawnedCreatureIsRare);
            _nextAllowedAt = Time.time + minSecondsBetweenEncounters;
            GameHud.Instance?.Toast("Something stirs beneath the waves...");
        }
    }
}
