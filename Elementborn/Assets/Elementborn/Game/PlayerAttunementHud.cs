using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A compact HUD readout of the player's elemental attunement: their own element, plus how they fare
    /// defensively against each element (RESIST / WEAK / neutral). Each element stays "???" until the player has
    /// <em>encountered</em> it — either by facing an opponent of that element (a periodic proximity check over live
    /// enemies) or by being struck by a move of it (the player's <see cref="Damageable"/> damage feed). The player's
    /// own element is known from the start. Discovery lives in a pure <see cref="ElementDex"/> and persists with the
    /// save (folded into <see cref="PlayerInventory"/>). Self-bootstrapping; finds the player by tag.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAttunementHud : MonoBehaviour
    {
        public static PlayerAttunementHud Instance { get; private set; }

        private const float ScanInterval = 0.5f;
        private const float EncounterRange = 22f;

        private static readonly Element[] Order = { Element.Fire, Element.Water, Element.Earth, Element.Air };
        private static readonly Color WeakColor = new Color(1f, 0.55f, 0.20f);
        private static readonly Color ResistColor = new Color(0.55f, 0.78f, 1f);
        private static readonly Color NeutralColor = new Color(0.70f, 0.72f, 0.78f);
        private static readonly Color SelfColor = new Color(0.93f, 0.93f, 0.97f);
        private static readonly Color LockedText = new Color(0.50f, 0.51f, 0.55f);
        private static readonly Color LockedPip = new Color(0.30f, 0.31f, 0.35f);

        private ElementDex _dex = new ElementDex();
        private Damageable _body;
        private bool _subscribed;
        private float _scanTimer;
        private readonly List<Element> _scratch = new List<Element>();

        private Canvas _canvas;
        private readonly Image[] _pips = new Image[4];
        private readonly UiLabel[] _rows = new UiLabel[4];
        private int _signature = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("PlayerAttunementHud");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PlayerAttunementHud>();
            Instance.Build();
        }

        private void Awake() { if (Instance == null) Instance = this; }

        private void OnDestroy()
        {
            if (_subscribed && _body != null && _body.Health != null) _body.Health.Damaged -= OnPlayerHit;
            if (Instance == this) Instance = null;
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("PlayerAttunement", 8);
            DontDestroyOnLoad(_canvas.gameObject);

            var vr = _canvas.gameObject.AddComponent<VrHudAnchor>();
            vr.viewOffset = new Vector3(-0.45f, -0.22f, 1.5f); vr.worldSize = new Vector2(340f, 320f);

            var panel = UiTheme.Panel(_canvas.transform, new Color(0.05f, 0.06f, 0.09f, 0.72f));
            var pr = panel.rectTransform;
            pr.anchorMin = pr.anchorMax = new Vector2(0f, 0.5f);
            pr.pivot = new Vector2(0f, 0.5f);
            pr.sizeDelta = new Vector2(190f, 176f);
            pr.anchoredPosition = new Vector2(12f, 0f);

            var title = UiTheme.Label(panel.rectTransform, "ATTUNEMENT", 15, new Color(0.86f, 0.87f, 0.92f), TextAnchor.UpperLeft);
            var tr = title.Rect;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0f, 1f);
            tr.sizeDelta = new Vector2(-16f, 24f); tr.anchoredPosition = new Vector2(12f, -10f);

            for (int i = 0; i < 4; i++)
            {
                float y = -42f - i * 32f;

                var pip = UiTheme.Panel(panel.rectTransform, LockedPip);
                var pipR = pip.rectTransform;
                pipR.anchorMin = pipR.anchorMax = new Vector2(0f, 1f); pipR.pivot = new Vector2(0f, 1f);
                pipR.sizeDelta = new Vector2(16f, 16f); pipR.anchoredPosition = new Vector2(14f, y - 4f);
                _pips[i] = pip;

                var row = UiTheme.Label(panel.rectTransform, "???", 16, LockedText, TextAnchor.MiddleLeft);
                var rr = row.Rect;
                rr.anchorMin = new Vector2(0f, 1f); rr.anchorMax = new Vector2(1f, 1f); rr.pivot = new Vector2(0f, 1f);
                rr.sizeDelta = new Vector2(-44f, 26f); rr.anchoredPosition = new Vector2(40f, y);
                _rows[i] = row;
            }
        }

        private void Update()
        {
            if (!_subscribed)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null) _body = tagged.GetComponentInParent<Damageable>();
                if (_body != null && _body.Health != null) { _body.Health.Damaged += OnPlayerHit; _subscribed = true; }
            }

            // Your own element is always known.
            var loadout = PlayerInventory.Instance != null ? PlayerInventory.Instance.Loadout : null;
            Element? mine = (loadout != null && loadout.IsChanneler) ? loadout.Elements[0] : (Element?)null;
            if (mine.HasValue) _dex.Discover(mine.Value);

            // Opponents of an element nearby = an encounter.
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= ScanInterval && _body != null)
            {
                _scanTimer = 0f;
                _scratch.Clear();
                StaggerController.CollectNearbyAffinities(_body.transform.position, EncounterRange, _scratch);
                for (int i = 0; i < _scratch.Count; i++) _dex.Discover(_scratch[i]);
            }

            RefreshIfChanged(mine);
        }

        // Being struck by a move of an element counts as encountering it.
        private void OnPlayerHit(DamageInfo info) => _dex.Discover(info.Source);

        private void RefreshIfChanged(Element? mine)
        {
            int mineCode = mine.HasValue ? (int)mine.Value + 1 : 0;
            int mask = 0;
            for (int i = 0; i < 4; i++) if (_dex.IsDiscovered(Order[i])) mask |= 1 << i;
            int sig = (mineCode << 4) | mask;
            if (sig == _signature) return;
            _signature = sig;

            for (int i = 0; i < 4; i++)
            {
                Element el = Order[i];
                bool isMine = mine.HasValue && mine.Value == el;
                if (isMine)
                {
                    _pips[i].color = ElementColor.For(el);
                    _rows[i].text = el + "  (you)";
                    _rows[i].SetColor(SelfColor);
                }
                else if (mine.HasValue && _dex.IsDiscovered(el))
                {
                    _pips[i].color = ElementColor.For(el);
                    var eff = ElementMatchup.Classify(el, mine.Value); // el attacks, the player defends
                    if (eff == Effectiveness.Strong) { _rows[i].text = el + "  WEAK"; _rows[i].SetColor(WeakColor); }
                    else if (eff == Effectiveness.Weak) { _rows[i].text = el + "  RESIST"; _rows[i].SetColor(ResistColor); }
                    else { _rows[i].text = el + "  neutral"; _rows[i].SetColor(NeutralColor); }
                }
                else
                {
                    _pips[i].color = LockedPip;
                    _rows[i].text = "???";
                    _rows[i].SetColor(LockedText);
                }
            }
        }

        // --- persistence (folded into PlayerInventory.ToSave / LoadFrom) ---
        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.discoveredElements.Clear();
            foreach (var s in _dex.ToSave()) d.discoveredElements.Add(s);
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            _dex = ElementDex.LoadFrom(d.discoveredElements);
            _signature = -1; // force a redraw
        }
    }
}
