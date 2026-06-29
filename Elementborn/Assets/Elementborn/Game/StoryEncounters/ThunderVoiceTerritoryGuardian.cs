using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ThunderVoiceTerritoryGuardian : MonoBehaviour
    {
        [SerializeField] private string guardianName = "The Judge";
        [SerializeField] private float warningRadius = 18f;
        [SerializeField] private float punishmentRadius = 8f;
        [SerializeField] private int warningPressureDelta = 3;
        [SerializeField] private bool warnOnlyOnce = true;

        private bool warned;

        public void WarnTrespasser(GameObject target)
        {
            if (warnOnlyOnce && warned)
            {
                return;
            }

            warned = true;
            ElementbornAudioService.PlayAt(ElementbornSoundEventId.BossPhase, transform.position);
            NotificationFeed.Post($"{guardianName}'s neon-pink voice cracks like thunder: Leave my territory.", NotificationType.Quest);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.HiddenThreat, warningPressureDelta, "The Judge warns trespassers away from her protected area.");
        }

        public void JudgeTrespasser(GameObject target)
        {
            ElementbornAudioService.PlayAt(ElementbornSoundEventId.BossAwaken, transform.position);
            NotificationFeed.Post($"{guardianName} passes harsh judgment on the trespasser.", NotificationType.Quest);
        }

        private void Update()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= punishmentRadius)
            {
                JudgeTrespasser(player);
            }
            else if (distance <= warningRadius)
            {
                WarnTrespasser(player);
            }
        }
    }
}
