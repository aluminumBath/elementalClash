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
            Loc.Install(_table); // let pure Core catalogs localize their display names through the same table
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
            En("summon.freeDaily", "Free daily summon  ★");
            En("character.spendPoint", "Spend a point on ");

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
            Es("summon.freeDaily", "Invocación diaria gratis  ★");
            Es("character.spendPoint", "Gastar un punto en ");

            // ---- Catalog content (keyed by the English string; English falls back to the key) ----
            // Home / crafting additions (Core: HomesteadCatalog).
            Es("Storage Chest", "Cofre de almacenamiento");
            Es("Workshop", "Taller");
            Es("Wardrobe", "Vestuario");
            Es("Creature Stable", "Establo de criaturas");
            Es("Enchanting Table", "Mesa de encantamiento");
            Es("Garden", "Jardín");
            Es("Stash items you don't want to carry.", "Guarda objetos que no quieras llevar.");
            Es("Craft and combine materials into gear.", "Fabrica y combina materiales en equipo.");
            Es("Change your look — appearance only, never your element.", "Cambia tu aspecto: solo apariencia, nunca tu elemento.");
            Es("House, rest, and manage your tamed creatures.", "Aloja, descansa y gestiona tus criaturas domadas.");
            Es("Imbue worn armor with an element.", "Imbuye tu armadura con un elemento.");
            Es("Grow plants and harvest ingredients.", "Cultiva plantas y cosecha ingredientes.");

            // Items (Core: ItemCatalog) — names.
            Es("Arrow", "Flecha");
            Es("Fire Arrow", "Flecha de Fuego");
            Es("Water Arrow", "Flecha de Agua");
            Es("Earth Arrow", "Flecha de Tierra");
            Es("Air Arrow", "Flecha de Aire");
            Es("Bottled Updraft", "Corriente Embotellada");
            Es("Briny Deep-jelly", "Gelatina Salobre");
            Es("Elemental Charm", "Amuleto Elemental");
            Es("Elixir of Vigor", "Elixir de Vigor");
            Es("Ember Shard", "Fragmento de Brasa");
            Es("Healing Tonic", "Tónico Curativo");
            Es("Hide", "Piel");
            Es("Iridescent Beetle", "Escarabajo Iridiscente");
            Es("Iron Helm", "Yelmo de Hierro");
            Es("Keelwood Splinter", "Astilla de Quillamadera");
            Es("Loamy Compost Truffle", "Trufa de Compost");
            Es("Midnight Sunflower Seeds", "Semillas de Girasol de Medianoche");
            Es("Old Relic", "Reliquia Antigua");
            Es("Ore-marrow Bone", "Hueso de Médula Mineral");
            Es("Prism Shard", "Fragmento de Prisma");
            Es("River Pearl", "Perla de Río");
            Es("Stamina Draught", "Brebaje de Aguante");
            Es("Stormwarden's Token", "Ficha del Guardatormentas");
            Es("Sturdy Boots", "Botas Resistentes");
            Es("Tideglass Draught", "Brebaje de Vidrio de Marea");
            Es("Tough Leather", "Cuero Endurecido");
            Es("Warding Cloak", "Capa Protectora");

            // Quests (Core: QuestCatalog) — titles + summaries.
            Es("A Wild Start", "Un comienzo salvaje");
            Es("A Gentle Hand", "Una mano amable");
            Es("Word to Kiana", "Recado para Kiana");
            Es("Pelts for the Tanner", "Pieles para el curtidor");
            Es("First Channeling", "Primera canalización");
            Es("Answer the Beacon", "Responde al faro");
            Es("Claim the Featured", "Reclama al destacado");
            Es("Willow wants to see you handle yourself out there before pointing you to anything rarer.",
               "Willow quiere verte defenderte ahí fuera antes de indicarte algo más raro.");
            Es("Kiana thinks you're ready to win a creature over rather than just fight it.",
               "Kiana cree que estás listo para ganarte a una criatura en vez de solo luchar contra ella.");
            Es("Kram needs word carried to Kiana — and wants to know you can earn your keep on the way.",
               "Kram necesita llevar un recado a Kiana, y quiere saber que puedes ganarte el sustento por el camino.");
            Es("Willow's tanner friend will pay well for fresh hides.",
               "El amigo curtidor de Willow pagará bien por pieles frescas.");
            Es("Before anything else, Willow wants to see you channel your element.",
               "Antes que nada, Willow quiere verte canalizar tu elemento.");

            // Creatures (Core: CreatureCatalog) — names.
            Es("Fire Dragon", "Dragón de Fuego");
            Es("Water Dragon", "Dragón de Agua");
            Es("Earth Dragon", "Dragón de Tierra");
            Es("Air Dragon", "Dragón de Aire");
            Es("Mermaid", "Sirena");
            Es("Earth Mole", "Topo de Tierra");
            Es("Earth Cat", "Gato de Tierra");
            Es("Air Dragonfly", "Libélula de Aire");
            Es("Air Jellyfish", "Medusa de Aire");
            Es("Horse", "Caballo");
            Es("Web Spider", "Araña Tejedora");
            Es("Water Cat", "Gato de Agua");
            Es("Ice Cat", "Gato de Hielo");
            Es("Phoenix", "Fénix");
            Es("Storm Squirrel", "Ardilla de Tormenta");
            Es("Earth Hound", "Sabueso de Tierra");
            Es("River Eel", "Anguila de Río");
            Es("Shore Crab", "Cangrejo de Costa");
            Es("Forest Monkey", "Mono del Bosque");
            Es("Marsh Crocodile", "Cocodrilo del Pantano");
            Es("Swamp Snake", "Serpiente del Pantano");
            Es("Roc", "Roc");
            Es("Thunderbird", "Pájaro del Trueno");
            Es("Plains Rhino", "Rinoceronte de las Llanuras");
            Es("Jungle Tiger", "Tigre de la Selva");
            Es("Skyotter", "Nutria del Cielo");
            Es("Goldkoi", "Koi Dorado");
            Es("Gillcloak", "Mantobranquia");
            Es("Skimfin", "Aleta Rasante");
            Es("Glidewisp", "Fatuo Planeador");
            Es("Skytyrant", "Tirano del Cielo");
            Es("Ridgewing", "Alacresta");
            Es("Tidewarden", "Guardián de la Marea");
            Es("Direstalker", "Acechador Feroz");
            Es("Storm Wolf", "Lobo de Tormenta");
            Es("Voltfang Wolf", "Lobo Voltcolmillo");
            Es("Ancient Stag", "Ciervo Ancestral");
            Es("Azurewing Knight", "Caballero Alazul");
            Es("Bonebound Behemoth", "Behemot Óseo");
            Es("Coral Leviathan", "Leviatán de Coral");
            Es("Embercrest Kitebeast", "Bestia Cometa de Brasa");
            Es("Unknown", "Desconocido");
        }
    }
}
