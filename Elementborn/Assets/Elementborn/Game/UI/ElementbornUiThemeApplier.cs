using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    public sealed class ElementbornUiThemeApplier : MonoBehaviour
    {
        [SerializeField] private ElementbornUiTheme theme;
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool includeInactive = true;

        private void Start()
        {
            if (applyOnStart)
            {
                Apply();
            }
        }

        [ContextMenu("Apply Theme")]
        public void Apply()
        {
            if (theme == null)
            {
                return;
            }

            foreach (var text in GetComponentsInChildren<Text>(includeInactive))
            {
                if (text == null)
                {
                    continue;
                }

                text.color = theme.TextColor;
                if (text.fontSize <= 14)
                {
                    text.fontSize = theme.SmallSize;
                }
                else if (text.fontSize >= 24)
                {
                    text.fontSize = theme.TitleSize;
                }
                else
                {
                    text.fontSize = theme.BodySize;
                }
            }

            foreach (var image in GetComponentsInChildren<Image>(includeInactive))
            {
                if (image == null)
                {
                    continue;
                }

                if (image.sprite == null && image.gameObject.name.ToLowerInvariant().Contains("panel"))
                {
                    image.sprite = theme.PanelSprite;
                    image.type = Image.Type.Sliced;
                }

                if (image.sprite == null && image.gameObject.name.ToLowerInvariant().Contains("button"))
                {
                    image.sprite = theme.ButtonSprite;
                    image.type = Image.Type.Sliced;
                }
            }
        }

        public void SetTheme(ElementbornUiTheme value)
        {
            theme = value;
            Apply();
        }
    }
}
