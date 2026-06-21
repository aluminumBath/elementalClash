using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The on-screen heads-up display: a currency readout (top-left, polled from the player's wallet), a
    /// contextual interaction prompt (bottom-centre, driven by <see cref="PlayerInteractor"/>), and a
    /// transient toast for results like a tame or a purchase. Singleton so interaction and shops can post
    /// to it. Builds its own canvas; drop it on any persistent object. Code-built placeholder UI until art.
    /// </summary>
    public sealed class GameHud : MonoBehaviour
    {
        public static GameHud Instance { get; private set; }

        [SerializeField] private float toastSeconds = 2.5f;

        private Text _currency;
        private Text _prompt;
        private Text _toast;
        private float _toastTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start() => Build();

        private void Update()
        {
            if (_currency != null && PlayerInventory.Instance != null)
                _currency.text = Format(PlayerInventory.Instance.Wallet);

            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f && _toast != null) _toast.text = "";
            }
        }

        /// <summary>Show (or clear, when null/empty) the contextual interaction prompt.</summary>
        public void SetPrompt(string text)
        {
            if (_prompt != null) _prompt.text = text ?? "";
        }

        /// <summary>Flash a brief message in the centre of the screen.</summary>
        public void Toast(string text)
        {
            if (_toast == null) return;
            _toast.text = text ?? "";
            _toastTimer = toastSeconds;
        }

        private static string Format(Wallet w) =>
            $"Dia {w.CountOf(Currency.Diamond)}   Sap {w.CountOf(Currency.Sapphire)}   " +
            $"Eme {w.CountOf(Currency.Emerald)}   Rub {w.CountOf(Currency.Ruby)}   Sil {w.CountOf(Currency.Silver)}";

        private void Build()
        {
            var canvasGo = new GameObject("GameHud", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);

            _currency = Label(canvasGo.transform, "", 24, new Vector2(0, 1), TextAnchor.UpperLeft,
                new Vector2(24, -20), new Vector2(640, 40));

            _prompt = Label(canvasGo.transform, "", 26, new Vector2(0.5f, 0), TextAnchor.LowerCenter,
                new Vector2(0, 90), new Vector2(720, 40));
            _prompt.color = new Color(1f, 0.95f, 0.7f);

            _toast = Label(canvasGo.transform, "", 30, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter,
                new Vector2(0, 150), new Vector2(820, 48));
            _toast.color = new Color(0.72f, 1f, 0.8f);
        }

        private static Text Label(Transform parent, string content, int size, Vector2 anchor,
            TextAnchor align, Vector2 pos, Vector2 sz)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = sz;
            rt.anchoredPosition = pos;

            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.text = content;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
