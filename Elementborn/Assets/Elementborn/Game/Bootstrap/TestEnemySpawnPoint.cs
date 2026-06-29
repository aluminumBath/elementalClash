using UnityEngine;

namespace Elementborn.Game
{
    public sealed class TestEnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private EnemyCombatProfile profile;
        [SerializeField] private CombatAttackDefinition meleeAttack;
        [SerializeField] private bool spawnOnStart = false;
        [SerializeField] private bool targetPlayerOnSpawn = true;

        private GameObject spawned;

        private void Start()
        {
            if (spawnOnStart)
            {
                Spawn();
            }
        }

        [ContextMenu("Spawn Test Enemy")]
        public GameObject Spawn()
        {
            if (spawned != null)
            {
                Destroy(spawned);
            }

            spawned = enemyPrefab != null
                ? Instantiate(enemyPrefab, transform.position, transform.rotation)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            spawned.name = "Test Enemy";
            spawned.transform.position = transform.position;
            spawned.transform.rotation = transform.rotation;

            Ensure<SimpleCombatHealth>(spawned);
            Ensure<StatusEffectController>(spawned);
            Ensure<CombatResistanceProfile>(spawned);
            Ensure<CharacterController>(spawned);
            Ensure<EnemyMovementMotor>(spawned);
            Ensure<EnemyPerceptionSensor>(spawned);
            EnemyMeleeAttackDriver melee = Ensure<EnemyMeleeAttackDriver>(spawned);
            EnemyCombatBrain brain = Ensure<EnemyCombatBrain>(spawned);
            Ensure<EnemyFleeBehavior>(spawned);
            Ensure<EnemyWeaknessAwareSelector>(spawned);
            Ensure<EnemyAiDebugPanel>(spawned);

            if (meleeAttack != null)
            {
                melee.SetAttackDefinition(meleeAttack);
            }

            if (profile != null)
            {
                brain.SetProfile(profile);
            }

            if (targetPlayerOnSpawn)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    brain.ForceTarget(player.transform);
                }
            }

            return spawned;
        }

        private static T Ensure<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}
