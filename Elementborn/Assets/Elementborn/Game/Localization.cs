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

        /// <summary>Return the live instance, creating one if the scene never spawned it (e.g. the rescue/bootstrap
        /// scene). Lets locale switching work everywhere instead of silently no-oping when Instance is null.</summary>
        public static Localization Ensure()
        {
            if (Instance != null) return Instance;
            var found = FindObjectOfType<Localization>();
            if (found != null) return found;
            return new GameObject(nameof(Localization)).AddComponent<Localization>();
        }

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

            // Main menu.
            En("menu.subtitle", "Channel the elements. Mend the Convergence.");
            En("menu.saveSlots", "Save Slots");
            En("menu.howToPlay", "How to Play");
            En("menu.credits", "Credits");
            En("menu.noSave", "No saved journey yet");
            En("menu.channeler", "Channeler");
            En("menu.confluence", "Confluence");
            En("menu.quitConfirmTitle", "Quit Elementborn?");
            En("menu.quitConfirmBody", "Leave the world and close the game?");
            En("menu.quitDesktop", "Quit to Desktop");
            En("menu.back", "Back");
            En("menu.designBy", "Design by Steele");

            // How to Play.
            En("howto.intro", "Channel the four elements — Fire, Water, Earth, Air — to mend the Convergence.");
            En("howto.controls", "Controls");
            En("howto.move", "Move: WASD / left stick      Look: mouse / right stick");
            En("howto.attack", "Channel & attack: hand triggers (mouse buttons on flat)");
            En("howto.panels1", "Equipment: V      Crafting: B      Summon Beacon: U");
            En("howto.panels2", "Wardrobe: J      Home: H      Map: M      Achievements: K");
            En("howto.close", "Close any panel: Esc");

            // Credits.
            En("credits.tagline", "An original elemental-combat RPG.");
            En("credits.by", "Design & Development by Steele.");
            En("credits.tech", "Built with Unity 6 and the Universal Render Pipeline.");
            En("credits.thanks", "Thank you for playing.");

            // HUD currency abbreviations.
            En("hud.dia", "Dia"); En("hud.sap", "Sap"); En("hud.eme", "Eme"); En("hud.rub", "Rub"); En("hud.sil", "Sil");

            // Shared UI chrome (every OverlayUi panel).
            En("ui.close", "Close (Esc)");

            // Overlay titles.
            En("ui.title.inventory", "Inventory");
            En("ui.title.crafting", "Crafting");
            En("ui.title.equipment", "Equipment");
            En("ui.title.wardrobe", "Wardrobe");
            En("ui.title.home", "Home");
            En("ui.title.achievements", "Achievements");
            En("ui.title.character", "Character");
            En("ui.title.quests", "Quests");
            En("ui.title.summon", "Summon Beacon");

            // Settings screen.
            En("settings.master", "Master volume");
            En("settings.music", "Music volume");
            En("settings.sfx", "SFX volume");
            En("settings.sensitivity", "Mouse sensitivity");
            En("settings.fov", "Field of view");
            En("settings.invertY", "Invert look (Y)");
            En("settings.vignette", "Comfort vignette (VR)");
            En("settings.controls", "Controls…");
            En("ui.closeShort", "Close");
            En("ui.back", "< Back");

            // Home menu.
            En("home.workshop", "Open Workshop  —  Crafting");
            En("home.enchanting", "Open Enchanting Table  —  Equipment");
            En("home.wardrobe", "Open Wardrobe  —  change your look");
            En("home.storage", "Open Storage  —  deposit & withdraw");
            En("home.stable", "Open Stable  —  house your creatures");
            En("home.storageTitle", "Home Storage");

            // Sample second locale (Spanish). Partial is fine — anything missing falls back to English.
            Es("menu.paused", "PAUSA");
            Es("menu.resume", "Reanudar");
            Es("menu.settings", "Ajustes");
            Es("menu.mainMenu", "Menú principal");
            Es("menu.quit", "Salir");
            Es("menu.newGame", "Nueva partida");
            Es("menu.continue", "Continuar");
            Es("hud.loadingWorld", "Generando mundo");

            Es("menu.subtitle", "Canaliza los elementos. Repara la Convergencia.");
            Es("menu.saveSlots", "Partidas guardadas");
            Es("menu.howToPlay", "Cómo jugar");
            Es("menu.credits", "Créditos");
            Es("menu.noSave", "Aún no hay viaje guardado");
            Es("menu.channeler", "Canalizador");
            Es("menu.confluence", "Confluencia");
            Es("menu.quitConfirmTitle", "¿Salir de Elementborn?");
            Es("menu.quitConfirmBody", "¿Abandonar el mundo y cerrar el juego?");
            Es("menu.quitDesktop", "Salir al escritorio");
            Es("menu.back", "Atrás");
            Es("menu.designBy", "Diseño por Steele");

            Es("howto.intro", "Canaliza los cuatro elementos — Fuego, Agua, Tierra, Aire — para reparar la Convergencia.");
            Es("howto.controls", "Controles");
            Es("howto.move", "Mover: WASD / stick izq.      Mirar: ratón / stick der.");
            Es("howto.attack", "Canalizar y atacar: gatillos de mano (botones del ratón en plano)");
            Es("howto.panels1", "Equipo: V      Fabricación: B      Faro de Invocación: U");
            Es("howto.panels2", "Vestuario: J      Hogar: H      Mapa: M      Logros: K");
            Es("howto.close", "Cerrar cualquier panel: Esc");

            Es("credits.tagline", "Un RPG original de combate elemental.");
            Es("credits.by", "Diseño y desarrollo por Steele.");
            Es("credits.tech", "Hecho con Unity 6 y el Universal Render Pipeline.");
            Es("credits.thanks", "Gracias por jugar.");

            Es("hud.dia", "Dia"); Es("hud.sap", "Zaf"); Es("hud.eme", "Esm"); Es("hud.rub", "Rub"); Es("hud.sil", "Pla");

            Es("ui.close", "Cerrar (Esc)");

            Es("ui.title.inventory", "Inventario");
            Es("ui.title.crafting", "Fabricación");
            Es("ui.title.equipment", "Equipo");
            Es("ui.title.wardrobe", "Vestuario");
            Es("ui.title.home", "Hogar");
            Es("ui.title.achievements", "Logros");
            Es("ui.title.character", "Personaje");
            Es("ui.title.quests", "Misiones");
            Es("ui.title.summon", "Faro de Invocación");

            Es("settings.master", "Volumen general");
            Es("settings.music", "Volumen de música");
            Es("settings.sfx", "Volumen de efectos");
            Es("settings.sensitivity", "Sensibilidad del ratón");
            Es("settings.fov", "Campo de visión");
            Es("settings.invertY", "Invertir vista (Y)");
            Es("settings.vignette", "Viñeta de confort (RV)");
            Es("settings.controls", "Controles…");
            Es("ui.closeShort", "Cerrar");
            Es("ui.back", "< Atrás");

            Es("home.workshop", "Abrir Taller  —  Fabricación");
            Es("home.enchanting", "Abrir Mesa de Encantamiento  —  Equipo");
            Es("home.wardrobe", "Abrir Vestuario  —  cambia tu aspecto");
            Es("home.storage", "Abrir Almacén  —  depositar y retirar");
            Es("home.stable", "Abrir Establo  —  aloja tus criaturas");
            Es("home.storageTitle", "Almacén del hogar");
        }
    }
}
