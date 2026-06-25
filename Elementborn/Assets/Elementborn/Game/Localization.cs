using System;
using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Game-wide localization: seeds a <see cref="LocaleTable"/> with the shipped string set (English base
    /// plus a sample Spanish locale to prove switching), exposes <see cref="T"/> for the UI, switches locale,
    /// remembers the choice in PlayerPrefs, and raises <see cref="LocaleChanged"/> so open screens can rebuild.
    /// Strings not yet translated fall back to English and then to the key, so the game is always readable while the
    /// full string set is migrated incrementally.</summary>
    public sealed class Localization : MonoBehaviour
    {
        public static Localization Instance { get; private set; }
        public static event Action LocaleChanged;

        private const string PrefKey = "elementborn.locale";
        private readonly LocaleTable _table = new LocaleTable("en");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Seed();
            _table.SetLocale(PlayerPrefs.GetString(PrefKey, "en"));
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Translate a key for the current locale (falls back to English, then the key).</summary>
        public static string T(string key) => Instance != null ? Instance._table.Get(key) : key;

        public string Current => _table.Current;
        public IReadOnlyCollection<string> Locales => _table.Locales;

        public void SetLocale(string code)
        {
            if (!_table.SetLocale(code)) return;
            PlayerPrefs.SetString(PrefKey, code);
            LocaleChanged?.Invoke();
        }

        private void En(string k, string v) => _table.Set("en", k, v);
        private void Es(string k, string v) => _table.Set("es", k, v);

        private void Seed()
        {
            // English (base) — the shipped strings. More are migrated onto this table over time.
            En("menu.paused", "PAUSED");
            En("menu.resume", "Resume");
            En("menu.settings", "Settings");
            En("menu.mainMenu", "Main Menu");
            En("menu.quit", "Quit");
            En("menu.newGame", "New Game");
            En("menu.continue", "Continue");
            En("hud.loadingWorld", "Generating world");

            // Sample second locale (Spanish). Partial is fine — anything missing falls back to English.
            Es("menu.paused", "PAUSA");
            Es("menu.resume", "Reanudar");
            Es("menu.settings", "Ajustes");
            Es("menu.mainMenu", "Menú principal");
            Es("menu.quit", "Salir");
            Es("menu.newGame", "Nueva partida");
            Es("menu.continue", "Continuar");
            Es("hud.loadingWorld", "Generando mundo");
        }
    }
}
