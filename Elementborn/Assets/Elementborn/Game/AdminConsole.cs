using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>A developer admin / cheat console — toggle with the <b>backquote</b> key (Escape closes). It exposes
    /// the game's major levers both as one-tap actions (heal, currency, advance the story, trigger the tower blast,
    /// discover rifts, teleport) and as live controls (god mode, hazards on/off, time scale, set affinity), and it
    /// reads current values so it doubles as a state inspector — i.e. cheat codes with a form. Built with
    /// <see cref="OverlayUi"/>/<see cref="UiTheme"/> so it's world-space in VR. The bootstrap scene adds one.</summary>
    public sealed class AdminConsole : MonoBehaviour
    {
        [SerializeField] private Key toggleKey = Key.Backquote;

        private static readonly Color Section = new Color(0.70f, 0.85f, 1f, 1f);

        private Canvas _canvas;
        private Transform _content;
        private bool _open;
        private bool _godMode;

        private void Awake() { Build(); Hide(); }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
            else if (_open && kb[Key.Escape].wasPressedThisFrame) Hide();
        }

        public void Open() { if (!_open) Show(); }
        private void Toggle() { if (_open) Hide(); else Show(); }
        private void Show() { _open = true; if (_canvas != null) _canvas.gameObject.SetActive(true); Rebuild(); }
        private void Hide() { _open = false; if (_canvas != null) _canvas.gameObject.SetActive(false); }

        private void Build()
        {
            var p = OverlayUi.Panel("AdminCanvas", "Admin / Cheats", 56, new Vector2(780, 900), Hide);
            _canvas = p.canvas;
            _content = p.content;
        }

        private void Rebuild()
        {
            if (_content == null) return;
            OverlayUi.Clear(_content);

            // Current state — the inspector line.
            var body = PlayerBody();
            var inv = PlayerInventory.Instance;
            string hp = body != null && body.Health != null
                ? Mathf.CeilToInt(body.Health.Current) + "/" + Mathf.CeilToInt(body.Health.Max) : "\u2014";
            string silver = inv != null ? inv.Wallet.CountOf(Currency.Silver).ToString() : "\u2014";
            string affinity = body != null && body.Affinity.HasValue ? body.Affinity.Value.ToString() : "none";
            string chapter = StoryController.Instance != null ? StoryController.Instance.Chapter.ToString() : "\u2014";
            OverlayUi.Header(_content,
                "HP " + hp + "   \u00b7   Silver " + silver + "   \u00b7   Affinity " + affinity + "   \u00b7   Chapter " + chapter);

            // One-tap actions.
            OverlayUi.Body(_content, "Quick actions", 20, Section);
            UiTheme.Button(_content, "Heal to full", () => { PlayerBody()?.Health?.Revive(); Rebuild(); });
            UiTheme.Button(_content, "+1000 Silver", () => { PlayerInventory.Instance?.AddCurrency(Currency.Silver, 1000); Rebuild(); });
            UiTheme.Button(_content, "Advance story chapter", () => { StoryController.Instance?.Advance(); Rebuild(); });
            UiTheme.Button(_content, "Trigger Convergence Tower blast", () => FindObjectOfType<ConcordSite>()?.Detonate());
            UiTheme.Button(_content, "Discover all leyline rifts", DiscoverAllRifts);
            UiTheme.Button(_content, "Teleport to Concord", TeleportToConcord);
            UiTheme.Button(_content, "Cricket: toggle earring form", () => CricketCompanion.Instance?.ToggleEarring());
            UiTheme.Button(_content, "Language: switch en/es", () => { var loc = Localization.Instance; if (loc != null) loc.SetLocale(loc.Current == "en" ? "es" : "en"); });

            // Live toggles + values (the form).
            OverlayUi.Body(_content, "Toggles & values", 20, Section);
            UiTheme.Toggle(_content, "God mode (95% damage cut)", _godMode,
                v => { _godMode = v; PlayerBody()?.SetDamageReduction(v ? 0.95f : 0f); });
            var hazards = FindObjectOfType<EnvironmentHazardController>();
            UiTheme.Toggle(_content, "Environmental hazards", hazards == null || hazards.enabled,
                v => { var h = FindObjectOfType<EnvironmentHazardController>(); if (h != null) h.enabled = v; });
            UiTheme.Slider(_content, "Time scale", Time.timeScale, 0.1f, 3f, v => Time.timeScale = v);

            // Set player affinity (drives the elemental-matchup and the hazard exemptions).
            OverlayUi.Body(_content, "Set player affinity", 18, new Color(0.80f, 0.82f, 0.86f));
            UiTheme.Button(_content, "None", () => { PlayerBody()?.ClearAffinity(); Rebuild(); }, 150, 44);
            UiTheme.Button(_content, "Fire", () => SetAffinity(Element.Fire), 150, 44);
            UiTheme.Button(_content, "Water", () => SetAffinity(Element.Water), 150, 44);
            UiTheme.Button(_content, "Earth", () => SetAffinity(Element.Earth), 150, 44);
            UiTheme.Button(_content, "Air", () => SetAffinity(Element.Air), 150, 44);
        }

        private void SetAffinity(Element e) { PlayerBody()?.SetAffinity(e); Rebuild(); }

        private void DiscoverAllRifts()
        {
            if (MapState.Instance == null) return;
            foreach (var rift in WorldMapLayout.Rifts) MapState.Instance.Discover(rift.Id);
            GameHud.Instance?.Toast("All leyline rifts discovered.");
        }

        private void TeleportToConcord()
        {
            var concord = FindObjectOfType<ConcordSite>();
            if (concord != null) RigTeleporter.WarpTo(concord.transform.position + Vector3.up * 1.5f);
        }

        private static Damageable PlayerBody()
        {
            var rig = RigTeleporter.Rig;
            return rig != null ? rig.GetComponentInParent<Damageable>() : null;
        }
    }
}
