using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class TimedDualLeaderPackRespawnController : MonoBehaviour
    {
        [SerializeField] private string packId = "romilus_madrangea_pack";
        [SerializeField] private GameObject leaderA;
        [SerializeField] private GameObject leaderB;
        [SerializeField] private List<GameObject> packMembers = new List<GameObject>();
        [SerializeField] private List<Transform> respawnPoints = new List<Transform>();
        [SerializeField] private float defeatWindowSeconds = 300f;
        [SerializeField] private float respawnDelaySeconds = 5f;
        [SerializeField] private bool respawnIfOnlyOneLeaderFalls = true;

        private float leaderADefeatedAt = -1f;
        private float leaderBDefeatedAt = -1f;
        private bool packDefeated;

        public bool PackDefeated => packDefeated;

        public void NotifyLeaderDefeated(string leaderId)
        {
            if (packDefeated)
            {
                return;
            }

            string normalized = (leaderId ?? "").ToLowerInvariant();
            if (normalized.Contains("romilus") || normalized.Contains("leadera") || normalized == "a")
            {
                leaderADefeatedAt = Time.time;
                StoryEncounterProgressTracker.Ensure().RecordLeaderDefeated(packId, true, Time.time);
            }
            else if (normalized.Contains("madrangea") || normalized.Contains("leaderb") || normalized == "b")
            {
                leaderBDefeatedAt = Time.time;
                StoryEncounterProgressTracker.Ensure().RecordLeaderDefeated(packId, false, Time.time);
            }

            EvaluateLeaderWindow();
        }

        public void NotifyRomilusDefeated() => NotifyLeaderDefeated("romilus");
        public void NotifyMadrangeaDefeated() => NotifyLeaderDefeated("madrangea");

        public void EvaluateLeaderWindow()
        {
            bool leaderADown = leaderADefeatedAt >= 0f;
            bool leaderBDown = leaderBDefeatedAt >= 0f;

            if (leaderADown && leaderBDown && Mathf.Abs(leaderADefeatedAt - leaderBDefeatedAt) <= defeatWindowSeconds)
            {
                MarkPackDefeated();
                return;
            }

            if (respawnIfOnlyOneLeaderFalls)
            {
                StopAllCoroutines();
                StartCoroutine(RespawnIfWindowExpires());
            }
        }

        public void MarkPackDefeated()
        {
            packDefeated = true;
            StopAllCoroutines();
            NotificationFeed.Post("Romilus and Madrangea have both fallen. The wolf pack will not return.", NotificationType.Quest);
            StoryEncounterProgressTracker.Ensure().ResolveEncounter(packId, "Both wolf pack leaders were defeated inside the five-minute window.");
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.NeutralCentralCity, CapitalPressureType.HiddenThreat, -10, "The wolf pack leaders were defeated within five minutes.");
        }

        public void ForceRespawnPack()
        {
            if (packDefeated)
            {
                return;
            }

            leaderADefeatedAt = -1f;
            leaderBDefeatedAt = -1f;

            SetActiveSafe(leaderA, true);
            SetActiveSafe(leaderB, true);

            for (int i = 0; i < packMembers.Count; i++)
            {
                GameObject member = packMembers[i];
                if (member == null)
                {
                    continue;
                }

                if (i < respawnPoints.Count && respawnPoints[i] != null)
                {
                    member.transform.position = respawnPoints[i].position;
                    member.transform.rotation = respawnPoints[i].rotation;
                }

                member.SetActive(true);
            }

            NotificationFeed.Post("The wolf pack regroups around Romilus and Madrangea.", NotificationType.Info);
            StoryEncounterProgressTracker.Ensure().SetRespawning(packId, "The pack respawned because both leaders were not defeated within five minutes.");
        }

        private IEnumerator RespawnIfWindowExpires()
        {
            yield return new WaitForSeconds(defeatWindowSeconds + respawnDelaySeconds);

            if (packDefeated)
            {
                yield break;
            }

            bool bothDown = leaderADefeatedAt >= 0f && leaderBDefeatedAt >= 0f &&
                            Mathf.Abs(leaderADefeatedAt - leaderBDefeatedAt) <= defeatWindowSeconds;

            if (!bothDown)
            {
                ForceRespawnPack();
            }
        }

        private void SetActiveSafe(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
