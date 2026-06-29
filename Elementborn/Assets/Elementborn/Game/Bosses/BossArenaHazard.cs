using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(Collider))]
    public sealed class BossArenaHazard : MonoBehaviour
    {
        [SerializeField] private CombatAttackDefinition hazardAttack; [SerializeField] private float tickSeconds = 1f; [SerializeField] private string playerTag = "Player"; private float nextTickAt;
        private void Reset(){ GetComponent<Collider>().isTrigger = true; }
        private void OnTriggerStay(Collider other){ if(Time.unscaledTime < nextTickAt) return; if(!string.IsNullOrWhiteSpace(playerTag) && !other.CompareTag(playerTag) && !other.transform.root.CompareTag(playerTag)) return; nextTickAt = Time.unscaledTime + Mathf.Max(0.1f, tickSeconds); GameObject target = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject; var context = new CombatHitContext{Source=gameObject,AttackDefinition=hazardAttack,BaseDamage=hazardAttack!=null?hazardAttack.BaseDamage:8f,Element=hazardAttack!=null?hazardAttack.Element:AbilityElementType.Fire,CritChance=0f,CritMultiplier=1f,KnockbackForce=hazardAttack!=null?hazardAttack.KnockbackForce:0f,UseEquipmentBonuses=false,OriginType=AttackOriginType.OnFoot,StatusToApply=hazardAttack!=null?hazardAttack.StatusToApply:null,AttackName=hazardAttack!=null?hazardAttack.DisplayName:"Arena Hazard"}; CombatDamageUtility.ApplyHit(target, context); }
    }
}
