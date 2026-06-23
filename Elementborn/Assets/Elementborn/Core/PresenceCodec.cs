using System.Globalization;
using UnityEngine;

namespace Elementborn.Core
{
    /// <summary>
    /// Packs a world position into the small string a presence/status payload carries, and back. Compact
    /// (two decimals, invariant culture so a comma decimal locale can't corrupt it) and total — a malformed or
    /// empty payload simply fails to decode, which the feed treats as "not sharing". Pure and unit-tested; the
    /// Nakama presence producer uses it, but it has no Nakama dependency.
    /// </summary>
    public static class PresenceCodec
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string Encode(Vector3 p) =>
            p.x.ToString("0.##", Inv) + "," + p.y.ToString("0.##", Inv) + "," + p.z.ToString("0.##", Inv);

        public static bool TryDecode(string s, out Vector3 position)
        {
            position = default;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 3) return false;
            if (float.TryParse(parts[0], NumberStyles.Float, Inv, out float x) &&
                float.TryParse(parts[1], NumberStyles.Float, Inv, out float y) &&
                float.TryParse(parts[2], NumberStyles.Float, Inv, out float z))
            {
                position = new Vector3(x, y, z);
                return true;
            }
            return false;
        }
    }
}
