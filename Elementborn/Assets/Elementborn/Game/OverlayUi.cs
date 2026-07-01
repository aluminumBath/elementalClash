using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>Small shared builders for the code-built overlay panels (dialogue, quest log), so they don't
    /// duplicate the canvas/title/close scaffolding. Built on <see cref="UiTheme"/>.</summary>
    public static class OverlayUi
    {
        /// <summary>A hidden, centered panel with a title and a Close button. Returns the canvas to toggle and the
        /// vertical-stack content area to fill.</summary>
        public static (Canvas canvas, Transform content) Panel(string name, string title, int sortOrder, Vector2 size, System.Action onClose)
        {
            var canvas = UiTheme.Canvas(name, sortOrder);
            canvas.gameObject.AddComponent<VrCanvasAdapter>(); // world-space in VR; no-op on flat/desktop
            var gateToken = canvas.gameObject.AddComponent<UiGateToken>(); // modal: holds the Esc gate while on-screen
            gateToken.onForceClose = onClose; // single-modal: opening another exclusive panel closes this one cleanly

            var root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rr = UiTheme.Rect(root);
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = size;
            rr.anchoredPosition = Vector2.zero;
            root.AddComponent<Image>().color = UiTheme.PanelColor;

            if (!string.IsNullOrEmpty(title))
            {
                var t = UiTheme.Label(root.transform, title, 32, UiTheme.TextColor, TextAnchor.MiddleLeft);
                var tr = t.Rect;
                tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f);
                tr.sizeDelta = new Vector2(-200, 52); tr.anchoredPosition = new Vector2(28, -14);
            }

            var close = UiTheme.Button(root.transform, Localization.T("ui.close"), onClose, 150, 42);
            var cr = UiTheme.Rect(close.gameObject);
            cr.anchorMin = cr.anchorMax = new Vector2(1f, 1f); cr.pivot = new Vector2(1f, 1f);
            cr.anchoredPosition = new Vector2(-18, -14);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(root.transform, false);
            var cc = UiTheme.Rect(content);
            cc.anchorMin = Vector2.zero; cc.anchorMax = Vector2.one;
            cc.offsetMin = new Vector2(24, 20); cc.offsetMax = new Vector2(-24, -76);
            var v = content.AddComponent<VerticalLayoutGroup>();
            v.spacing = 8; v.childControlWidth = true; v.childControlHeight = false;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false; v.childAlignment = TextAnchor.UpperLeft;

            return (canvas, content.transform);
        }

        public static UiLabel Header(Transform parent, string text, int size = 24)
        {
            var l = UiTheme.Label(parent, text, size, UiTheme.TextColor, TextAnchor.MiddleLeft);
            var le = l.Graphic.gameObject.AddComponent<LayoutElement>();
            le.minHeight = size + 10; le.preferredHeight = size + 10;
            return l;
        }

        public static UiLabel Body(Transform parent, string text, int size = 20, Color? color = null)
        {
            var l = UiTheme.Label(parent, text, size, color ?? UiTheme.TextColor, TextAnchor.UpperLeft);
            var le = l.Graphic.gameObject.AddComponent<LayoutElement>();
            le.minHeight = size + 8; le.preferredHeight = size + 8;
            return l;
        }

        public static void Clear(Transform content)
        {
            if (content == null) return;
            for (int i = content.childCount - 1; i >= 0; i--) Object.DestroyImmediate(content.GetChild(i).gameObject);
        }
    }
}
