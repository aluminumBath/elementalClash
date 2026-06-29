using System;

namespace Elementborn.Game
{
    [Serializable]
    public class PlayerAbilityRecord
    {
        public string AbilityId = "";
        public AbilityUnlockSource UnlockSource = AbilityUnlockSource.Unknown;
        public bool IsUnlocked = true;
        public bool IsNew = true;
        public int TimesUsed = 0;
        public float LastUsedAtUnscaledTime = -1f;
    }
}
