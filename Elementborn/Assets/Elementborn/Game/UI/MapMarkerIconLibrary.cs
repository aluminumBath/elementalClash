using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Simple icon resolver for map markers.
    /// Assign sprites to this on a shared map UI object, then call Resolve(marker).
    /// </summary>
    public sealed class MapMarkerIconLibrary : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Sprite playerIcon;
        [SerializeField] private Sprite boatIcon;
        [SerializeField] private Sprite companionIcon;
        [SerializeField] private Sprite currentObjectiveIcon;
        [SerializeField] private Sprite unknownIcon;

        [Header("Player-Owned / Utility")]
        [SerializeField] private Sprite campIcon;
        [SerializeField] private Sprite homeBaseIcon;
        [SerializeField] private Sprite storageChestIcon;
        [SerializeField] private Sprite craftingStationIcon;
        [SerializeField] private Sprite stableIcon;

        [Header("Items / Loot")]
        [SerializeField] private Sprite questItemIcon;
        [SerializeField] private Sprite rareItemIcon;
        [SerializeField] private Sprite weaponIcon;
        [SerializeField] private Sprite resourceNodeIcon;
        [SerializeField] private Sprite treasureIcon;

        [Header("NPCs")]
        [SerializeField] private Sprite vendorIcon;
        [SerializeField] private Sprite guideIcon;
        [SerializeField] private Sprite trainerIcon;
        [SerializeField] private Sprite healerIcon;
        [SerializeField] private Sprite questGiverIcon;

        [Header("Threats")]
        [SerializeField] private Sprite rareEnemyIcon;
        [SerializeField] private Sprite bossLairIcon;
        [SerializeField] private Sprite enemyCampIcon;
        [SerializeField] private Sprite dangerZoneIcon;
        [SerializeField] private Sprite seaMonsterIcon;

        [Header("Places / Discoveries")]
        [SerializeField] private Sprite dockIcon;
        [SerializeField] private Sprite fastTravelIcon;
        [SerializeField] private Sprite shrineIcon;
        [SerializeField] private Sprite dungeonIcon;
        [SerializeField] private Sprite caveIcon;
        [SerializeField] private Sprite puzzleIcon;
        [SerializeField] private Sprite lockedDoorIcon;
        [SerializeField] private Sprite fishingSpotIcon;
        [SerializeField] private Sprite underwaterRuinIcon;
        [SerializeField] private Sprite coralReefIcon;
        [SerializeField] private Sprite windCurrentIcon;
        [SerializeField] private Sprite customPinIcon;

        [Header("Creature Traversal")]
        [SerializeField] private Sprite landCreatureIcon;
        [SerializeField] private Sprite flyingCreatureIcon;
        [SerializeField] private Sprite swimmingCreatureIcon;
        [SerializeField] private Sprite amphibiousCreatureIcon;
        [SerializeField] private Sprite burrowingCreatureIcon;

        public Sprite Resolve(TrackedMapMarkerRecord marker)
        {
            if (marker == null)
            {
                return unknownIcon;
            }

            return marker.MarkerType switch
            {
                MapMarkerType.Player => playerIcon != null ? playerIcon : unknownIcon,
                MapMarkerType.Boat => boatIcon != null ? boatIcon : unknownIcon,
                MapMarkerType.LastRiddenCreature => ResolveCreatureTraversalIcon(marker.CreatureTraversalType),
                MapMarkerType.ActiveCompanion => companionIcon != null ? companionIcon : ResolveCreatureTraversalIcon(marker.CreatureTraversalType),
                MapMarkerType.CurrentObjective => currentObjectiveIcon != null ? currentObjectiveIcon : questGiverIcon != null ? questGiverIcon : customPinIcon != null ? customPinIcon : unknownIcon,

                MapMarkerType.Camp => campIcon != null ? campIcon : unknownIcon,
                MapMarkerType.HomeBase => homeBaseIcon != null ? homeBaseIcon : unknownIcon,
                MapMarkerType.StorageChest => storageChestIcon != null ? storageChestIcon : unknownIcon,
                MapMarkerType.CraftingStation => craftingStationIcon != null ? craftingStationIcon : unknownIcon,
                MapMarkerType.Stable => stableIcon != null ? stableIcon : unknownIcon,

                MapMarkerType.QuestItem => questItemIcon != null ? questItemIcon : unknownIcon,
                MapMarkerType.RareItem => rareItemIcon != null ? rareItemIcon : unknownIcon,
                MapMarkerType.Weapon => weaponIcon != null ? weaponIcon : unknownIcon,
                MapMarkerType.ResourceNode => resourceNodeIcon != null ? resourceNodeIcon : unknownIcon,
                MapMarkerType.Treasure => treasureIcon != null ? treasureIcon : unknownIcon,

                MapMarkerType.VendorNpc => vendorIcon != null ? vendorIcon : unknownIcon,
                MapMarkerType.GuideNpc => guideIcon != null ? guideIcon : unknownIcon,
                MapMarkerType.TrainerNpc => trainerIcon != null ? trainerIcon : unknownIcon,
                MapMarkerType.HealerNpc => healerIcon != null ? healerIcon : unknownIcon,
                MapMarkerType.QuestGiverNpc => questGiverIcon != null ? questGiverIcon : unknownIcon,

                MapMarkerType.RareEnemySighting => rareEnemyIcon != null ? rareEnemyIcon : unknownIcon,
                MapMarkerType.BossLair => bossLairIcon != null ? bossLairIcon : unknownIcon,
                MapMarkerType.EnemyCamp => enemyCampIcon != null ? enemyCampIcon : unknownIcon,
                MapMarkerType.DangerZone => dangerZoneIcon != null ? dangerZoneIcon : unknownIcon,
                MapMarkerType.SeaMonsterSighting => seaMonsterIcon != null ? seaMonsterIcon : unknownIcon,

                MapMarkerType.Dock => dockIcon != null ? dockIcon : unknownIcon,
                MapMarkerType.FastTravel => fastTravelIcon != null ? fastTravelIcon : unknownIcon,
                MapMarkerType.Shrine => shrineIcon != null ? shrineIcon : unknownIcon,
                MapMarkerType.Dungeon => dungeonIcon != null ? dungeonIcon : unknownIcon,
                MapMarkerType.Cave => caveIcon != null ? caveIcon : unknownIcon,
                MapMarkerType.Puzzle => puzzleIcon != null ? puzzleIcon : unknownIcon,
                MapMarkerType.LockedDoor => lockedDoorIcon != null ? lockedDoorIcon : unknownIcon,
                MapMarkerType.FishingSpot => fishingSpotIcon != null ? fishingSpotIcon : unknownIcon,
                MapMarkerType.UnderwaterRuin => underwaterRuinIcon != null ? underwaterRuinIcon : unknownIcon,
                MapMarkerType.CoralReef => coralReefIcon != null ? coralReefIcon : unknownIcon,
                MapMarkerType.WindCurrent => windCurrentIcon != null ? windCurrentIcon : unknownIcon,

                MapMarkerType.CustomPin => customPinIcon != null ? customPinIcon : unknownIcon,
                _ => unknownIcon
            };
        }

        private Sprite ResolveCreatureTraversalIcon(CreatureTraversalType traversalType)
        {
            return traversalType switch
            {
                CreatureTraversalType.Land => landCreatureIcon != null ? landCreatureIcon : unknownIcon,
                CreatureTraversalType.Flying => flyingCreatureIcon != null ? flyingCreatureIcon : unknownIcon,
                CreatureTraversalType.Swimming => swimmingCreatureIcon != null ? swimmingCreatureIcon : unknownIcon,
                CreatureTraversalType.Amphibious => amphibiousCreatureIcon != null ? amphibiousCreatureIcon : unknownIcon,
                CreatureTraversalType.Burrowing => burrowingCreatureIcon != null ? burrowingCreatureIcon : unknownIcon,
                _ => unknownIcon
            };
        }
    }
}
