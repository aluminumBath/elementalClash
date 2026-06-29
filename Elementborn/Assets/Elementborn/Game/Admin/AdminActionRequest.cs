using System;
using System.Collections.Generic;

namespace Elementborn.Game
{
    [Serializable]
    public class AdminActionRequest
    {
        public string ActionId = "";
        public Dictionary<string, string> Values = new Dictionary<string, string>();

        public string Get(string fieldId, string fallback = "")
        {
            if (Values != null && Values.TryGetValue(fieldId, out string value))
            {
                return value;
            }

            return fallback;
        }

        public int GetInt(string fieldId, int fallback = 0)
        {
            return int.TryParse(Get(fieldId), out int parsed) ? parsed : fallback;
        }

        public float GetFloat(string fieldId, float fallback = 0f)
        {
            return float.TryParse(Get(fieldId), out float parsed) ? parsed : fallback;
        }

        public bool GetBool(string fieldId, bool fallback = false)
        {
            string value = Get(fieldId);
            if (bool.TryParse(value, out bool parsed))
            {
                return parsed;
            }

            if (value == "1" || value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (value == "0" || value.Equals("no", StringComparison.OrdinalIgnoreCase) || value.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return fallback;
        }
    }
}
