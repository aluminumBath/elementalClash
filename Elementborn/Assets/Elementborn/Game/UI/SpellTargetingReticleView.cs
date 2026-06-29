using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SpellTargetingReticleView : MonoBehaviour
    {
        [SerializeField] private SpellCastController caster;
        [SerializeField] private SpellCastDefinition previewSpell;
        [SerializeField] private Transform reticle;
        [SerializeField] private bool followCasterAim = true;

        private void Update()
        {
            if (reticle == null || previewSpell == null || caster == null)
            {
                return;
            }

            if (!followCasterAim)
            {
                return;
            }

            Vector3 point = caster.GetDefaultGroundPoint(previewSpell);
            reticle.position = point;
            reticle.localScale = Vector3.one * Mathf.Max(0.5f, previewSpell.Radius * 2f);
        }

        public void SetPreviewSpell(SpellCastDefinition spell)
        {
            previewSpell = spell;
            if (reticle != null)
            {
                reticle.gameObject.SetActive(spell != null);
            }
        }
    }
}
