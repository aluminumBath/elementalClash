using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/UI/UI Theme", fileName = "ElementbornUiTheme")]
    public sealed class ElementbornUiTheme : ScriptableObject
    {
        [Header("Sprites")]
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite questPanelSprite;
        [SerializeField] private Sprite spellBarSprite;
        [SerializeField] private Sprite bossFrameSprite;

        [Header("Colors")]
        [SerializeField] private Color textColor = new Color(1f, 0.96f, 0.86f, 1f);
        [SerializeField] private Color accentColor = new Color(1f, 0.78f, 0.28f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, 0.42f, 0.32f, 1f);
        [SerializeField] private Color successColor = new Color(0.55f, 1f, 0.45f, 1f);

        [Header("Typography")]
        [SerializeField] private int titleSize = 26;
        [SerializeField] private int bodySize = 18;
        [SerializeField] private int smallSize = 14;

        public Sprite PanelSprite => panelSprite;
        public Sprite ButtonSprite => buttonSprite;
        public Sprite QuestPanelSprite => questPanelSprite;
        public Sprite SpellBarSprite => spellBarSprite;
        public Sprite BossFrameSprite => bossFrameSprite;
        public Color TextColor => textColor;
        public Color AccentColor => accentColor;
        public Color WarningColor => warningColor;
        public Color SuccessColor => successColor;
        public int TitleSize => Mathf.Max(8, titleSize);
        public int BodySize => Mathf.Max(8, bodySize);
        public int SmallSize => Mathf.Max(8, smallSize);
    }
}
