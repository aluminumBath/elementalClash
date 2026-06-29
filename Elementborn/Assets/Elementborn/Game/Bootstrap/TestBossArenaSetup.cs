using UnityEngine;

namespace Elementborn.Game
{
    public sealed class TestBossArenaSetup : MonoBehaviour
    {
        [SerializeField] private BossDefinition bossDefinition;
        [SerializeField] private EnemyCombatProfile bossProfile;
        [SerializeField] private CombatAttackDefinition bossAttack;
        [SerializeField] private bool buildOnStart = false;

        public BossController Boss { get; private set; }
        public BossArenaController Arena { get; private set; }

        private void Start()
        {
            if (buildOnStart)
            {
                Build();
            }
        }

        [ContextMenu("Build Test Boss Arena")]
        public void Build()
        {
            GameObject arenaObject = new GameObject("Test Boss Arena");
            arenaObject.transform.position = transform.position;
            Arena = arenaObject.AddComponent<BossArenaController>();

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Arena Floor";
            floor.transform.SetParent(arenaObject.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(18f, 0.2f, 18f);

            GameObject trigger = new GameObject("Boss Arena Trigger");
            trigger.transform.SetParent(arenaObject.transform);
            trigger.transform.localPosition = new Vector3(0f, 1f, -7f);
            BoxCollider triggerCollider = trigger.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(6f, 3f, 2f);
            BossArenaTrigger arenaTrigger = trigger.AddComponent<BossArenaTrigger>();

            GameObject bossObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bossObject.name = bossDefinition != null ? bossDefinition.DisplayName : "Test Boss";
            bossObject.transform.SetParent(arenaObject.transform);
            bossObject.transform.localPosition = new Vector3(0f, 1.2f, 4f);
            bossObject.transform.localScale = new Vector3(2f, 2f, 2f);

            bossObject.AddComponent<SimpleCombatHealth>();
            bossObject.AddComponent<StatusEffectController>();
            bossObject.AddComponent<CombatResistanceProfile>();
            bossObject.AddComponent<CharacterController>();
            bossObject.AddComponent<EnemyMovementMotor>();
            bossObject.AddComponent<EnemyPerceptionSensor>();
            var melee = bossObject.AddComponent<EnemyMeleeAttackDriver>();
            var brain = bossObject.AddComponent<EnemyCombatBrain>();
            bossObject.AddComponent<EnemyWeaknessAwareSelector>();
            Boss = bossObject.AddComponent<BossController>();

            if (bossAttack != null)
            {
                melee.SetAttackDefinition(bossAttack);
            }

            if (bossProfile != null)
            {
                brain.SetProfile(bossProfile);
            }

            Boss.Configure(bossDefinition, Arena);
            arenaTrigger.Configure(Boss);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
