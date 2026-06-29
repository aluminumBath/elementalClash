using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Optional test helper. Add this to a scene and enable seedOnStart to populate
    /// the tracker with sample markers so you can verify icon assignments quickly.
    /// </summary>
    public sealed class MapMarkerSeedTester : MonoBehaviour
    {
        [SerializeField] private bool seedOnStart = false;
        [SerializeField] private Transform origin;

        private void Start()
        {
            if (seedOnStart)
            {
                Seed();
            }
        }

        [ContextMenu("Seed Test Map Markers")]
        public void Seed()
        {
            Vector3 o = origin != null ? origin.position : transform.position;

            PlayerMapMarkerTracker.ReportPlayer(o);
            PlayerMapMarkerTracker.ReportBoat(o + new Vector3(8, 0, 0), false);
            PlayerMapMarkerTracker.ReportLastRiddenCreature(o + new Vector3(16, 0, 0), "Skyotter");
            PlayerMapMarkerTracker.ReportActiveCompanion(o + new Vector3(24, 0, 0), "Raven_Parrot");
            PlayerMapMarkerTracker.ReportCamp(o + new Vector3(32, 0, 0));
            PlayerMapMarkerTracker.ReportHomeBase(o + new Vector3(40, 0, 0));
            PlayerMapMarkerTracker.ReportStorageChest(o + new Vector3(48, 0, 0));
            PlayerMapMarkerTracker.ReportCraftingStation(o + new Vector3(56, 0, 0));
            PlayerMapMarkerTracker.ReportStable(o + new Vector3(64, 0, 0));
            PlayerMapMarkerTracker.ReportQuestItem(o + new Vector3(72, 0, 0), "Pearl Compass");
            PlayerMapMarkerTracker.ReportRareItem(o + new Vector3(80, 0, 0));
            PlayerMapMarkerTracker.ReportWeapon(o + new Vector3(88, 0, 0), "Emberblade");
            PlayerMapMarkerTracker.ReportResourceNode(o + new Vector3(96, 0, 0));
            PlayerMapMarkerTracker.ReportTreasure(o + new Vector3(104, 0, 0));
            PlayerMapMarkerTracker.ReportVendorNpc(o + new Vector3(112, 0, 0), "Tide Merchant");
            PlayerMapMarkerTracker.ReportGuideNpc(o + new Vector3(120, 0, 0), "Kram");
            PlayerMapMarkerTracker.ReportTrainerNpc(o + new Vector3(128, 0, 0));
            PlayerMapMarkerTracker.ReportHealerNpc(o + new Vector3(136, 0, 0));
            PlayerMapMarkerTracker.ReportQuestGiverNpc(o + new Vector3(144, 0, 0));
            PlayerMapMarkerTracker.ReportRareEnemySighting(o + new Vector3(152, 0, 0));
            PlayerMapMarkerTracker.ReportBossLair(o + new Vector3(160, 0, 0));
            PlayerMapMarkerTracker.ReportEnemyCamp(o + new Vector3(168, 0, 0));
            PlayerMapMarkerTracker.ReportDangerZone(o + new Vector3(176, 0, 0));
            PlayerMapMarkerTracker.ReportSeaMonsterSighting(o + new Vector3(184, 0, 0));
            PlayerMapMarkerTracker.ReportDock(o + new Vector3(192, 0, 0));
            PlayerMapMarkerTracker.ReportFastTravel(o + new Vector3(200, 0, 0));
            PlayerMapMarkerTracker.ReportShrine(o + new Vector3(208, 0, 0));
            PlayerMapMarkerTracker.ReportDungeon(o + new Vector3(216, 0, 0));
            PlayerMapMarkerTracker.ReportCave(o + new Vector3(224, 0, 0));
            PlayerMapMarkerTracker.ReportPuzzle(o + new Vector3(232, 0, 0));
            PlayerMapMarkerTracker.ReportLockedDoor(o + new Vector3(240, 0, 0));
            PlayerMapMarkerTracker.ReportFishingSpot(o + new Vector3(248, 0, 0));
            PlayerMapMarkerTracker.ReportUnderwaterRuin(o + new Vector3(256, 0, 0));
            PlayerMapMarkerTracker.ReportCoralReef(o + new Vector3(264, 0, 0), "Neritha Reefwood");
            PlayerMapMarkerTracker.ReportWindCurrent(o + new Vector3(272, 0, 0));
            PlayerMapMarkerTracker.ReportCustomPin("pin_test", o + new Vector3(280, 0, 0), "Test Pin");
        }
    }
}
