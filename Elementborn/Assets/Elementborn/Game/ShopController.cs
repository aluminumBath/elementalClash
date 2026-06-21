using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The buy menu. A single instance that any <see cref="Merchant"/> opens with its stock; it lists the
    /// items the player's element allows and that they don't already own, each as a button showing the price
    /// in gems. Buying spends from the wallet and toasts the result on the <see cref="GameHud"/>. While open
    /// it gates player combat + flat movement and frees the cursor. Code-built placeholder UI until art.
    /// </summary>
    public sealed class ShopController : MonoBehaviour
    {
        public static ShopController Instance { get; private set; }

        private Canvas _canvas;
        private Transform _list;
        private Text _title;
        private Merchant _merchant;

        private PlayerCombatController _combat;
        private MonoBehaviour _rig;
        private bool _wasCursorVisible;
        private CursorLockMode _wasCursorLock;

        private static readonly Color PanelColor = new Color(0.08f, 0.09f, 0.12f, 0.95f);
        private static readonly Color ButtonColor = new Color(0.22f, 0.5f, 0.85f, 1f);
        private static readonly Color CloseColor = new Color(0.6f, 0.25f, 0.28f, 1f);

        public static ShopController EnsureInstance()
        {
            if (Instance != null) return Instance;
            return new GameObject("Shop").AddComponent<ShopController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Build();
            _canvas.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Open(Merchant merchant)
        {
            _merchant = merchant;
            _title.text = merchant != null ? merchant.ShopName : "Shop";
            Gate(true);
            _canvas.gameObject.SetActive(true);
            Populate();
        }

        public void Close()
        {
            _canvas.gameObject.SetActive(false);
            Gate(false);
        }

        // --- population ---
        private void Populate()
        {
            for (int i = _list.childCount - 1; i >= 0; i--) Destroy(_list.GetChild(i).gameObject);

            var inv = PlayerInventory.Instance;
            if (inv == null || _merchant == null) return;

            foreach (var kind in _merchant.Creatures())
            {
                var info = CreatureCatalog.For(kind);
                if (!info.Purchasable || inv.Owns(kind) || !inv.CanUse(info)) continue;
                var k = kind;
                AddRow($"{info.Name} — {PriceText(info.Price)}", () =>
                {
                    inv.TryBuy(k, out string reason);
                    GameHud.Instance?.Toast(reason);
                    Populate();
                });
            }

            foreach (var kind in _merchant.Vehicles())
            {
                var info = VehicleCatalog.For(kind);
                if (inv.OwnsVehicle(kind) || !inv.CanUseVehicle(info)) continue;
                var k = kind;
                AddRow($"{info.Name} — {PriceText(info.Price)}", () =>
                {
                    inv.TryBuyVehicle(k, out string reason);
                    GameHud.Instance?.Toast(reason);
                    Populate();
                });
            }

            if (_list.childCount == 0) AddNote("Nothing here for you right now.");
        }

        private static string PriceText(long value)
        {
            var parts = new List<string>();
            foreach (var kv in Wallet.Breakdown(value))
                if (kv.Value > 0) parts.Add($"{kv.Value} {kv.Key}");
            return parts.Count > 0 ? string.Join(", ", parts) : "free";
        }

        // --- construction ---
        private void Build()
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("ShopCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 50;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_canvas.transform, false);
            var prt = (RectTransform)panel.transform;
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(620, 640);
            panel.GetComponent<Image>().color = PanelColor;

            _title = MakeText(panel.transform, "Shop", 40, new Vector2(0.5f, 1f), TextAnchor.UpperCenter,
                new Vector2(0, -24), new Vector2(580, 56));

            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            listGo.transform.SetParent(panel.transform, false);
            var lrt = (RectTransform)listGo.transform;
            lrt.anchorMin = lrt.anchorMax = lrt.pivot = new Vector2(0.5f, 1f);
            lrt.anchoredPosition = new Vector2(0, -96);
            lrt.sizeDelta = new Vector2(560, 0);
            var vlg = listGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            listGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            _list = listGo.transform;

            MakeButton(panel.transform, "Close", new Vector2(0.5f, 0f), new Vector2(0, 28), new Vector2(240, 56), CloseColor, Close);
        }

        private void AddRow(string label, Action onClick)
        {
            var go = new GameObject("Item", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(_list, false);
            go.GetComponent<Image>().color = ButtonColor;
            go.GetComponent<LayoutElement>().preferredHeight = 56;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
            MakeText(go.transform, label, 24, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, Vector2.zero, new Vector2(540, 56));
        }

        private void AddNote(string label)
        {
            var go = new GameObject("Note", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(_list, false);
            go.GetComponent<LayoutElement>().preferredHeight = 56;
            MakeText(go.transform, label, 22, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, Vector2.zero, new Vector2(540, 56));
        }

        // --- gating ---
        private void Gate(bool open)
        {
            if (open)
            {
                _combat = FindObjectOfType<PlayerCombatController>();
                _rig = FindObjectOfType<FirstPersonRig>();
                _wasCursorVisible = Cursor.visible;
                _wasCursorLock = Cursor.lockState;
            }

            if (_combat != null) _combat.enabled = !open;
            if (_rig != null) _rig.enabled = !open;

            if (open)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = _wasCursorVisible;
                Cursor.lockState = _wasCursorLock;
            }
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static Text MakeText(Transform parent, string content, int size, Vector2 anchor,
            TextAnchor align, Vector2 pos, Vector2 sz)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
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

        private void MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 sz, Color color, Action onClick)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = sz;
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().onClick.AddListener(() => onClick());
            MakeText(go.transform, label, 26, new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter, Vector2.zero, sz);
        }
    }
}
