using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The Grimoire (default key G): a discovery-driven tome with three tabbed sections — Bestiary, Attacks and
    /// Bloodlines — themed after a maroon-and-gold book. Every entry stays "???" until the player does the thing
    /// that reveals it (sights / defeats / tames a creature, casts an attack, carries or meets a bloodline), and a
    /// tier never downgrades. Holds the live <see cref="GrimoireProgress"/>, subscribes to the gameplay events on
    /// <see cref="QuestEvents"/> that fill it in, and persists through <see cref="PlayerInventory"/>. The bootstrap
    /// scene adds one.
    /// </summary>
    public sealed class GrimoireController : MonoBehaviour
    {
        public static GrimoireController Instance { get; private set; }

        [SerializeField] private Key toggleKey = Key.G;

        // Tome palette — echoes the maroon cover, gilt corners, and aged-parchment pages of the reference art.
        private static readonly Color Cover   = new Color(0.27f, 0.09f, 0.09f, 0.98f); // book cover / margins
        private static readonly Color Page     = new Color(0.92f, 0.86f, 0.72f, 1f);    // parchment
        private static readonly Color Gold     = new Color(0.85f, 0.69f, 0.33f, 1f);    // gilt headings
        private static readonly Color Ink      = new Color(0.32f, 0.17f, 0.11f, 1f);    // body text on parchment
        private static readonly Color Crimson  = new Color(0.62f, 0.18f, 0.13f, 1f);    // accents / mastered
        private static readonly Color Locked   = new Color(0.52f, 0.42f, 0.33f, 1f);    // undiscovered "???"
        private static readonly Color TabIdle  = new Color(0.40f, 0.15f, 0.13f, 1f);

        private GrimoireProgress _progress = new GrimoireProgress();
        public GrimoireProgress Progress => _progress;

        private Canvas _canvas;
        private Transform _listContent;
        private UiLabel _countLabel;
        private readonly Dictionary<GrimoireSection, Image> _tabImages = new Dictionary<GrimoireSection, Image>();
        private GrimoireSection _section = GrimoireSection.Bestiary;
        private bool _open;
        private bool _seededOwnBloodlines;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Build();
            Hide();
        }

        private void OnEnable()
        {
            QuestEvents.CreatureSighted += OnSighted;
            QuestEvents.CreatureDefeated += OnDefeated;
            QuestEvents.CreatureTamed += OnTamed;
            QuestEvents.AbilityCast += OnCast;
        }

        private void OnDisable()
        {
            QuestEvents.CreatureSighted -= OnSighted;
            QuestEvents.CreatureDefeated -= OnDefeated;
            QuestEvents.CreatureTamed -= OnTamed;
            QuestEvents.AbilityCast -= OnCast;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        // --- discovery hooks (a tier only ever advances) ---
        private void OnSighted(string kind) { if (Enum.TryParse(kind, out CreatureKind k) && _progress.RecordSighting(k)) Refresh(); }
        private void OnDefeated(string kind) { if (Enum.TryParse(kind, out CreatureKind k) && _progress.RecordDefeat(k)) Refresh(); }
        private void OnTamed(string kind) { if (Enum.TryParse(kind, out CreatureKind k) && _progress.RecordTame(k)) Refresh(); }

        private void OnCast(string element, string intent)
        {
            if (!Enum.TryParse(element, out Element e) || !Enum.TryParse(intent, out IntentType it)) return;
            bool changed = _progress.RecordCast(e, it);
            changed |= _progress.RecordBloodlineSeen(Bloodlines.ForElement(e)); // channeling implies carrying the line
            if (changed) Refresh();
        }

        /// <summary>Glimpse the bloodlines the player actually carries (their loadout / Confluence). Idempotent —
        /// re-running never downgrades, so it is safe to call again after a save load or each time the book opens.</summary>
        private void SeedOwnBloodlines()
        {
            var pi = PlayerInventory.Instance;
            if (pi == null || pi.Loadout == null) return;
            bool changed = false;
            if (pi.PlayerIsConfluence) changed |= _progress.RecordBloodlineSeen(BloodlineId.Confluence);
            foreach (var el in pi.Loadout.Elements) changed |= _progress.RecordBloodlineSeen(Bloodlines.ForElement(el));
            foreach (var sa in pi.Loadout.SubArts)
                if (Bloodlines.TryForSubArt(sa, out var id)) changed |= _progress.RecordBloodlineSeen(id);
            _seededOwnBloodlines = true;
            if (changed && _open) Rebuild();
        }

        // --- persistence (folded into PlayerInventory.ToSave / LoadFrom) ---
        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.grimoireKeys.Clear();
            d.grimoireTiers.Clear();
            foreach (var kv in _progress.ToSave()) { d.grimoireKeys.Add(kv.Key); d.grimoireTiers.Add(kv.Value); }
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            var map = new Dictionary<string, int>();
            int n = Mathf.Min(d.grimoireKeys.Count, d.grimoireTiers.Count);
            for (int i = 0; i < n; i++) map[d.grimoireKeys[i]] = d.grimoireTiers[i];
            _progress = GrimoireProgress.LoadFrom(map);
            _seededOwnBloodlines = false; // re-seed carried lines against the restored loadout when next opened
            if (_open) Rebuild();
        }

        // --- show / hide ---
        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }

        private void Show()
        {
            _open = true;
            if (!_seededOwnBloodlines) SeedOwnBloodlines();
            if (_canvas != null) _canvas.gameObject.SetActive(true);
            Rebuild();
        }

        private void Hide()
        {
            _open = false;
            if (_canvas != null) _canvas.gameObject.SetActive(false);
        }

        private void Refresh() { if (_open) Rebuild(); }

        // --- build (direct on UiTheme so the whole panel can wear the tome palette) ---
        private void Build()
        {
            _canvas = UiTheme.Canvas("GrimoireCanvas", 56);
            _canvas.gameObject.AddComponent<VrCanvasAdapter>();

            var root = new GameObject("Root", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(_canvas.transform, false);
            var rr = (RectTransform)root.transform;
            rr.anchorMin = rr.anchorMax = new Vector2(0.5f, 0.5f);
            rr.sizeDelta = new Vector2(1120, 720);
            rr.anchoredPosition = Vector2.zero;
            root.GetComponent<Image>().color = Cover;

            var title = UiTheme.Label(root.transform, "Grimoire", 34, Gold, TextAnchor.MiddleLeft);
            var tr = title.Rect;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f); tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(-220, 54); tr.anchoredPosition = new Vector2(30, -14);

            var close = UiTheme.Button(root.transform, "Close (Esc)", Hide, 150, 42);
            var cr = UiTheme.Rect(close.gameObject);
            cr.anchorMin = cr.anchorMax = new Vector2(1f, 1f); cr.pivot = new Vector2(1f, 1f);
            cr.anchoredPosition = new Vector2(-18, -16);

            // Tab row
            var tabs = new GameObject("Tabs", typeof(RectTransform));
            tabs.transform.SetParent(root.transform, false);
            var tabsR = (RectTransform)tabs.transform;
            tabsR.anchorMin = new Vector2(0f, 1f); tabsR.anchorMax = new Vector2(1f, 1f); tabsR.pivot = new Vector2(0.5f, 1f);
            tabsR.sizeDelta = new Vector2(-48, 46); tabsR.anchoredPosition = new Vector2(0, -76);
            var hl = tabs.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8; hl.childControlWidth = true; hl.childControlHeight = true;
            hl.childForceExpandWidth = false; hl.childForceExpandHeight = true; hl.childAlignment = TextAnchor.MiddleLeft;
            AddTab(tabs.transform, GrimoireSection.Bestiary, "Bestiary");
            AddTab(tabs.transform, GrimoireSection.Attacks, "Attacks");
            AddTab(tabs.transform, GrimoireSection.Bloodlines, "Bloodlines");

            _countLabel = UiTheme.Label(root.transform, "", 20, Gold, TextAnchor.MiddleRight);
            var clr = _countLabel.Rect;
            clr.anchorMin = new Vector2(0f, 1f); clr.anchorMax = new Vector2(1f, 1f); clr.pivot = new Vector2(0.5f, 1f);
            clr.sizeDelta = new Vector2(-48, 26); clr.anchoredPosition = new Vector2(0, -128);

            _listContent = BuildScroll(root.transform);
        }

        private void AddTab(Transform parent, GrimoireSection section, string label)
        {
            var btn = UiTheme.Button(parent, label, () => { _section = section; Rebuild(); }, 220, 44);
            _tabImages[section] = btn.image;
        }

        private Transform BuildScroll(Transform parent)
        {
            var scrollGo = new GameObject("Pages", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            var sr = (RectTransform)scrollGo.transform;
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(24, 24); sr.offsetMax = new Vector2(-24, -160);
            scrollGo.GetComponent<Image>().color = Page; // parchment backdrop (also the wheel raycast target)

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false; scroll.vertical = true; scroll.scrollSensitivity = 26f; scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollGo.transform, false);
            var vr = (RectTransform)viewport.transform;
            vr.anchorMin = Vector2.zero; vr.anchorMax = Vector2.one;
            vr.offsetMin = new Vector2(18, 14); vr.offsetMax = new Vector2(-18, -14);

            var content = new GameObject("ListContent", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var cc = (RectTransform)content.transform;
            cc.anchorMin = new Vector2(0f, 1f); cc.anchorMax = new Vector2(1f, 1f); cc.pivot = new Vector2(0.5f, 1f);
            cc.anchoredPosition = Vector2.zero; cc.sizeDelta = new Vector2(0, 0);
            var v = content.AddComponent<VerticalLayoutGroup>();
            v.spacing = 10; v.padding = new RectOffset(6, 6, 6, 6);
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false; v.childAlignment = TextAnchor.UpperLeft;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vr;
            scroll.content = cc;
            return content.transform;
        }

        private void Rebuild()
        {
            if (_listContent == null) return;
            for (int i = _listContent.childCount - 1; i >= 0; i--) DestroyImmediate(_listContent.GetChild(i).gameObject);

            foreach (var kv in _tabImages) kv.Value.color = kv.Key == _section ? Gold : TabIdle;

            var entries = GrimoireCatalog.ForSection(_section);
            int discovered = _progress.CountDiscovered(_section);
            if (_countLabel != null) _countLabel.text = discovered + " / " + entries.Count + " discovered";

            foreach (var entry in entries)
            {
                var tier = _progress.TierOf(_section, entry.Id);
                AddEntryRow(Grimoire.Redact(entry, tier));
            }
        }

        private void AddEntryRow(RedactedEntry r)
        {
            var row = new GameObject("Entry", typeof(RectTransform));
            row.transform.SetParent(_listContent, false);
            var vlg = row.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2; vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false; vlg.childAlignment = TextAnchor.UpperLeft;

            bool locked = r.IsLocked;
            string headText = locked ? Grimoire.Hidden : r.Name + "   ·   " + TierTag(r.Tier);
            UiTheme.Label(row.transform, headText, 23,
                locked ? Locked : (r.Tier == DiscoveryTier.Mastered ? Crimson : Gold), TextAnchor.MiddleLeft);

            if (r.Lines != null)
                foreach (var line in r.Lines)
                    if (!string.IsNullOrEmpty(line))
                        UiTheme.Label(row.transform, "  •  " + line, 18, Ink, TextAnchor.UpperLeft);
        }

        private static string TierTag(DiscoveryTier t)
        {
            switch (t)
            {
                case DiscoveryTier.Glimpsed: return "Glimpsed";
                case DiscoveryTier.Known: return "Known";
                case DiscoveryTier.Mastered: return "Mastered";
                default: return "";
            }
        }
    }
}
