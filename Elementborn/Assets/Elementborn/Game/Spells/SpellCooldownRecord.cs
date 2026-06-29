using System;

namespace Elementborn.Game
{
    [Serializable]
    public class SpellCooldownRecord
    {
        public string SpellId = "";
        public float ReadyAtUnscaledTime = 0f;
        public int TimesCast = 0;
        public bool FirstCastRewardGranted = false;
    }
}
