using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Bridges world data into the scene: spawns WeaponPickup prefabs at weapon-cache POIs and,
    /// optionally, enemy prefabs by each POI's suggested count. Needs prefabs assigned and a
    /// map->world scale; the sculpted terrain these sit on is built in the editor / Blender.
    /// </summary>
    public sealed class WorldSpawnPlacer : MonoBehaviour
    {
        [SerializeField] private WorldMapController worldSource;
        [SerializeField] private WeaponPickup weaponPickupPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject civilianPrefab;
        [SerializeField] private int civiliansPerTown = 4;
        [SerializeField] private GameObject creaturePrefab;
        [SerializeField] private int creaturesPerRegion = 2;
        [Tooltip("Chance per region to also spawn a rare, tameable companion.")]
        [SerializeField] private float companionSpawnChance = 0.5f;
        [SerializeField] private LurePickup lurePickupPrefab;
        [SerializeField] private float mapToWorldScale = 1f;
        [SerializeField] private float groundY = 0f;
        [Tooltip("If a Terrain is active, drop spawns onto its surface instead of using groundY.")]
        [SerializeField] private bool snapToTerrain = true;
        [SerializeField] private bool spawnOnStart = false;

        private void Start()
        {
            if (spawnOnStart) Place();
        }

        public void Place() => Place(worldSource != null ? worldSource.World : null);

        public void Place(WorldMap world)
        {
            if (world == null) return;
            foreach (var region in world.Regions)
            {
                var rng = new SystemRandomSource(region.Id.GetHashCode());
                foreach (var poi in region.Pois)
                {
                    Vector3 at = ToWorld(poi.MapPosition);

                    if (poi.HasWeaponCache && weaponPickupPrefab != null)
                    {
                        var pickup = Instantiate(weaponPickupPrefab, at, Quaternion.identity, transform);
                        pickup.Configure(poi.WeaponType, poi.WeaponMaterial);
                    }

                    if (enemyPrefab != null)
                        for (int e = 0; e < poi.EnemySpawnCount; e++)
                        {
                            Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
                            var enemy = Instantiate(enemyPrefab, at + offset, Quaternion.identity, transform);
                            var ec = enemy.GetComponent<EnemyController>();
                            if (ec != null)
                            {
                                var plan = EnemyComposition.Pick(region.Biome, region.DangerLevel, rng);
                                ec.Configure(plan.Kind, plan.Faction, plan.Element);
                            }
                        }

                    if (civilianPrefab != null && IsTown(poi.Type))
                        for (int c = 0; c < civiliansPerTown; c++)
                        {
                            Vector3 offset = new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));
                            var civ = Instantiate(civilianPrefab, at + offset, Quaternion.identity, transform);
                            var fm = civ.GetComponent<FactionMember>();
                            if (fm == null) fm = civ.AddComponent<FactionMember>();
                            fm.Configure(Faction.Civilian, null);
                        }

                    if (lurePickupPrefab != null && IsLureSpot(poi.Type))
                    {
                        var lure = Instantiate(lurePickupPrefab, at, Quaternion.identity, transform);
                        lure.Configure(Wildlife.Pick(region.Biome, rng));
                    }
                }

                if (creaturePrefab != null)
                    for (int c = 0; c < creaturesPerRegion; c++)
                    {
                        Vector2 off = Random.insideUnitCircle * region.Radius;
                        Vector3 cpos = ToWorld(region.MapPosition + off);
                        var creatureGo = Instantiate(creaturePrefab, cpos, Quaternion.identity, transform);
                        var cc = creatureGo.GetComponent<CreatureController>();
                        if (cc != null) cc.Configure(Wildlife.Pick(region.Biome, rng));
                    }

                if (creaturePrefab != null && rng.NextUnit() < companionSpawnChance)
                {
                    var companion = CompanionSpawns.For(region.Biome, rng);
                    if (companion.HasValue)
                    {
                        Vector2 off = Random.insideUnitCircle * region.Radius;
                        Vector3 cpos = ToWorld(region.MapPosition + off);
                        var go = Instantiate(creaturePrefab, cpos, Quaternion.identity, transform);
                        var wc = go.GetComponent<CreatureController>();
                        if (wc != null) wc.Configure(companion.Value); // wild + tameable
                    }
                }
            }

            // Concord: the Convergence Tower, the diplomat, and the inciting blast, planted at the capital city.
            var capital = world.Capital();
            if (capital != null)
            {
                var hub = new GameObject("Concord");
                hub.transform.SetParent(transform);
                hub.transform.position = ToWorld(capital.MapPosition);
                hub.AddComponent<ConcordSite>();
            }
        }

        private static bool IsTown(PoiType t) => t == PoiType.City || t == PoiType.Village || t == PoiType.Market;

        private static bool IsLureSpot(PoiType t) => t == PoiType.Market || t == PoiType.Shrine || t == PoiType.Camp;

        private Vector3 ToWorld(Vector2 mapPos)
        {
            var p = new Vector3(mapPos.x * mapToWorldScale, groundY, mapPos.y * mapToWorldScale);
            if (snapToTerrain) p.y = TerrainHeight.Sample(p);
            return p;
        }
    }
}
