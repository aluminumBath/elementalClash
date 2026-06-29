using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class SpellCastBarView : MonoBehaviour
    {
        [SerializeField] private SpellCastController caster;
        [SerializeField] private Slider slider;
        [SerializeField] private Text label;
        [SerializeField] private GameObject root;

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            bool active = caster != null && caster.State == SpellCastState.Casting && caster.CurrentSpell != null;

            if (root != null)
            {
                root.SetActive(active);
            }

            if (slider != null)
            {
                slider.value = active ? caster.CastProgress01 : 0f;
            }

            if (label != null)
            {
                label.text = active ? $"Casting {caster.CurrentSpell.DisplayName}" : "";
            }
        }
    }
}
