using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AnimatorCombatEventBridge : MonoBehaviour
    {
        [SerializeField] private MeleeCombatHitbox meleeHitbox;
        [SerializeField] private ProjectileCombatEmitter projectileEmitter;
        [SerializeField] private WeaponTrailController weaponTrail;

        private void Awake()
        {
            if (meleeHitbox == null)
            {
                meleeHitbox = GetComponentInChildren<MeleeCombatHitbox>();
            }

            if (projectileEmitter == null)
            {
                projectileEmitter = GetComponentInChildren<ProjectileCombatEmitter>();
            }

            if (weaponTrail == null)
            {
                weaponTrail = GetComponentInChildren<WeaponTrailController>();
            }
        }

        public void Anim_EnableWeaponTrail()
        {
            if (weaponTrail != null)
            {
                weaponTrail.PlayTrail();
            }
        }

        public void Anim_DisableWeaponTrail()
        {
            if (weaponTrail != null)
            {
                weaponTrail.StopTrail();
            }
        }

        public void Anim_PerformMeleeHit()
        {
            if (meleeHitbox != null)
            {
                meleeHitbox.PerformAttack();
            }
        }

        public void Anim_FireProjectile()
        {
            if (projectileEmitter != null)
            {
                projectileEmitter.Emit();
            }
        }
    }
}
