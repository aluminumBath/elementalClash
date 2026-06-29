using System;

namespace Elementborn.Game
{
    public static class BossEventHub
    {
        public static event Action<BossController> BossStarted;
        public static event Action<BossController, int> BossPhaseChanged;
        public static event Action<BossController> BossDefeated;
        public static event Action<BossController> BossReset;
        public static void RaiseBossStarted(BossController boss) => BossStarted?.Invoke(boss);
        public static void RaiseBossPhaseChanged(BossController boss, int phase) => BossPhaseChanged?.Invoke(boss, phase);
        public static void RaiseBossDefeated(BossController boss) => BossDefeated?.Invoke(boss);
        public static void RaiseBossReset(BossController boss) => BossReset?.Invoke(boss);
    }
}
