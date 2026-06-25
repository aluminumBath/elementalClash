using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The instanced interior a <see cref="SiteEntrance"/> opens into. Entering teleports the player to a
    /// reusable pocket room (built far off-world, near sea level so the altitude/pressure hazards stay quiet),
    /// populates it from the site's <see cref="SitePayload"/> — a boss to fight, a creature to tame, treasure to
    /// claim, or lore to read — and drops an exit portal. Leaving teleports the player back to the entrance and
    /// clears the room. Built on the same teleport path as fast travel (<see cref="RigTeleporter"/>) and the
    /// Arena's enemy spawn; bosses/dressing models are wired in later. Spawned by the bootstrap scene.</summary>
    public sealed class SiteInteriorController : MonoBehaviour
    {
        public static SiteInteriorController Instance { get; private set; }

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject creaturePrefab;

        // Far off-world but near sea level, so the player stands on the room floor without tripping cold/pressure.
        private static readonly Vector3 PocketOrigin = new Vector3(50000f, 0f, 50000f);
        private const float RoomHalf = 16f;
        // A flooded interior's water surface sits this far above the pocket floor — high enough that the
        // player stands well below it and the pressure ramp reaches a punishing (not instant-kill) level.
        private const float SubmergedSurfaceLocalY = 100f;

        private Transform _room;       // built once, reused
        private Transform _contents;   // per-visit; destroyed on exit
        private Vector3 _returnPosition;
        private bool _inside;

        public bool IsInside => _inside;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Enter(SiteInfo info, Vector3 returnPosition)
        {
            if (_inside || RigTeleporter.Rig == null) return;
            _returnPosition = returnPosition;
            EnsureRoom();
            PopulateContents(info);
            RigTeleporter.WarpTo(PocketOrigin + new Vector3(0f, 1.2f, -8f)); // back of the entrance hall
            _inside = true;
            GameHud.Instance?.Toast("Entered " + info.DisplayName);
        }

        public void Exit()
        {
            if (!_inside) return;
            _inside = false;
            if (_contents != null) Destroy(_contents.gameObject);
            RigTeleporter.WarpTo(_returnPosition);
            GameHud.Instance?.Toast("You return to the surface world.");
        }

        private void EnsureRoom()
        {
            if (_room != null) return;
            var room = new GameObject("SiteInterior");
            room.transform.SetParent(transform, false);
            room.transform.position = PocketOrigin;
            _room = room.transform;

            var wall = new Color(0.26f, 0.26f, 0.30f);
            var floor = new Color(0.32f, 0.32f, 0.36f);
            const float H = 8f, Y = 4f; // wall height / centre height

            // A short approach, not a bare box: entrance hall -> a pinched corridor -> the boss arena.
            // Footprint x[-12,12], z[-12,52]; centre doorways (x[-3,3]) link the three spaces.
            Slab(new Vector3(24f, 1f, 64f), new Vector3(0f, -0.5f, 20f), floor); // floor

            Slab(new Vector3(1f, H, 64f), new Vector3(-12f, Y, 20f), wall);  // outer left
            Slab(new Vector3(1f, H, 64f), new Vector3(12f, Y, 20f), wall);   // outer right
            Slab(new Vector3(24f, H, 1f), new Vector3(0f, Y, -12f), wall);   // back (entrance)
            Slab(new Vector3(24f, H, 1f), new Vector3(0f, Y, 52f), wall);    // front (arena)

            Slab(new Vector3(9f, H, 1f), new Vector3(-7.5f, Y, 8f), wall);   // hall -> corridor threshold
            Slab(new Vector3(9f, H, 1f), new Vector3(7.5f, Y, 8f), wall);
            Slab(new Vector3(1f, H, 16f), new Vector3(-3f, Y, 16f), wall);   // corridor walls
            Slab(new Vector3(1f, H, 16f), new Vector3(3f, Y, 16f), wall);
            Slab(new Vector3(9f, H, 1f), new Vector3(-7.5f, Y, 24f), wall);  // corridor -> arena threshold
            Slab(new Vector3(9f, H, 1f), new Vector3(7.5f, Y, 24f), wall);

            AddLight(new Vector3(0f, 7f, -4f), 42f); // entrance
            AddLight(new Vector3(0f, 7f, 16f), 28f); // corridor
            AddLight(new Vector3(0f, 7f, 40f), 60f); // arena
        }

        private void PopulateContents(SiteInfo info)
        {
            var contents = new GameObject("Contents");
            contents.transform.SetParent(_room, false);
            _contents = contents.transform;
            Vector3 focus = PocketOrigin + new Vector3(0f, 1f, 40f); // the boss arena, at the far end

            switch (info.Payload)
            {
                case SitePayload.Boss: SpawnBoss(focus, info, contents.transform); break;
                case SitePayload.RareCreature: SpawnCreature(focus, contents.transform); break;
                case SitePayload.Treasure: SpawnProp(focus, contents.transform, new Vector3(1.6f, 1f, 1.1f),
                    new Color(0.85f, 0.70f, 0.30f), "Treasure").AddComponent<SiteTreasure>(); break;
                default: SpawnProp(focus, contents.transform, new Vector3(1.2f, 3f, 0.6f),
                    new Color(0.72f, 0.70f, 0.62f), "Monument"); GameHud.Instance?.Toast(info.Lore); break;
            }

            // Underwater sites (the Sunken Gate) flood: a water body set high overhead makes the player
            // deep, so the pressure hazard bites unless they're attuned to Water/Earth.
            if (info.Domain == SiteDomain.Underwater) FloodRoom(contents.transform);

            // A place-setting inscription in the entrance hall, so the approach in isn't empty.
            var stele = SpawnProp(PocketOrigin + new Vector3(6f, 1.5f, 0f), contents.transform,
                new Vector3(0.6f, 3f, 0.6f), new Color(0.55f, 0.52f, 0.48f), "Inscription");
            stele.AddComponent<ApproachInscription>().Configure(ApproachLore.Line(info.Kind));

            var portal = SpawnProp(PocketOrigin + new Vector3(5f, 1.3f, 46f), contents.transform,
                new Vector3(1.4f, 2.6f, 0.4f), new Color(0.50f, 0.85f, 1f), "ExitPortal");
            portal.AddComponent<SiteExit>();
        }

        private void SpawnBoss(Vector3 at, SiteInfo info, Transform parent)
        {
            if (enemyPrefab == null) { GameHud.Instance?.Toast(info.Lore); return; }
            var boss = BossCatalog.For(info.Kind);

            var go = Instantiate(enemyPrefab, at, Quaternion.identity, parent);
            go.name = "Boss_" + boss.ModelName;
            go.transform.localScale *= boss.Scale; // a looming presence

            var ec = go.GetComponent<EnemyController>();
            if (ec != null)
            {
                // A strong archetype gives real stats/behaviour; the catalog overrides element, size, and health.
                var rng = new SystemRandomSource(info.Kind.GetHashCode());
                var plan = EnemyComposition.Pick(BiomeType.Volcano, 5, rng);
                ec.Configure(plan.Kind, plan.Faction, boss.Element);
                ec.ScaleHealth(boss.HealthMultiplier);
                var rig = RigTeleporter.Rig;
                if (rig != null) ec.SetTarget(rig);
            }

            // Wear the uploaded boss model when it's present; otherwise the scaled placeholder stands in.
            CreatureModelLibrary.AttachExternal(boss.ModelName, go);

            // A telegraphed shockwave special so the boss fights differently from a rank-and-file enemy.
            go.AddComponent<BossController>().Configure(boss.Element, boss.Name, boss.SilverReward, boss.GemReward);

            GameHud.Instance?.Toast(boss.Name + " awakens.");
        }

        private void SpawnCreature(Vector3 at, Transform parent)
        {
            if (creaturePrefab == null) return;
            var go = Instantiate(creaturePrefab, at, Quaternion.identity, parent);
            var cc = go.GetComponent<CreatureController>();
            if (cc != null) cc.Configure(Wildlife.Pick(BiomeType.Marsh, new SystemRandomSource(20260624)));
        }

        // Turns the pocket room into a submerged chamber: a pressure-bearing WaterBody set high overhead,
        // plus a translucent surface pane near the ceiling for visual feedback. Both live on the per-visit
        // contents, so leaving clears them and the next (dry) site isn't waterlogged.
        private void FloodRoom(Transform parent)
        {
            float surfaceY = PocketOrigin.y + SubmergedSurfaceLocalY;

            var bodyGo = new GameObject("InteriorWater");
            bodyGo.transform.SetParent(parent, false);
            var body = bodyGo.AddComponent<WaterBody>();
            body.Configure(new Vector2(PocketOrigin.x, PocketOrigin.z + 20f),
                           new Vector2(12f, 32f), surfaceY);

            var pane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            pane.name = "InteriorWaterSurface";
            pane.transform.SetParent(parent, false);
            pane.transform.position = PocketOrigin + new Vector3(0f, 7.6f, 20f); // a watery "lid" over the dungeon
            pane.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            pane.transform.localScale = new Vector3(24f, 64f, 1f);
            var col = pane.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var mr = pane.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(new Color(0.10f, 0.42f, 0.62f));

            GameHud.Instance?.Toast("The chamber is flooded — the depths press in.");
        }

        private GameObject SpawnProp(Vector3 at, Transform parent, Vector3 scale, Color tint, string name)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = at;
            go.transform.localScale = scale;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col); // interaction is distance-based; don't block the player
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(tint);
            return go;
        }

        private void AddLight(Vector3 localPos, float range)
        {
            var go = new GameObject("InteriorLight");
            go.transform.SetParent(_room, false);
            go.transform.localPosition = localPos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = range;
            l.intensity = 2.1f;
            l.color = new Color(1f, 0.96f, 0.9f);
        }

        private void Slab(Vector3 scale, Vector3 localPos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(_room, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(color);
            // floor + walls keep their colliders so the player stands and stays contained
        }
    }
}
