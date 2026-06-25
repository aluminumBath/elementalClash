using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>Pure, engine-free localization table: registered locales (each a key→string map) plus a current
    /// locale, with a robust lookup that falls back to the base locale and finally to the key itself — so a missing
    /// translation degrades to readable text instead of an empty string or a crash. Deterministic and unit-tested;
    /// the <c>Localization</c> service seeds it with the shipped strings and exposes <c>T(key)</c> to the UI.</summary>
    public sealed class LocaleTable
    {
        private readonly Dictionary<string, Dictionary<string, string>> _locales =
            new Dictionary<string, Dictionary<string, string>>();
        private readonly string _baseCode;

        public string Current { get; private set; }
        public string BaseCode => _baseCode;
        public IReadOnlyCollection<string> Locales => _locales.Keys;

        public LocaleTable(string baseCode = "en")
        {
            _baseCode = string.IsNullOrEmpty(baseCode) ? "en" : baseCode;
            Current = _baseCode;
            _locales[_baseCode] = new Dictionary<string, string>();
        }

        public void Set(string code, string key, string value)
        {
            if (string.IsNullOrEmpty(code) || key == null) return;
            if (!_locales.TryGetValue(code, out var d)) { d = new Dictionary<string, string>(); _locales[code] = d; }
            d[key] = value;
        }

        public bool HasLocale(string code) => code != null && _locales.ContainsKey(code);

        public bool SetLocale(string code)
        {
            if (code == null || !_locales.ContainsKey(code)) return false;
            Current = code;
            return true;
        }

        /// <summary>Current locale → base locale → the key itself.</summary>
        public string Get(string key)
        {
            if (key == null) return "";
            if (_locales.TryGetValue(Current, out var cur) && cur.TryGetValue(key, out var v)) return v;
            if (_locales.TryGetValue(_baseCode, out var bas) && bas.TryGetValue(key, out var b)) return b;
            return key;
        }

        public bool Has(string key)
        {
            if (key == null) return false;
            return (_locales.TryGetValue(Current, out var c) && c.ContainsKey(key)) ||
                   (_locales.TryGetValue(_baseCode, out var b) && b.ContainsKey(key));
        }
    }
}
