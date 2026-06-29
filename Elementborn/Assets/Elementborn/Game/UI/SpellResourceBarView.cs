using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class SpellResourceBarView : MonoBehaviour
    {
        [SerializeField] private SpellResourcePool resourcePool;
        [SerializeField] private Slider slider;
        [SerializeField] private Text label;

        private void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (resourcePool == null)
            {
                return;
            }

            if (slider != null)
            {
                slider.value = resourcePool.Normalized;
            }

            if (label != null)
            {
                label.text = $"{resourcePool.ResourceType}: {resourcePool.CurrentValue:0}/{resourcePool.MaxValue:0}";
            }
        }
    }
}
