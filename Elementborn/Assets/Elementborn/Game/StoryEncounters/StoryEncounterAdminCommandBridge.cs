using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// encounter.summary
    /// encounter.register
    /// pack.defeat romilus
    /// pack.defeat madrangea
    /// pack.respawn
    /// orphanage.heal
    /// donowl.sleep
    /// donowl.distract
    /// judge.warn
    /// </summary>
    public sealed class StoryEncounterAdminCommandBridge : MonoBehaviour
    {
        [SerializeField] private TimedDualLeaderPackRespawnController packController;
        [SerializeField] private CreatureOrphanageHealingService orphanage;
        [SerializeField] private DistractedSleeperMonsterController donowl;
        [SerializeField] private ThunderVoiceTerritoryGuardian judge;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed == "encounter.summary")
            {
                Debug.Log(StoryEncounterRegistry.Ensure().BuildSummary());
                return true;
            }

            if (trimmed == "encounter.register")
            {
                StoryEncounterRegistry.Ensure().RegisterJournalAndMap();
                return true;
            }

            if (trimmed.StartsWith("pack.defeat "))
            {
                if (packController != null)
                {
                    packController.NotifyLeaderDefeated(trimmed.Substring("pack.defeat ".Length).Trim());
                }
                return true;
            }

            if (trimmed == "pack.respawn")
            {
                if (packController != null)
                {
                    packController.ForceRespawnPack();
                }
                return true;
            }

            if (trimmed == "orphanage.heal")
            {
                if (orphanage != null)
                {
                    orphanage.HealRegisteredCreatures();
                }
                return true;
            }

            if (trimmed == "donowl.sleep")
            {
                if (donowl != null)
                {
                    donowl.FallAsleep();
                }
                return true;
            }

            if (trimmed == "donowl.distract")
            {
                if (donowl != null)
                {
                    donowl.BecomeDistracted();
                }
                return true;
            }

            if (trimmed == "judge.warn")
            {
                if (judge != null)
                {
                    judge.WarnTrespasser(null);
                }
                return true;
            }

            return false;
        }
    }
}
