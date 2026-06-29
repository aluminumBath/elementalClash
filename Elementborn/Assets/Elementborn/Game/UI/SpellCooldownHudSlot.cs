using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class SpellCooldownHudSlot : MonoBehaviour
    {
        [SerializeField] private SpellCastDefinition spell;
        [SerializeField] private Image icon;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Text cooldownText;

        private void Update()
        {
            Refresh();
        }

        public void SetSpell(SpellCastDefinition value)
        {
            spell = value;
            Refresh();
        }

        public void Refresh()
        {
            if (icon != null)
            {
                icon.sprite = spell != null ? spell.Icon : null;
                icon.enabled = spell != null && spell.Icon != null;
            }

            if (spell == null)
            {
                if (cooldownFill != null) cooldownFill.fillAmount = 0f;
                if (cooldownText != null) cooldownText.text = "";
                return;
            }

            float remaining = SpellCooldownTracker.Remaining(spell.SpellId);
            float total = Mathf.Max(0.001f, spell.CooldownSeconds);
            float pct = Mathf.Clamp01(remaining / total);

            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = pct;
            }

            if (cooldownText != null)
            {
                cooldownText.text = remaining > 0.05f ? remaining.ToString("0.0") : "";
            }
        }
    }
}
