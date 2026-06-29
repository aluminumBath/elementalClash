using UnityEngine;

namespace Elementborn.Game
{
    public sealed class DistractedSleeperMonsterController : MonoBehaviour
    {
        [SerializeField] private string monsterName = "Donowl";
        [SerializeField] private float sleepChancePerCheck = 0.25f;
        [SerializeField] private float distractionChancePerCheck = 0.35f;
        [SerializeField] private float checkIntervalSeconds = 6f;
        [SerializeField] private float sleepDurationSeconds = 4f;
        [SerializeField] private bool isAsleep;
        [SerializeField] private bool isDistracted;

        private float nextCheckTime;
        private float wakeTime;

        public bool IsAsleep => isAsleep;
        public bool IsDistracted => isDistracted;

        private void Update()
        {
            if (isAsleep)
            {
                if (Time.time >= wakeTime)
                {
                    WakeUp();
                }
                return;
            }

            if (Time.time < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.time + checkIntervalSeconds;

            if (Random.value <= sleepChancePerCheck)
            {
                FallAsleep();
            }
            else if (Random.value <= distractionChancePerCheck)
            {
                BecomeDistracted();
            }
            else
            {
                isDistracted = false;
            }
        }

        public void FallAsleep()
        {
            isAsleep = true;
            isDistracted = false;
            wakeTime = Time.time + sleepDurationSeconds;
            NotificationFeed.Post($"{monsterName} falls asleep mid-encounter.", NotificationType.Info);
        }

        public void WakeUp()
        {
            isAsleep = false;
            NotificationFeed.Post($"{monsterName} wakes up, confused but still extremely strong.", NotificationType.Info);
        }

        public void BecomeDistracted()
        {
            isDistracted = true;
            NotificationFeed.Post($"{monsterName} is distracted by something shiny or suspiciously snack-shaped.", NotificationType.Info);
        }
    }
}
