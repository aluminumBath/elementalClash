using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class ProjectileCombatEmitter : MonoBehaviour
    {
        [SerializeField] private CombatAttackDefinition attackDefinition;
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifeSeconds = 5f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private GameObject owner;

        private void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            lifeSeconds -= Time.deltaTime;
            if (lifeSeconds <= 0f) Destroy(gameObject);
        }

        public void SetOwner(GameObject value) => owner = value;


        // v54 compatibility: animation event bridge calls Emit() on the emitter.
        // If no explicit projectile prefab is assigned, spawn a lightweight copy of this emitter.
        public void Emit()
        {
            Transform origin = transform;
            GameObject clone = Instantiate(gameObject, origin.position, origin.rotation);
            clone.name = gameObject.name + "_Projectile";
            clone.SetActive(true);

            var emitter = clone.GetComponent<ProjectileCombatEmitter>();
            if (emitter != null)
            {
                emitter.attackDefinition = attackDefinition;
                emitter.speed = speed;
                emitter.lifeSeconds = Mathf.Max(0.1f, lifeSeconds);
                emitter.destroyOnHit = destroyOnHit;
                emitter.owner = owner != null ? owner : transform.root.gameObject;
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            GameObject target = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            if (owner != null && target.transform.root == owner.transform.root) return;

            var context = new CombatHitContext
            {
                Source = owner != null ? owner : gameObject,
                AttackDefinition = attackDefinition,
                BaseDamage = attackDefinition != null ? attackDefinition.BaseDamage : 10f,
                Element = attackDefinition != null ? attackDefinition.Element : AbilityElementType.Neutral,
                CritChance = attackDefinition != null ? attackDefinition.CritChance : 0.05f,
                CritMultiplier = attackDefinition != null ? attackDefinition.CritMultiplier : 1.5f,
                KnockbackForce = attackDefinition != null ? attackDefinition.KnockbackForce : 0f,
                UseEquipmentBonuses = attackDefinition == null || attackDefinition.UseEquipmentBonuses,
                OriginType = attackDefinition != null ? attackDefinition.OriginType : AttackOriginType.OnFoot,
                StatusToApply = attackDefinition != null ? attackDefinition.StatusToApply : null,
                AttackName = attackDefinition != null ? attackDefinition.DisplayName : "Projectile"
            };

            CombatDamageUtility.ApplyHit(target, context);
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
