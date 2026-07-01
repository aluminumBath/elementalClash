namespace Elementborn.Core
{
    /// <summary>
    /// Core-side localization facade so pure data catalogs can localize their own display strings without depending
    /// on the Game layer. Catalogs call <see cref="T"/> with their English string as the key; the Game
    /// <c>Localization</c> component installs the live <see cref="LocaleTable"/> at startup, so those strings resolve
    /// to the current locale. With no table installed (unit tests, or a scene without Localization), <see cref="T"/>
    /// returns the key unchanged — i.e. the original English — so Core stays engine-free and fully testable.
    /// </summary>
    public static class Loc
    {
        private static LocaleTable _table;

        /// <summary>Point the facade at the live locale table (called once by the Game Localization component).</summary>
        public static void Install(LocaleTable table) => _table = table;

        /// <summary>Translate a key — or an English string used as its own key — for the current locale.</summary>
        public static string T(string key) => _table != null ? _table.Get(key) : key;
    }
}
